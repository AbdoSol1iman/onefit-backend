using Microsoft.EntityFrameworkCore;
using OneFit.Domain.Entities;
using OneFit.Domain.Entities.Brands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Brand> Brands { get; }

        DbSet<BrandDocument> BrandDocuments { get; }
        DbSet<Order> Orders { get; }
        DbSet<SubOrder> SubOrders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<ProductSize> ProductSizes { get; }
        DbSet<Shopper> Shoppers { get; }
        DbSet<UserInteraction> UserInteractions { get; }
        DbSet<Product> Products { get; }
        DbSet<Cart> Carts { get; }
        DbSet<CartItem> CartItems { get; }
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
