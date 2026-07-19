using LapBox.Application.Features.Carts.DTOs;
using LapBox.Domain.Carts;

namespace LapBox.Application.Features.Carts.Mappers;

public static class CartMappingExtensions
{
    public static CartDTO ToDTO(this Cart cart)
    {
        var items = cart.Items.Select(static i => new CartItemDTO(
            i.Id,
            i.LaptopId,
            i.Quantity,
            i.UnitPrice,
            i.UnitPrice * i.Quantity
        )).ToList().AsReadOnly();

        return new CartDTO(cart.Id, cart.IdentityId, cart.Status.ToString(), items, cart.CalculateTotal());
    }
}
