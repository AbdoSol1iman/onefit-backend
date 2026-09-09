using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Cart.Models;

namespace OneFit.Application.Features.Cart.Commands.RemoveCartItem
{
    /// <summary>
    /// Handler for removing a cart item by product ID and size
    /// </summary>
    public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveCartItemCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handles the RemoveCartItemCommand by removing the specified item from the cart
        /// </summary>
        public async Task<CartResponse> Handle(RemoveCartItemCommand request, CancellationToken ct)
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

            if (string.IsNullOrWhiteSpace(request.Size))
            {
                throw new NotFoundException("INVALID_REQUEST", "Size is required.");
            }

            var cart = await _unitOfWork.Cart.GetOrCreateActiveCartAsync(request.ShopperId, ct);

            var loadedCart = await _unitOfWork.Cart.GetCartWithItemsAsync(cart.CartId, ct)
                ?? throw new InvalidOperationException("Cart disappeared immediately after being fetched.");

            // Find the specific item matching both ProductId and Size
            var lineToRemove = loadedCart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == request.ProductId && ci.Size == request.Size)
                ?? throw new NotFoundException(
                    "CART_ITEM_NOT_FOUND",
                    $"No item with product_id '{request.ProductId}' and size '{request.Size}' was found in this cart.");

            _unitOfWork.Cart.RemoveLineItem(lineToRemove);
            await _unitOfWork.SaveChangesAsync(ct);

            // Fetch updated cart (may be empty)
            var updatedCart = await _unitOfWork.Cart.GetCartWithItemsAsync(cart.CartId, ct)
                ?? throw new InvalidOperationException("Cart disappeared immediately after being saved.");

            var items = updatedCart.CartItems
                .Select(ci => new CartItemDto(ci.ProductId, ci.Brand.Name, ci.ProductSize.Product.PriceEgp, ci.Qty))
                .ToList();

            return new CartResponse(updatedCart.CartId, items, items.Sum(i => i.PriceEgp * i.Qty));
        }
    }
}
