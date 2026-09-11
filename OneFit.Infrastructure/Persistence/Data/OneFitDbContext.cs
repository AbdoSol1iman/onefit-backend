using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneFit.Application.Common.Interfaces;
using OneFit.Domain.Entities;
using OneFit.Domain.Entities.Brands;
using OneFit.Infrastructure.Identity;

namespace OneFit.Infrastructure.Persistence.Data;

public partial class OneFitDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public OneFitDbContext(DbContextOptions<OneFitDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<BrandDocument> BrandDocuments { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<Shopper> Shoppers { get; set; }

    public virtual DbSet<SubOrder> SubOrders { get; set; }

    public virtual DbSet<WishlistItem> WishlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OneFitDbContext).Assembly);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
