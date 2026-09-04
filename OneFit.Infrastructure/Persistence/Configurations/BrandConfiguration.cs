using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(e => e.BrandId).HasName("brands_pkey");
        builder.ToTable("brands");

        builder.Property(e => e.BrandId).HasColumnType("character varying").HasColumnName("brand_id");
        builder.Property(e => e.Name).HasColumnType("character varying").HasColumnName("name");
    }
}