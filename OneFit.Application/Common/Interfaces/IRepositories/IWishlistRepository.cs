using OneFit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.IRepositories
{
    public interface IWishlistRepository
    {
        
        Task<ProductSize?> FindProductSizeAsync(string productId, string size, CancellationToken ct = default);

        Task AddAsync(WishlistItem item, CancellationToken ct = default);

        Task<WishlistItem?> GetByIdAsync(string wishlistItemId, CancellationToken ct = default);

        void Remove(WishlistItem item);

        Task<IReadOnlyList<WishlistItem>> ListByShopperAsync(string shopperId, CancellationToken ct = default);
    }
}
