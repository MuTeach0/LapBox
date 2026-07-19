namespace LapBox.Application.Features.Carts.DTOs;

public sealed record CartDTO(
    Guid Id,
    Guid IdentityId,
    string Status,
    IReadOnlyCollection<CartItemDTO> Items,
    decimal TotalPrice);
