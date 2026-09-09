using Microsoft.EntityFrameworkCore;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Domain.Entities;
using OneFit.Infrastructure.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure.Persistence.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly OneFitDbContext _db;

        public WishlistRepository(OneFitDbContext db) => _db = db;

        public Task<ProductSize?> FindProductSizeAsync(string productId, string size, CancellationToken ct = default) =>
            _db.ProductSizes
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.Size == size, ct);

        public Task AddAsync(WishlistItem item, CancellationToken ct = default)
        {
            _db.WishlistItems.Add(item);
            return Task.CompletedTask;
        }

        public Task<WishlistItem?> GetByIdAsync(string wishlistItemId, CancellationToken ct = default) =>
            _db.WishlistItems.FirstOrDefaultAsync(w => w.WishlistItemId == wishlistItemId, ct);

        public void Remove(WishlistItem item) => _db.WishlistItems.Remove(item);

        public async Task<IReadOnlyList<WishlistItem>> ListByShopperAsync(string shopperId, CancellationToken ct = default) =>
            await _db.WishlistItems
                .AsNoTracking()
                .Include(w => w.ProductSize)
                    .ThenInclude(ps => ps.Product)
                        .ThenInclude(p => p.Brand)
                .Where(w => w.ShopperId == shopperId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync(ct);
    }
}
