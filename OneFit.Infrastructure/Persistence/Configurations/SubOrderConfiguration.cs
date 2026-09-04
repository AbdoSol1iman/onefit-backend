using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class SubOrderConfiguration : IEntityTypeConfiguration<SubOrder>
{
    public void Configure(EntityTypeBuilder<SubOrder> builder)
    {
        builder.HasKey(e => e.SubOrderId).HasName("sub_orders_pkey");
        builder.ToTable("sub_orders");

        builder.HasIndex(e => e.BrandId, "idx_sub_orders_brand_id");
        builder.HasIndex(e => e.OrderId, "idx_sub_orders_order_id");

        builder.Property(e => e.SubOrderId).HasColumnType("character varying").HasColumnName("sub_order_id");
        builder.Property(e => e.BrandId).HasColumnType("character varying").HasColumnName("brand_id");
        builder.Property(e => e.OrderId).HasColumnType("character varying").HasColumnName("order_id");
        builder.Property(e => e.TotalEgp).HasPrecision(10, 2).HasComment("FR-21 — one sub-order per brand; must equal SUM(order_items.price_at_purchase_egp * qty)").HasColumnName("total_egp");

        builder.HasOne(d => d.Brand).WithMany(p => p.SubOrders).HasForeignKey(d => d.BrandId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_sub_orders_brand");
        builder.HasOne(d => d.Order).WithMany(p => p.SubOrders).HasForeignKey(d => d.OrderId).HasConstraintName("fk_sub_orders_order");
    }
}