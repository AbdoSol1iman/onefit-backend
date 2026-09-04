using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class ShopperConfiguration : IEntityTypeConfiguration<Shopper>
{
    public void Configure(EntityTypeBuilder<Shopper> builder)
    {
        builder.HasKey(e => e.ShopperId).HasName("shoppers_pkey");
        builder.ToTable("shoppers");

        builder.Property(e => e.ShopperId).HasColumnType("character varying").HasColumnName("shopper_id");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
        builder.Property(e => e.Name).HasColumnType("character varying").HasColumnName("name");
    }
}