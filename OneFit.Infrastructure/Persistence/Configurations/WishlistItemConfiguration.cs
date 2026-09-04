using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(e => e.WishlistItemId).HasName("wishlist_items_pkey");
        builder.ToTable("wishlist_items");

        builder.HasIndex(e => e.ShopperId, "idx_wishlist_shopper_id");

        builder.Property(e => e.WishlistItemId).HasColumnType("character varying").HasColumnName("wishlist_item_id");
        builder.Property(e => e.AddedAt).HasDefaultValueSql("now()").HasColumnName("added_at");
        builder.Property(e => e.PriceAtSaveEgp).HasPrecision(10, 2).HasComment("snapshot used to detect PRICE_DROP per FR-25").HasColumnName("price_at_save_egp");
        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.ShopperId).HasColumnType("character varying").HasColumnName("shopper_id");
        builder.Property(e => e.Size).HasComment("needed for FR-26 per-size stock alerts").HasColumnType("character varying").HasColumnName("size");

        builder.HasOne(d => d.Shopper).WithMany(p => p.WishlistItems).HasForeignKey(d => d.ShopperId).HasConstraintName("fk_wishlist_shopper");
        builder.HasOne(d => d.ProductSize).WithMany(p => p.WishlistItems).HasForeignKey(d => new { d.ProductId, d.Size }).HasConstraintName("fk_wishlist_product_size");
    }
}