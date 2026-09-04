using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.HasKey(e => e.AlertId).HasName("alerts_pkey");
        builder.ToTable("alerts");

        builder.HasIndex(e => e.ShopperId, "idx_alerts_shopper_unread").HasFilter("(is_read = false)");

        builder.Property(e => e.AlertId).HasColumnType("character varying").HasColumnName("alert_id");
        builder.Property(e => e.AlertType).HasComment("PRICE_DROP or LOW_STOCK").HasColumnType("character varying").HasColumnName("alert_type");
        builder.Property(e => e.IsRead).HasComment("FR-28").HasColumnName("is_read");
        builder.Property(e => e.NewPriceEgp).HasPrecision(10, 2).HasColumnName("new_price_egp");
        builder.Property(e => e.OldPriceEgp).HasPrecision(10, 2).HasColumnName("old_price_egp");
        builder.Property(e => e.PercentSaved).HasPrecision(5, 2).HasColumnName("percent_saved");
        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.ShopperId).HasColumnType("character varying").HasColumnName("shopper_id");
        builder.Property(e => e.Size).HasComment("set only for LOW_STOCK alerts, per FR-26").HasColumnType("character varying").HasColumnName("size");
        builder.Property(e => e.TriggeredAt).HasDefaultValueSql("now()").HasColumnName("triggered_at");

        builder.HasOne(d => d.Product).WithMany(p => p.Alerts).HasForeignKey(d => d.ProductId).HasConstraintName("fk_alerts_product");
        builder.HasOne(d => d.Shopper).WithMany(p => p.Alerts).HasForeignKey(d => d.ShopperId).HasConstraintName("fk_alerts_shopper");
    }
}