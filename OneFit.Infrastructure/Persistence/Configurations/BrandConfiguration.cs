using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities.Brands;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(e => e.BrandId)
            .HasName("brands_pkey");

        builder.ToTable("brands");

        builder.Property(e => e.BrandId)
            .HasColumnType("character varying")
            .HasColumnName("brand_id");

        builder.Property(e => e.Name)
            .HasColumnType("character varying")
            .HasColumnName("name");

        builder.Property(e => e.ApplicationUserId)
            .HasColumnType("character varying")
            .HasColumnName("application_user_id")
            .IsRequired(false);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .HasDefaultValue(BrandStatusEnum.Approved);

        builder.Property(e => e.IsLocal)
            .HasColumnName("is_local")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasMany(e => e.Documents)
            .WithOne(e => e.Brand)
            .HasForeignKey(e => e.BrandId)
            .HasConstraintName("brand_documents_brand_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}