using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.OrderId).HasName("orders_pkey");
        builder.ToTable("orders");

        builder.HasIndex(e => e.ShopperId, "idx_orders_shopper_id");

        builder.Property(e => e.OrderId).HasColumnType("character varying").HasColumnName("order_id");
        builder.Property(e => e.CartId).HasColumnType("character varying").HasColumnName("cart_id");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        builder.Property(e => e.GrandTotalEgp).HasPrecision(10, 2).HasComment("must equal SUM(sub_orders.total_egp) — enforced in application code, not by the DB").HasColumnName("grand_total_egp");
        builder.Property(e => e.PaymentStatus).HasDefaultValueSql("'PAID_SIMULATED'::character varying").HasComment("FR-23 — simulated only").HasColumnType("character varying").HasColumnName("payment_status");
        builder.Property(e => e.ShopperId).HasColumnType("character varying").HasColumnName("shopper_id");

        builder.HasOne(d => d.Cart).WithMany(p => p.Orders).HasForeignKey(d => d.CartId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_orders_cart");
        builder.HasOne(d => d.Shopper).WithMany(p => p.Orders).HasForeignKey(d => d.ShopperId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_orders_shopper");
    }
}