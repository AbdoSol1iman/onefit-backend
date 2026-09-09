using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.WishList.Models;
using OneFit.Domain.Entities;

namespace OneFit.Application.Features.WishList.Commands.AddWishlistItem
{
    /// <summary>
    /// Handler for adding an item to the wishlist
    /// </summary>
    public class AddWishlistItemHandler : IRequestHandler<AddWishlistItemCommand, WishlistItemDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddWishlistItemHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        /// <summary>
        /// Handles the AddWishlistItemCommand by creating a new wishlist item
        /// </summary>
        public async Task<WishlistItemDto> Handle(AddWishlistItemCommand request, CancellationToken ct)
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

            var productSize = await _unitOfWork.Wishlist.FindProductSizeAsync(request.ProductId, request.Size, ct)
                ?? throw new NotFoundException(
                    "PRODUCT_SIZE_NOT_FOUND",
                    $"No product '{request.ProductId}' with size '{request.Size}' was found.");

            var item = new WishlistItem
            {
                WishlistItemId = Guid.NewGuid().ToString(), // Use full GUID for uniqueness
                ShopperId = request.ShopperId,
                ProductId = request.ProductId,
                Size = request.Size,
                PriceAtSaveEgp = productSize.Product.PriceEgp,
                AddedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wishlist.AddAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new WishlistItemDto(item.WishlistItemId, item.ProductId, item.Size, item.PriceAtSaveEgp, item.AddedAt);
        }
    }
}
