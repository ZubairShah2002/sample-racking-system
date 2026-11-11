using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class RackLocationConfiguration : IEntityTypeConfiguration<RackLocation>
{
    public void Configure(EntityTypeBuilder<RackLocation> builder)
    {
        builder.ToTable("rack_locations", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(e => e.Zone).HasColumnName("zone").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Level).HasColumnName("level").IsRequired();
        builder.Property(e => e.Position).HasColumnName("position").IsRequired();
        builder.Property(e => e.SortOrder).HasColumnName("sort_order").IsRequired();
        builder.Property(e => e.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique().HasDatabaseName("ix_rack_locations_tenant_code");
        builder.HasIndex(e => new { e.TenantId, e.IsActive }).HasDatabaseName("ix_rack_locations_tenant_active");
    }
}
