using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(e => e.CartItemId).HasName("cart_items_pkey");
        builder.ToTable("cart_items");

        builder.HasIndex(e => e.CartId, "idx_cart_items_cart_id");
        builder.HasIndex(e => new { e.CartId, e.ProductId, e.Size }, "uq_cart_item_product_size").IsUnique();

        builder.Property(e => e.CartItemId).HasColumnType("character varying").HasColumnName("cart_item_id");
        builder.Property(e => e.BrandId).HasComment("denormalized for FR-18 per-line-item brand display without a join").HasColumnType("character varying").HasColumnName("brand_id");
        builder.Property(e => e.CartId).HasColumnType("character varying").HasColumnName("cart_id");
        builder.Property(e => e.PriceAtAddEgp).HasPrecision(10, 2).HasComment("snapshot for FR-09/NFR-09 re-validation at checkout").HasColumnName("price_at_add_egp");
        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.Qty).HasDefaultValue(1).HasColumnName("qty");
        builder.Property(e => e.Size).HasColumnType("character varying").HasColumnName("size");

        builder.HasOne(d => d.Brand).WithMany(p => p.CartItems).HasForeignKey(d => d.BrandId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_cart_items_brand");
        builder.HasOne(d => d.Cart).WithMany(p => p.CartItems).HasForeignKey(d => d.CartId).HasConstraintName("fk_cart_items_cart");
        builder.HasOne(d => d.ProductSize).WithMany(p => p.CartItems).HasForeignKey(d => new { d.ProductId, d.Size }).HasConstraintName("fk_cart_items_product_size");
    }
}