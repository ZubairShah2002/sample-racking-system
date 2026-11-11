using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.ValueObjects;

namespace SampleRack.Application.SampleTracking.Common;

/// <summary>
/// Canonical occupancy-state names surfaced to the 3D visualization and rack
/// overview. These are data values, not colours — the frontend maps each to a
/// configurable colour, so conditions and colours are never hardcoded together.
/// </summary>
public static class OccupancyState
{
    public const string Available = "Available";
    public const string Occupied = "Occupied";
    public const string Overdue = "Overdue";
    public const string PendingTesting = "PendingTesting";
    public const string TestingInProgress = "TestingInProgress";
}

/// <summary>
/// Derives a location's live condition from the samples currently stored in it.
/// </summary>
public sealed record LocationCondition(
    int SampleCount,
    bool HasOverdue,
    bool HasPendingTesting,
    bool HasTestingInProgress,
    string PrimaryState)
{
    public static LocationCondition Evaluate(
        IReadOnlyCollection<Sample> storedSamples,
        DateOnly today,
        AgingThresholds thresholds)
    {
        if (storedSamples.Count == 0)
            return new LocationCondition(0, false, false, false, OccupancyState.Available);

        var hasOverdue = storedSamples.Any(s =>
            AgingCalculator.Classify(
                Math.Max(0, today.DayNumber - s.ReceivedDate.DayNumber), thresholds) == AgingCategory.Overdue);

        var hasPendingTesting = storedSamples.Any(s => s.Status == SampleStatus.PendingTesting.ToString());
        var hasTestingInProgress = storedSamples.Any(s => s.Status == SampleStatus.TestingInProgress.ToString());

        // Primary state for single-colour rendering uses a severity priority.
        // The detail panel still exposes every flag that applies.
        var primary = hasOverdue
            ? OccupancyState.Overdue
            : hasTestingInProgress
                ? OccupancyState.TestingInProgress
                : hasPendingTesting
                    ? OccupancyState.PendingTesting
                    : OccupancyState.Occupied;

        return new LocationCondition(
            storedSamples.Count, hasOverdue, hasPendingTesting, hasTestingInProgress, primary);
    }
}
