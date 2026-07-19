using LapBox.Domain.Common;

namespace LapBox.Domain.Carts;

public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid LaptopId { get; private set; } // Slicing ID
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private CartItem() { }

    internal CartItem(Guid cartId, Guid laptopId, int quantity, decimal unitPrice) : base(Guid.NewGuid())
    {
        CartId = cartId;
        LaptopId = laptopId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void UpdateQuantity(int newQuantity) => Quantity = newQuantity;
}