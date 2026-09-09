using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Cart.Models;
using OneFit.Domain.Entities;

namespace OneFit.Application.Features.Cart.Commands.AddCartItem
{
    /// <summary>
    /// Handler for adding an item to the shopping cart
    /// </summary>
    public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, CartResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddCartItemCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handles the AddCartItemCommand by adding or updating a cart item
        /// </summary>
        public async Task<CartResponse> Handle(AddCartItemCommand request, CancellationToken ct)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.ShopperId))
            {
                throw new NotFoundException("INVALID_REQUEST", "ShopperId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                throw new NotFoundException("INVALID_REQUEST", "ProductId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Size) || request.Qty <= 0)
            {
                throw new NotFoundException("INVALID_REQUEST", "Size and a positive Qty are required.");
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
                    CartItemId = Guid.NewGuid().ToString(), // Use full GUID for uniqueness
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
