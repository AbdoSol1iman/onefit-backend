using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Cart.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Commands.RemoveCartItem
{
    public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveCartItemCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<CartResponse> Handle(RemoveCartItemCommand request, CancellationToken ct)
        {
            var cart = await _unitOfWork.Cart.GetOrCreateActiveCartAsync(request.ShopperId, ct);

            var loadedCart = await _unitOfWork.Cart.GetCartWithItemsAsync(cart.CartId, ct)
                ?? throw new InvalidOperationException("Cart disappeared immediately after being fetched.");

            var lineToRemove = loadedCart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId)
                ?? throw new NotFoundException(
                    "CART_ITEM_NOT_FOUND",
                    $"No item with product_id '{request.ProductId}' was found in this cart.");

            _unitOfWork.Cart.RemoveLineItem(lineToRemove);
            await _unitOfWork.SaveChangesAsync(ct);

           
            var updatedCart = await _unitOfWork.Cart.GetCartWithItemsAsync(cart.CartId, ct)
                ?? throw new InvalidOperationException("Cart disappeared immediately after being saved.");

            var items = updatedCart.CartItems
                .Select(ci => new CartItemDto(ci.ProductId, ci.Brand.Name, ci.ProductSize.Product.PriceEgp, ci.Qty))
                .ToList();

            return new CartResponse(updatedCart.CartId, items, items.Sum(i => i.PriceEgp * i.Qty));
        }
    }
}
