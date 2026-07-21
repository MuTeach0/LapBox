using LapBox.Domain.Common;

namespace LapBox.Domain.Carts;

/// <summary>
/// CartItem is a regular entity (not an owned entity).
/// This gives EF Core full change-tracking control and avoids owned-entity quirks
/// where new owned instances could be treated as UPDATE instead of INSERT.
/// </summary>
public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid LaptopId { get; private set; }
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
