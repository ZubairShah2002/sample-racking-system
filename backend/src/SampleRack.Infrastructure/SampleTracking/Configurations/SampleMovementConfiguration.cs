using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class SampleMovementConfiguration : IEntityTypeConfiguration<SampleMovement>
{
    public void Configure(EntityTypeBuilder<SampleMovement> builder)
    {
        builder.ToTable("sample_movements", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SampleId).HasColumnName("sample_id").IsRequired();
        builder.Property(e => e.FromLocationId).HasColumnName("from_location_id");
        builder.Property(e => e.FromLocationCode).HasColumnName("from_location_code").HasMaxLength(50);
        builder.Property(e => e.ToLocationId).HasColumnName("to_location_id").IsRequired();
        builder.Property(e => e.ToLocationCode).HasColumnName("to_location_code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.MovedBy).HasColumnName("moved_by").HasMaxLength(150).IsRequired();
        builder.Property(e => e.MovedAt).HasColumnName("moved_at").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.Property(e => e.Remarks).HasColumnName("remarks").HasMaxLength(1000);

        builder.HasIndex(e => e.SampleId).HasDatabaseName("ix_sample_movements_sample");
        builder.HasIndex(e => new { e.TenantId, e.MovedAt }).HasDatabaseName("ix_sample_movements_tenant_moved");
    }
}
