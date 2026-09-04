using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(e => e.CartId).HasName("carts_pkey");
        builder.ToTable("carts");

        builder.Property(e => e.CartId).HasColumnType("character varying").HasColumnName("cart_id");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        builder.Property(e => e.ShopperId).HasColumnType("character varying").HasColumnName("shopper_id");
        builder.Property(e => e.Status).HasDefaultValueSql("'active'::character varying").HasComment("active, checked_out").HasColumnType("character varying").HasColumnName("status");

        builder.HasOne(d => d.Shopper).WithMany(p => p.Carts).HasForeignKey(d => d.ShopperId).HasConstraintName("fk_carts_shopper");
    }
}