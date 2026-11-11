using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.ValueObjects;

namespace SampleRack.Domain.SampleTracking;

/// <summary>
/// Pure, deterministic calculation of sample storage aging. Centralizes the
/// aging rules so the dashboard, aging module, rack overview and sample detail
/// all report identical figures. The received date is always the anchor;
/// movement between locations never resets it.
/// </summary>
public static class AgingCalculator
{
    private const decimal DaysPerMonth = 30.4375m;

    /// <summary>
    /// Builds an aging snapshot for a sample received on <paramref name="receivedDate"/>,
    /// evaluated as of <paramref name="asOf"/>, using the supplied thresholds.
    /// </summary>
    public static SampleAgingSnapshot Calculate(DateOnly receivedDate, DateOnly asOf, AgingThresholds thresholds)
    {
        // Clamp to zero: a received date in the future (data entry edge case)
        // must never yield a negative storage age.
        var totalDays = Math.Max(0, asOf.DayNumber - receivedDate.DayNumber);

        var ageMonths = Math.Round(totalDays / DaysPerMonth, 1, MidpointRounding.AwayFromZero);

        var daysRemaining = thresholds.OverdueDays - totalDays;
        var daysOverdue = Math.Max(0, totalDays - thresholds.OverdueDays);

        var category = Classify(totalDays, thresholds);

        return new SampleAgingSnapshot(
            receivedDate,
            asOf,
            totalDays,
            ageMonths,
            daysRemaining,
            daysOverdue,
            category);
    }

    /// <summary>
    /// Classifies a stored-day count into an aging category using the thresholds.
    /// </summary>
    public static AgingCategory Classify(int totalDaysStored, AgingThresholds thresholds)
    {
        if (totalDaysStored >= thresholds.OverdueDays)
            return AgingCategory.Overdue;
        if (totalDaysStored > thresholds.NormalMaxDays)
            return AgingCategory.ApproachingLimit;
        if (totalDaysStored > thresholds.NewMaxDays)
            return AgingCategory.Normal;
        return AgingCategory.New;
    }
}
