using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.WishList.Models;
using OneFit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Commands.AddWishlistItem
{
    public class AddWishlistItemHandler : IRequestHandler<AddWishlistItemCommand, WishlistItemDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddWishlistItemHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<WishlistItemDto> Handle(AddWishlistItemCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Size))
            {
                throw new NotFoundException("INVALID_REQUEST", "size is required.");
            }

            var productSize = await _unitOfWork.Wishlist.FindProductSizeAsync(request.ProductId, request.Size, ct)
                ?? throw new NotFoundException(
                    "PRODUCT_SIZE_NOT_FOUND",
                    $"No product '{request.ProductId}' with size '{request.Size}' was found.");

            var item = new WishlistItem
            {
                WishlistItemId = $"WL-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
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
