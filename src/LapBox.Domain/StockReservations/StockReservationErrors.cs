using LapBox.Domain.Common.Results;

namespace LapBox.Domain.StockReservations;

public static class StockReservationErrors
{
    public static readonly Error LaptopIdRequired =
        Error.Validation("Reservation.LaptopId", "Laptop ID is required.");

    public static readonly Error UserIdRequired =
        Error.Validation("Reservation.UserId", "User ID (IdentityId) is required.");

    public static readonly Error OrderIdRequired =
        Error.Validation("Reservation.OrderId", "Order ID is required.");

    public static readonly Error QuantityInvalid =
        Error.Validation("Reservation.Quantity", "Quantity must be greater than zero.");

    public static readonly Error NotActive =
        Error.Conflict("Reservation.NotActive", "Reservation is no longer active.");

    public static readonly Error Expired =
        Error.Conflict("Reservation.Expired", "Reservation has expired.");

    public static readonly Error CannotRelease =
        Error.Conflict("Reservation.CannotRelease", "Cannot release an inactive reservation.");

    public static readonly Error AlreadyConsumed =
        Error.Conflict("Reservation.AlreadyConsumed", "Reservation has already been consumed for an order.");

    public static readonly Error NoActiveReservations =
        Error.Validation("Reservation.NoActive", "No active reservations found for this user.");

    public static readonly Error ReservationMismatch =
        Error.Conflict("Reservation.Mismatch", "Active reservations do not match the order items.");
}