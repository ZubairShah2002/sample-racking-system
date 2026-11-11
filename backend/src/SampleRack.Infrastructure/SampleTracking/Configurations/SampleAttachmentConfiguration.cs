using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class SampleAttachmentConfiguration : IEntityTypeConfiguration<SampleAttachment>
{
    public void Configure(EntityTypeBuilder<SampleAttachment> builder)
    {
        builder.ToTable("sample_attachments", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SampleId).HasColumnName("sample_id").IsRequired();
        builder.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(300).IsRequired();
        builder.Property(e => e.FileRef).HasColumnName("file_ref").HasMaxLength(500).IsRequired();
        builder.Property(e => e.ContentType).HasColumnName("content_type").HasMaxLength(150);
        builder.Property(e => e.SizeBytes).HasColumnName("size_bytes");
        builder.Property(e => e.Category).HasColumnName("category").HasMaxLength(30).IsRequired();
        builder.Property(e => e.UploadedBy).HasColumnName("uploaded_by").HasMaxLength(150).IsRequired();
        builder.Property(e => e.UploadedAt).HasColumnName("uploaded_at").IsRequired();

        builder.HasIndex(e => e.SampleId).HasDatabaseName("ix_sample_attachments_sample");
    }
}
