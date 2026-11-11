namespace SampleRack.Application.SampleTracking.DTOs;

/// <summary>
/// Serializable view of a sample's computed storage aging.
/// </summary>
public sealed record AgingSnapshotDto(
    DateOnly ReceivedDate,
    DateOnly CurrentDate,
    int TotalDaysStored,
    decimal StorageAgeMonths,
    int DaysRemainingBeforeOverdue,
    int DaysOverdue,
    string Category);
