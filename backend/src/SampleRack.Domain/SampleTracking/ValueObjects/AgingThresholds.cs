namespace SampleRack.Domain.SampleTracking.ValueObjects;

/// <summary>
/// Configurable day boundaries that classify a sample's storage aging.
/// Defaults follow the operational specification: 0-30 New, 31-60 Normal,
/// 61-89 Approaching Limit, 90+ Overdue.
/// </summary>
public readonly record struct AgingThresholds(int NewMaxDays, int NormalMaxDays, int OverdueDays)
{
    /// <summary>The operational default thresholds (30 / 60 / 90 days).</summary>
    public static readonly AgingThresholds Default = new(30, 60, 90);

    /// <summary>
    /// Creates thresholds, falling back to defaults for any non-positive value
    /// and guaranteeing a strictly increasing sequence.
    /// </summary>
    public static AgingThresholds Create(int? newMaxDays, int? normalMaxDays, int? overdueDays)
    {
        var newMax = newMaxDays is > 0 ? newMaxDays.Value : Default.NewMaxDays;
        var normalMax = normalMaxDays is > 0 ? normalMaxDays.Value : Default.NormalMaxDays;
        var overdue = overdueDays is > 0 ? overdueDays.Value : Default.OverdueDays;

        if (normalMax <= newMax) normalMax = newMax + 1;
        if (overdue <= normalMax) overdue = normalMax + 1;

        return new AgingThresholds(newMax, normalMax, overdue);
    }
}
