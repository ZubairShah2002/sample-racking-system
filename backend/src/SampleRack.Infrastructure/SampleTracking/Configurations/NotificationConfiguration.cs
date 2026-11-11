using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Severity).HasColumnName("severity").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(e => e.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.SampleId).HasColumnName("sample_id");
        builder.Property(e => e.SampleCode).HasColumnName("sample_code").HasMaxLength(50);
        builder.Property(e => e.DedupeKey).HasColumnName("dedupe_key").HasMaxLength(200);
        builder.Property(e => e.IsRead).HasColumnName("is_read").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ReadAt).HasColumnName("read_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.IsRead }).HasDatabaseName("ix_notifications_tenant_read");
        builder.HasIndex(e => new { e.TenantId, e.CreatedAt }).HasDatabaseName("ix_notifications_tenant_created");
        builder.HasIndex(e => e.DedupeKey).HasDatabaseName("ix_notifications_dedupe");
    }
}
