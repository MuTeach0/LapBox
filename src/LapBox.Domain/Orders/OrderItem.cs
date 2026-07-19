using LapBox.Domain.Common;

namespace LapBox.Domain.Orders;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid LaptopId { get; private set; } // Slicing ID reference
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }

    private OrderItem() { }

    internal OrderItem(Guid orderId, Guid laptopId, int quantity, decimal unitPrice, decimal discountAmount) 
        : base(Guid.NewGuid())
    {
        OrderId = orderId;
        LaptopId = laptopId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
    }
}