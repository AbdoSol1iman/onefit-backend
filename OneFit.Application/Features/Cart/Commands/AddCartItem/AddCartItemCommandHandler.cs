using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Cart.Models;
using OneFit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Cart.Commands.AddCartItem
{
    public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, CartResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddCartItemCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<CartResponse> Handle(AddCartItemCommand request, CancellationToken ct)
        {
           
            
            if (string.IsNullOrWhiteSpace(request.Size) || request.Qty <= 0)
            {
                throw new NotFoundException("INVALID_REQUEST", "size and a positive qty are required.");
            }

            var productSize = await _unitOfWork.Cart.FindProductSizeAsync(request.ProductId, request.Size, ct)
                ?? throw new NotFoundException(
                    "PRODUCT_SIZE_NOT_FOUND",
                    $"No product '{request.ProductId}' with size '{request.Size}' was found.");

            var cart = await _unitOfWork.Cart.GetOrCreateActiveCartAsync(request.ShopperId, ct);

            CartWarningDto? warning = null;
            var existingLine = await _unitOfWork.Cart.FindLineItemAsync(cart.CartId, request.ProductId, request.Size, ct);

            if (existingLine is not null)
            {
                
                existingLine.Qty += request.Qty;

              
                if (existingLine.PriceAtAddEgp != productSize.Product.PriceEgp)
                {
                    warning = new CartWarningDto(
                        request.ProductId, "PRICE_CHANGED", existingLine.PriceAtAddEgp, productSize.Product.PriceEgp);
                }
            }
            else
            {
                var newItem = new CartItem
                {
                    CartItemId = $"CI-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
                    CartId = cart.CartId,
                    ProductId = request.ProductId,
                    BrandId = productSize.Product.BrandId,
                    Size = request.Size,
                    Qty = request.Qty,
                    PriceAtAddEgp = productSize.Product.PriceEgp
                };
                await _unitOfWork.Cart.AddLineItemAsync(newItem, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var updatedCart = await _unitOfWork.Cart.GetCartWithItemsAsync(cart.CartId, ct)
                ?? throw new InvalidOperationException("Cart disappeared immediately after being saved.");

            var items = updatedCart.CartItems
                .Select(ci => new CartItemDto(ci.ProductId, ci.Brand.Name, ci.ProductSize.Product.PriceEgp, ci.Qty))
                .ToList();

            return new CartResponse(
                updatedCart.CartId,
                items,
                items.Sum(i => i.PriceEgp * i.Qty),
                warning is null ? null : new List<CartWarningDto> { warning });
        }
    }
}
