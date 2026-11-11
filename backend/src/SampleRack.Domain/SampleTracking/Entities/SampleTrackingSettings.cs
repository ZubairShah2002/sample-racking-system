using SampleRack.Domain.SampleTracking.ValueObjects;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// Per-tenant configuration for the rack management and sample tracking system.
/// Exactly one row exists per tenant. Aging thresholds are configurable here and
/// feed the aging engine, dashboard and rack overview.
/// </summary>
public sealed class SampleTrackingSettings : AggregateRoot<Guid>, IAuditableEntity
{
    public string? CompanyName { get; private set; }
    public string? DepartmentName { get; private set; }
    public string SampleIdPrefix { get; private set; } = "SMP";
    public int AgingNewMaxDays { get; private set; } = AgingThresholds.Default.NewMaxDays;
    public int AgingNormalMaxDays { get; private set; } = AgingThresholds.Default.NormalMaxDays;
    public int AgingOverdueDays { get; private set; } = AgingThresholds.Default.OverdueDays;
    public string DateFormat { get; private set; } = "dd/MM/yyyy";
    public string TimeZone { get; private set; } = "Asia/Kuala_Lumpur";
    public bool NotificationsEnabled { get; private set; } = true;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private SampleTrackingSettings() { }

    public static SampleTrackingSettings CreateDefault(Guid id, TenantId tenantId)
    {
        return new SampleTrackingSettings
        {
            Id = id,
            TenantId = tenantId,
            SampleIdPrefix = "SMP",
            AgingNewMaxDays = AgingThresholds.Default.NewMaxDays,
            AgingNormalMaxDays = AgingThresholds.Default.NormalMaxDays,
            AgingOverdueDays = AgingThresholds.Default.OverdueDays,
            DateFormat = "dd/MM/yyyy",
            TimeZone = "Asia/Kuala_Lumpur",
            NotificationsEnabled = true
        };
    }

    public AgingThresholds GetThresholds()
        => AgingThresholds.Create(AgingNewMaxDays, AgingNormalMaxDays, AgingOverdueDays);

    public void Update(
        string? companyName,
        string? departmentName,
        string? sampleIdPrefix,
        int? agingNewMaxDays,
        int? agingNormalMaxDays,
        int? agingOverdueDays,
        string? dateFormat,
        string? timeZone,
        bool? notificationsEnabled)
    {
        CompanyName = companyName?.Trim();
        DepartmentName = departmentName?.Trim();

        if (!string.IsNullOrWhiteSpace(sampleIdPrefix))
            SampleIdPrefix = sampleIdPrefix.Trim().ToUpperInvariant();

        var thresholds = AgingThresholds.Create(
            agingNewMaxDays ?? AgingNewMaxDays,
            agingNormalMaxDays ?? AgingNormalMaxDays,
            agingOverdueDays ?? AgingOverdueDays);
        AgingNewMaxDays = thresholds.NewMaxDays;
        AgingNormalMaxDays = thresholds.NormalMaxDays;
        AgingOverdueDays = thresholds.OverdueDays;

        if (!string.IsNullOrWhiteSpace(dateFormat))
            DateFormat = dateFormat.Trim();
        if (!string.IsNullOrWhiteSpace(timeZone))
            TimeZone = timeZone.Trim();
        if (notificationsEnabled.HasValue)
            NotificationsEnabled = notificationsEnabled.Value;
    }
}
