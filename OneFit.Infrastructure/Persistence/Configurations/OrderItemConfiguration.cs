using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(e => e.OrderItemId).HasName("order_items_pkey");
        builder.ToTable("order_items");

        builder.HasIndex(e => e.SubOrderId, "idx_order_items_sub_order_id");

        builder.Property(e => e.OrderItemId).HasColumnType("character varying").HasColumnName("order_item_id");
        builder.Property(e => e.PriceAtPurchaseEgp).HasPrecision(10, 2).HasComment("final locked-in price at checkout — independent of later product.price_egp changes").HasColumnName("price_at_purchase_egp");
        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.Qty).HasDefaultValue(1).HasColumnName("qty");
        builder.Property(e => e.Size).HasColumnType("character varying").HasColumnName("size");
        builder.Property(e => e.SubOrderId).HasColumnType("character varying").HasColumnName("sub_order_id");

        builder.HasOne(d => d.Product).WithMany(p => p.OrderItems).HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_order_items_product");
        builder.HasOne(d => d.SubOrder).WithMany(p => p.OrderItems).HasForeignKey(d => d.SubOrderId).HasConstraintName("fk_order_items_sub_order");
    }
}