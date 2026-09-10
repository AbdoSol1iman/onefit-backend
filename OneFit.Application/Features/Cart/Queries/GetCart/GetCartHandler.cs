using MediatR;
using OneFit.Application.Common.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Queries.GetCart
{
    public class GetCartHandler
        : IRequestHandler<GetCartQuery, CartDto>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<CartDto> Handle(
            GetCartQuery request,
            CancellationToken cancellationToken)
        {
            var cart = await _cartRepository
                .GetOrCreateActiveCartAsync(
                    request.ShopperId,
                    cancellationToken);

            var cartWithItems = await _cartRepository
                .GetCartWithItemsAsync(
                    cart.CartId,
                    cancellationToken);

            if (cartWithItems is null)
                throw new KeyNotFoundException("Cart not found.");

            return new CartDto
            {
                CartId = cartWithItems.CartId,
                Status = cartWithItems.Status,
                CreatedAt = cartWithItems.CreatedAt,

                Items = cartWithItems.CartItems
                    .Select(item => new CartItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductSize.Product.Name,
                        ImageUrl = item.ProductSize.Product.ImageUrl,

                        BrandId = item.BrandId,
                        BrandName = item.Brand.Name,

                        Size = item.Size,
                        Quantity = item.Qty,

                        UnitPrice = item.PriceAtAddEgp,
                        TotalPrice = item.PriceAtAddEgp * item.Qty
                    })
                    .ToList(),

                Total = cartWithItems.CartItems
                    .Sum(item => item.PriceAtAddEgp * item.Qty)
            };
        }
    }
}

