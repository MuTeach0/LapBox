using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.StockReservations.Enums;

namespace LapBox.Domain.StockReservations;
public sealed class StockReservation : AggregateRoot
{
    public Guid LaptopId { get; private set; }
    
    /// <summary>
    /// The Identity/User ID who placed the reservation.
    /// This is the user's IdentityId (not CustomerId, since Customer doesn't exist yet).
    /// Used to track reservations before Order is created.
    /// </summary>
    public Guid UserId { get; private set; }
    
    /// <summary>
    /// Optional OrderId - populated when Order is created and reservations are consumed.
    /// Null during temporary reservation phase.
    /// </summary>
    public Guid? OrderId { get; private set; }
    
    public int Quantity { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public ReservationStatus Status { get; private set; }

    private StockReservation() { }

    private StockReservation(Guid id, Guid laptopId, Guid userId, Guid? orderId, int quantity, int durationInMinutes) : base(id)
    {
        LaptopId = laptopId;
        UserId = userId;
        OrderId = orderId;
        Quantity = quantity;
        ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(durationInMinutes);
        Status = ReservationStatus.Active;
    }

    /// <summary>
    /// Creates a temporary reservation (before Order is created).
    /// OrderId will be set when the Order is actually created.
    /// Uses UserId (IdentityId) since Customer record doesn't exist yet.
    /// </summary>
    public static Result<StockReservation> CreateTemporary(Guid laptopId, Guid userId, int quantity, int durationInMinutes = 15)
    {
        if (laptopId == Guid.Empty) return StockReservationErrors.LaptopIdRequired;
        if (userId == Guid.Empty) return StockReservationErrors.UserIdRequired;
        if (quantity <= 0) return StockReservationErrors.QuantityInvalid;

        return new StockReservation(Guid.NewGuid(), laptopId, userId, null, quantity, durationInMinutes);
    }

    /// <summary>
    /// Creates a reservation linked to an existing Order.
    /// Used for backwards compatibility and direct order creation.
    /// </summary>
    public static Result<StockReservation> CreateWithOrder(Guid laptopId, Guid userId, Guid orderId, int quantity, int durationInMinutes = 15)
    {
        if (laptopId == Guid.Empty) return StockReservationErrors.LaptopIdRequired;
        if (userId == Guid.Empty) return StockReservationErrors.UserIdRequired;
        if (orderId == Guid.Empty) return StockReservationErrors.OrderIdRequired;
        if (quantity <= 0) return StockReservationErrors.QuantityInvalid;

        return new StockReservation(Guid.NewGuid(), laptopId, userId, orderId, quantity, durationInMinutes);
    }

    /// <summary>
    /// Links this reservation to an Order and marks it as consumed.
    /// Called when payment is confirmed and Order is created.
    /// </summary>
    public Result<Success> ConsumeForOrder(Guid orderId)
    {
        if (Status != ReservationStatus.Active) return StockReservationErrors.NotActive;
        if (IsExpired()) { Status = ReservationStatus.Expired; return StockReservationErrors.Expired; }
        if (OrderId.HasValue) return StockReservationErrors.AlreadyConsumed;

        OrderId = orderId;
        Status = ReservationStatus.Consumed;
        return Result.Success;
    }

    public Result<Success> AddQuantity(int quantity)
    {
        if (quantity <= 0) return StockReservationErrors.QuantityInvalid;
        if (Status != ReservationStatus.Active) return StockReservationErrors.NotActive;
        if (IsExpired())
        {
            Status = ReservationStatus.Expired;
            return StockReservationErrors.Expired;
        }

        Quantity += quantity;
        return Result.Success;
    }

    public bool IsExpired() => Status == ReservationStatus.Active && DateTimeOffset.UtcNow > ExpiresAtUtc;

    /// <summary>
    /// Consumes an active reservation (old method for backwards compatibility).
    /// Prefer ConsumeForOrder() when linking to an Order.
    /// </summary>
    public Result<Success> Consume()
    {
        if (Status != ReservationStatus.Active) return StockReservationErrors.NotActive;
        if (IsExpired()) { Status = ReservationStatus.Expired; return StockReservationErrors.Expired; }

        Status = ReservationStatus.Consumed;
        return Result.Success;
    }

    public Result<Success> Release()
    {
        if (Status != ReservationStatus.Active) return StockReservationErrors.CannotRelease;

        Status = ReservationStatus.Released;
        return Result.Success;
    }
}
