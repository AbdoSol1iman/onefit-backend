using MediatR;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.WishList.Models;
using OneFit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.WishList.Queries.GetWishlist
{
    public class GetWishlistHandler : IRequestHandler<GetWishlistQuery, IReadOnlyList<WishlistListItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWishlistHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IReadOnlyList<WishlistListItemDto>> Handle(GetWishlistQuery request, CancellationToken ct)
        {
            var items = await _unitOfWork.Wishlist.ListByShopperAsync(request.ShopperId, ct);
            return items.Select(MapToDto).ToList();
        }

        private static WishlistListItemDto MapToDto(WishlistItem w)
        {
            var product = w.ProductSize.Product;

            return new WishlistListItemDto(
                WishlistItemId: w.WishlistItemId,
                ProductId: w.ProductId,
                BrandName: product.Brand.Name,
                Size: w.Size,
                PriceEgp: product.PriceEgp,
                PriceAtSaveEgp: w.PriceAtSaveEgp,
                InStock: w.ProductSize.StockQty > 0);
        }
    }
}
