using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class TestingRecordConfiguration : IEntityTypeConfiguration<TestingRecord>
{
    public void Configure(EntityTypeBuilder<TestingRecord> builder)
    {
        builder.ToTable("testing_records", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SampleId).HasColumnName("sample_id").IsRequired();
        builder.Property(e => e.TestingType).HasColumnName("testing_type").HasMaxLength(150).IsRequired();
        builder.Property(e => e.TestingStandard).HasColumnName("testing_standard").HasMaxLength(200);
        builder.Property(e => e.TestingDate).HasColumnName("testing_date").IsRequired();
        builder.Property(e => e.TestedBy).HasColumnName("tested_by").HasMaxLength(150).IsRequired();
        builder.Property(e => e.Result).HasColumnName("result").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Remarks).HasColumnName("remarks").HasMaxLength(1000);
        builder.Property(e => e.DefectDescription).HasColumnName("defect_description").HasMaxLength(2000);
        builder.Property(e => e.PhotoEvidenceRef).HasColumnName("photo_evidence_ref").HasMaxLength(500);
        builder.Property(e => e.ReportAttachmentRef).HasColumnName("report_attachment_ref").HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(e => e.SampleId).HasDatabaseName("ix_testing_records_sample");
    }
}
