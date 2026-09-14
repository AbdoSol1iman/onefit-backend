using DomainCart = OneFit.Domain.Entities.Cart;

namespace OneFit.Application.Features.Cart.Models;

public static class CartMapper
{
    public static CartResponse FromCart(DomainCart cart, IReadOnlyList<CartWarningDto>? warnings = null)
    {
        var items = cart.CartItems
            .Select(ci => new CartItemDto(ci.ProductId, ci.Brand.Name, ci.ProductSize.Product.PriceEgp, ci.Qty))
            .ToList();
        return new CartResponse(cart.CartId, items, items.Sum(i => i.PriceEgp * i.Qty), warnings);
    }

    public static CartResponse WithWarning(DomainCart cart, CartWarningDto? warning) =>
        FromCart(cart, warning is null ? null : new List<CartWarningDto> { warning });
}
