using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.ProductId).HasName("products_pkey");
        builder.ToTable("products");

        builder.HasIndex(e => e.BrandId, "idx_products_brand_id");
        builder.HasIndex(e => e.Category, "idx_products_category");
        builder.HasIndex(e => e.StyleTags, "idx_products_style_tags").HasMethod("gin");

        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.BrandId).HasColumnType("character varying").HasColumnName("brand_id");
        builder.Property(e => e.Category).HasComment("shirt, pants, shoes, accessory, etc.").HasColumnType("character varying").HasColumnName("category");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        builder.Property(e => e.ImageUrl).HasColumnType("character varying").HasColumnName("image_url");
        builder.Ignore(e => e.ImageEmbedding);
        builder.Property(e => e.Name).HasColumnType("character varying").HasColumnName("name");
        builder.Property(e => e.PriceEgp).HasPrecision(10, 2).HasColumnName("price_egp");
        builder.Property(e => e.StyleTags).HasComment("e.g. linen, casual, chino — used by Catalog-Query Tool").HasColumnType("character varying[]").HasColumnName("style_tags");

        builder.HasOne(d => d.Brand).WithMany(p => p.Products).HasForeignKey(d => d.BrandId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_products_brand");
    }
}