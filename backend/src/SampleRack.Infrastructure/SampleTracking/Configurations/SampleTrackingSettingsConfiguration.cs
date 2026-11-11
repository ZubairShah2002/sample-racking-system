using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class SampleTrackingSettingsConfiguration : IEntityTypeConfiguration<SampleTrackingSettings>
{
    public void Configure(EntityTypeBuilder<SampleTrackingSettings> builder)
    {
        builder.ToTable("settings", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(200);
        builder.Property(e => e.DepartmentName).HasColumnName("department_name").HasMaxLength(200);
        builder.Property(e => e.SampleIdPrefix).HasColumnName("sample_id_prefix").HasMaxLength(10).IsRequired();
        builder.Property(e => e.AgingNewMaxDays).HasColumnName("aging_new_max_days").IsRequired();
        builder.Property(e => e.AgingNormalMaxDays).HasColumnName("aging_normal_max_days").IsRequired();
        builder.Property(e => e.AgingOverdueDays).HasColumnName("aging_overdue_days").IsRequired();
        builder.Property(e => e.DateFormat).HasColumnName("date_format").HasMaxLength(50).IsRequired();
        builder.Property(e => e.TimeZone).HasColumnName("time_zone").HasMaxLength(100).IsRequired();
        builder.Property(e => e.NotificationsEnabled).HasColumnName("notifications_enabled").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => e.TenantId).IsUnique().HasDatabaseName("ix_settings_tenant");
    }
}
