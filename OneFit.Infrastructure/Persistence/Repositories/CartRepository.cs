using Microsoft.EntityFrameworkCore;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Domain.Entities;
using OneFit.Infrastructure.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly OneFitDbContext _db;

        public CartRepository(OneFitDbContext db) => _db = db;

        public Task<ProductSize?> FindProductSizeAsync(string productId, string size, CancellationToken ct = default) =>
            _db.ProductSizes
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.Size == size, ct);

        public async Task<Cart> GetOrCreateActiveCartAsync(string shopperId, CancellationToken ct = default)
        {
            var existing = await _db.Carts
                .FirstOrDefaultAsync(c => c.ShopperId == shopperId && c.Status == "active", ct);

            if (existing is not null) return existing;

            var cart = new Cart
            {
                CartId = $"CART-{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
                ShopperId = shopperId,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct); 

            return cart;
        }

        public Task<CartItem?> FindLineItemAsync(string cartId, string productId, string size, CancellationToken ct = default) =>
            _db.CartItems.FirstOrDefaultAsync(
                ci => ci.CartId == cartId && ci.ProductId == productId && ci.Size == size, ct);

        public Task AddLineItemAsync(CartItem item, CancellationToken ct = default)
        {
            _db.CartItems.Add(item);
            return Task.CompletedTask;
        }

        public void RemoveLineItem(CartItem item) => _db.CartItems.Remove(item);

        public Task<Cart?> GetCartWithItemsAsync(string cartId, CancellationToken ct = default) =>
            _db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .FirstOrDefaultAsync(c => c.CartId == cartId, ct);
    }
}
