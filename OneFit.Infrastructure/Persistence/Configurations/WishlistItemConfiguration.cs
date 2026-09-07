
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");
        builder.HasKey(w => w.WishlistItemId);

        builder.Property(w => w.WishlistItemId).HasColumnName("wishlist_item_id");
        builder.Property(w => w.ShopperId).HasColumnName("shopper_id");
        builder.Property(w => w.ProductId).HasColumnName("product_id");
        builder.Property(w => w.Size).HasColumnName("size");
        builder.Property(w => w.PriceAtSaveEgp).HasColumnName("price_at_save_egp").HasColumnType("decimal(10,2)");
        builder.Property(w => w.AddedAt).HasColumnName("added_at");

        builder.HasOne(w => w.Shopper)
            .WithMany(s => s.WishlistItems)
            .HasForeignKey(w => w.ShopperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.ProductSize)
            .WithMany(ps => ps.WishlistItems)
            .HasForeignKey(w => new { w.ProductId, w.Size })
            .HasPrincipalKey(ps => new { ps.ProductId, ps.Size })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.ShopperId).HasDatabaseName("idx_wishlist_shopper_id");
    }
}

