using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Carts;

public static class CartErrors
{
    public static readonly Error IdentityIdRequired =
        Error.Validation("Cart.IdentityId", "Identity ID is required.");

    public static readonly Error QuantityInvalid =
        Error.Validation("Cart.Quantity", "Quantity must be greater than zero.");

    public static readonly Error ItemNotFound =
        Error.NotFound("Cart.ItemNotFound", "Laptop is not in the cart.");
}