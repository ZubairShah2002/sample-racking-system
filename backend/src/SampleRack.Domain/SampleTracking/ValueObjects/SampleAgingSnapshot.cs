using SampleRack.Domain.SampleTracking.Enums;

namespace SampleRack.Domain.SampleTracking.ValueObjects;

/// <summary>
/// A point-in-time view of a sample's storage aging, derived entirely from its
/// original received date and the current date. This is a computed value and is
/// never persisted — it is always current when read.
/// </summary>
public sealed record SampleAgingSnapshot(
    DateOnly ReceivedDate,
    DateOnly CurrentDate,
    int TotalDaysStored,
    decimal StorageAgeMonths,
    int DaysRemainingBeforeOverdue,
    int DaysOverdue,
    AgingCategory Category);
