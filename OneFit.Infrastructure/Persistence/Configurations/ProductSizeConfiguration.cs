using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
{
    public void Configure(EntityTypeBuilder<ProductSize> builder)
    {
        builder.HasKey(e => new { e.ProductId, e.Size }).HasName("product_sizes_pkey");
        builder.ToTable("product_sizes");

        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.Size).HasComment("S, M, L, XL, etc.").HasColumnType("character varying").HasColumnName("size");
        builder.Property(e => e.StockQty).HasColumnName("stock_qty");

        builder.HasOne(d => d.Product).WithMany(p => p.ProductSizes).HasForeignKey(d => d.ProductId).HasConstraintName("fk_product_sizes_product");
    }
}