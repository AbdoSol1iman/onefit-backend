using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;

namespace OneFit.Application.Features.WishList.Commands.RemoveWishlistItem
{
    /// <summary>
    /// Handler for removing a wishlist item with authorization check
    /// </summary>
    public class RemoveWishlistItemHandler : IRequestHandler<RemoveWishlistItemCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveWishlistItemHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handles the RemoveWishlistItemCommand by verifying ownership and removing the item
        /// </summary>
        public async Task<bool> Handle(RemoveWishlistItemCommand request, CancellationToken ct)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.ShopperId))
            {
                throw new NotFoundException("INVALID_REQUEST", "ShopperId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.WishlistItemId))
            {
                throw new NotFoundException("INVALID_REQUEST", "WishlistItemId is required.");
            }

            var item = await _unitOfWork.Wishlist.GetByIdAsync(request.WishlistItemId, ct)
                ?? throw new NotFoundException(
                    "WISHLIST_ITEM_NOT_FOUND",
                    $"No wishlist item '{request.WishlistItemId}' was found.");

            // Security check: Ensure item belongs to the requesting shopper
            if (item.ShopperId != request.ShopperId)
            {
                throw new NotFoundException(
                    "UNAUTHORIZED",
                    "You do not have permission to delete this wishlist item.");
            }

            _unitOfWork.Wishlist.Remove(item);
            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
