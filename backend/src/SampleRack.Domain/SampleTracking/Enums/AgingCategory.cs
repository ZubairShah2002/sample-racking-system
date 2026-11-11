namespace SampleRack.Domain.SampleTracking.Enums;

/// <summary>
/// Storage aging category derived from the number of days a sample has been
/// stored, measured from its original received date. Thresholds are
/// configurable; the default boundaries are 30 / 60 / 90 days.
/// </summary>
public enum AgingCategory
{
    /// <summary>0 to the "new" threshold (default 30 days).</summary>
    New,

    /// <summary>Between the "new" and "normal" thresholds (default 31-60 days).</summary>
    Normal,

    /// <summary>Between the "normal" and "overdue" thresholds (default 61-89 days).</summary>
    ApproachingLimit,

    /// <summary>At or beyond the overdue threshold (default 90 days and above).</summary>
    Overdue
}
