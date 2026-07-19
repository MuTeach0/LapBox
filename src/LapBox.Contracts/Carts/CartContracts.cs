namespace LapBox.Contracts.Carts;

public sealed record CartItemResponse(
    Guid Id,
    Guid LaptopId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record CartResponse(
    Guid Id,
    Guid IdentityId,
    string Status,
    IReadOnlyList<CartItemResponse> Items,
    decimal TotalAmount);

public sealed record AddOrUpdateCartItemRequest(
    Guid LaptopId,
    int Quantity);

public sealed record RemoveCartItemRequest(Guid LaptopId);
