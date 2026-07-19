using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Carts.Enums;

namespace LapBox.Domain.Carts;

public sealed class Cart : AggregateRoot
{
    /// <summary>
    /// The Identity/User ID who owns this cart.
    /// Uses IdentityId directly since users can have carts before becoming customers.
    /// </summary>
    public Guid IdentityId { get; private set; }
    public CartStatus Status { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    private Cart(Guid id, Guid identityId) : base(id)
    {
        IdentityId = identityId;
        Status = CartStatus.Active;
    }

    public static Result<Cart> Create(Guid identityId)
    {
        if (identityId == Guid.Empty)
            return CartErrors.IdentityIdRequired;

        return new Cart(Guid.NewGuid(), identityId);
    }

    public Result<Success> AddOrUpdateItem(Guid laptopId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            return CartErrors.QuantityInvalid;

        var existingItem = _items.FirstOrDefault(x => x.LaptopId == laptopId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new CartItem(Id, laptopId, quantity, unitPrice));
        }

        Status = CartStatus.StockReserved;
        return Result.Success;
    }

    public Result<Success> RemoveItem(Guid laptopId)
    {
        var item = _items.FirstOrDefault(x => x.LaptopId == laptopId);
        if (item is null)
            return CartErrors.ItemNotFound;

        _items.Remove(item);
        return Result.Success;
    }

    public void Clear(bool checkedOut = false)
    {
        _items.Clear();
        Status = checkedOut ? CartStatus.CheckedOut : CartStatus.Active;
    }

    public decimal CalculateTotal() => _items.Sum(x => x.UnitPrice * x.Quantity);
    public void UpdateLastModified()
    {
        LastModifiedUtc = DateTimeOffset.UtcNow;
    }
}
