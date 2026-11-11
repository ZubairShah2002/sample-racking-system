using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class SampleStatusHistoryConfiguration : IEntityTypeConfiguration<SampleStatusHistory>
{
    public void Configure(EntityTypeBuilder<SampleStatusHistory> builder)
    {
        builder.ToTable("sample_status_history", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SampleId).HasColumnName("sample_id").IsRequired();
        builder.Property(e => e.PreviousStatus).HasColumnName("previous_status").HasMaxLength(50);
        builder.Property(e => e.NewStatus).HasColumnName("new_status").HasMaxLength(50).IsRequired();
        builder.Property(e => e.ChangedBy).HasColumnName("changed_by").HasMaxLength(150).IsRequired();
        builder.Property(e => e.ChangedAt).HasColumnName("changed_at").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(e => e.Remarks).HasColumnName("remarks").HasMaxLength(1000);

        builder.HasIndex(e => e.SampleId).HasDatabaseName("ix_sample_status_history_sample");
    }
}
