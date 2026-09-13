using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities;

namespace OneFit.Infrastructure.Persistence.Configurations;

public class UserInteractionConfiguration : IEntityTypeConfiguration<UserInteraction>
{
    public void Configure(EntityTypeBuilder<UserInteraction> builder)
    {
        builder.HasKey(e => e.InteractionId).HasName("user_interactions_pkey");
        builder.ToTable("user_interactions");

        builder.HasIndex(e => e.ShopperId, "idx_user_interactions_user_id");

        builder.Property(e => e.InteractionId).HasColumnType("character varying").HasColumnName("interaction_id");
        builder.Property(e => e.ShopperId).HasColumnType("character varying").HasColumnName("user_id");
        builder.Property(e => e.ProductId).HasColumnType("character varying").HasColumnName("product_id");
        builder.Property(e => e.InteractionType).HasColumnType("character varying(20)").HasColumnName("interaction_type");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

        builder.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).HasConstraintName("fk_user_interactions_product");
        builder.HasOne(d => d.Shopper).WithMany().HasForeignKey(d => d.ShopperId).HasConstraintName("fk_user_interactions_shopper");
    }
}
