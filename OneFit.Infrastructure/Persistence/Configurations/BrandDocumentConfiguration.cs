using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneFit.Domain.Entities.Brands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure.Persistence.Configurations
{
    public class BrandDocumentConfiguration : IEntityTypeConfiguration<BrandDocument>
    {
        public void Configure(EntityTypeBuilder<BrandDocument> builder)
        {
            builder.HasKey(e => e.DocumentId)
                .HasName("brand_documents_pkey");

            builder.ToTable("brand_documents");

            builder.Property(e => e.DocumentId)
                .HasColumnName("document_id")
                .HasColumnType("uuid");

            builder.Property(e => e.BrandId)
                .HasColumnName("brand_id")
                .HasColumnType("character varying")
                .IsRequired();

            builder.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasColumnType("character varying")
                .IsRequired();

            builder.Property(e => e.ContentType)
                .HasColumnName("content_type")
                .HasColumnType("character varying")
                .IsRequired();

            builder.Property(e => e.FileSize)
                .HasColumnName("file_size")
                .IsRequired();

            builder.Property(e => e.StorageKey)
                .HasColumnName("storage_key")
                .HasColumnType("character varying")
                .IsRequired();

            builder.Property(e => e.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasColumnType("timestamp with time zone");

            builder.HasOne(e => e.Brand)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.BrandId)
                .HasConstraintName("brand_documents_brand_id_fkey")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
