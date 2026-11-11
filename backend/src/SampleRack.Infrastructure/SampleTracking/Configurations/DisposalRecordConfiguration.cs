using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class DisposalRecordConfiguration : IEntityTypeConfiguration<DisposalRecord>
{
    public void Configure(EntityTypeBuilder<DisposalRecord> builder)
    {
        builder.ToTable("disposal_records", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SampleId).HasColumnName("sample_id").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        builder.Property(e => e.RequestedBy).HasColumnName("requested_by").HasMaxLength(150).IsRequired();
        builder.Property(e => e.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(e => e.ApprovalStatus).HasColumnName("approval_status").HasMaxLength(20).IsRequired();
        builder.Property(e => e.ApprovedBy).HasColumnName("approved_by").HasMaxLength(150);
        builder.Property(e => e.ApprovedAt).HasColumnName("approved_at");
        builder.Property(e => e.DisposalDate).HasColumnName("disposal_date");
        builder.Property(e => e.Remarks).HasColumnName("remarks").HasMaxLength(1000);

        builder.HasIndex(e => e.SampleId).IsUnique().HasDatabaseName("ix_disposal_records_sample");
    }
}
