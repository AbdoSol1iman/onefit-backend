using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Common.Interfaces.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Commands.RemoveWishlistItem
{
    public class RemoveWishlistItemHandler : IRequestHandler<RemoveWishlistItemCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveWishlistItemHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<bool> Handle(RemoveWishlistItemCommand request, CancellationToken ct)
        {
            var item = await _unitOfWork.Wishlist.GetByIdAsync(request.WishlistItemId, ct)
                ?? throw new NotFoundException(
                    "WISHLIST_ITEM_NOT_FOUND",
                    $"No wishlist item '{request.WishlistItemId}' was found.");

            _unitOfWork.Wishlist.Remove(item);
            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }
    }
}
