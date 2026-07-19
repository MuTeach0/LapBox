using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Orders.Enums;
using LapBox.Domain.Orders.Events;
using LapBox.Domain.Orders.ValueObjects;

namespace LapBox.Domain.Orders;

public sealed class Order : AggregateRoot
{
    public Guid UserId { get; private set; } // Slicing ID reference
    public DateTimeOffset OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; } = null!;
    public string? TrackingLabel { get; private set; }
    public Guid? AppliedPromotionId { get; private set; }
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order() { }

    private Order(Guid id, Guid userId, ShippingAddress shippingAddress, List<OrderItemPayload> initialItems)
    : base(id)
    {
        UserId = userId;
        OrderDate = DateTimeOffset.UtcNow;
        Status = OrderStatus.Placed;
        ShippingAddress = shippingAddress;
        foreach (var item in initialItems)
        {
            _orderItems.Add(new OrderItem(Id, item.LaptopId, item.Quantity, item.UnitPrice, 0));
        }
    }

   public static Result<Order> Create(Guid userId, ShippingAddress shippingAddress, List<OrderItemPayload> items)
    {
        if (items is null || items.Count == 0)
            return Error.Validation("Order.ItemsRequired", "An order must contain at least one item.");

        var order = new Order(Guid.NewGuid(), userId, shippingAddress, items);

        // نمرر الـ items مباشرة للـ Event
        order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id, userId, items));
        
        return order;
    }

    public Result<Success> AddItem(Guid laptopId, int quantity, decimal unitPrice, decimal discountAmount = 0)
    {
        if (Status != OrderStatus.Placed)
            return OrderErrors.NotPlaced;

        if (_orderItems.Any(x => x.LaptopId == laptopId))
            return OrderErrors.ItemExists;

        if (quantity <= 0)
            return OrderErrors.QuantityInvalid;

        _orderItems.Add(new OrderItem(Id, laptopId, quantity, unitPrice, discountAmount));
        return Result.Success;
    }

    public Result<Success> RemoveItem(Guid laptopId)
    {
        if (_orderItems.Count <= 1)
            return Error.Validation("Order.CannotBeEmpty", "Cannot remove the last item. Order must have at least one item.");

        var item = _orderItems.FirstOrDefault(x => x.LaptopId == laptopId);
        if (item is null)
            return OrderErrors.ItemNotFound;

        _orderItems.Remove(item);
        return Result.Success;
    }

    public decimal CalculateTotal() =>
        _orderItems.Sum(item => (item.UnitPrice - item.DiscountAmount) * item.Quantity);

    public Result<Success> UpdateStatus(OrderStatus newStatus)
    {
        if (Status == OrderStatus.Cancelled || Status == OrderStatus.Delivered)
            return OrderErrors.StatusUnchangeable;

        // تعديل: تبسيط الخريطة لأننا شيلنا حالة Created المبدئية والـ PaymentProcessed
        bool isValidTransition = (Status, newStatus) switch
        {
            (OrderStatus.Placed, OrderStatus.Packaged) => true,
            (OrderStatus.Placed, OrderStatus.Cancelled) => true,
            (OrderStatus.Packaged, OrderStatus.Cancelled) => true,
            (OrderStatus.Dispatched, OrderStatus.Delivered) => true,
            _ => false
        };

        if (!isValidTransition)
            return OrderErrors.InvalidStatusTransition;

        Status = newStatus;

        // تعديل: إضافة الـ Event هنا عشان نبعت إشعار بأي تغيير في الحالة للعميل
        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, newStatus, TrackingLabel));

        return Result.Success;
    }

    public Result<Success> CancelByCustomer()
    {
        if (Status == OrderStatus.Cancelled ||
            Status == OrderStatus.Delivered ||
            Status == OrderStatus.Dispatched) return OrderErrors.StatusUnchangeable;

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledEvent(Id, UserId));
        return Result.Success;
    }

    public Result<Success> DispatchOrder(string trackingLabel)
    {
        if (Status != OrderStatus.Packaged)
            return OrderErrors.NotPackaged;

        if (string.IsNullOrWhiteSpace(trackingLabel))
            return OrderErrors.TrackingRequired;

        TrackingLabel = trackingLabel;
        Status = OrderStatus.Dispatched;

        // تعديل: استخدام نفس الـ Event الموحد بدل Event جديد مخصوص للـ Dispatch
        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, Status, TrackingLabel));

        return Result.Success;
    }
}
