namespace SampleRack.Application.SampleTracking.DTOs;

/// <summary>Summary of a rack location including derived occupancy.</summary>
public sealed record RackLocationDto(
    Guid Id,
    string Code,
    string Zone,
    int Level,
    int Position,
    string? Description,
    bool IsActive,
    int SampleCount,
    int Capacity,
    int AvailableCapacity,
    bool HasOverdue,
    bool HasPendingTesting,
    bool HasTestingInProgress,
    string OccupancyState);

/// <summary>
/// Occupancy record consumed by the 3D visualization and rack overview. Encodes
/// the live condition of a location derived from the samples currently stored.
/// </summary>
public sealed record RackOccupancyDto(
    Guid Id,
    string Code,
    string Zone,
    int Level,
    int Position,
    bool IsActive,
    int SampleCount,
    int Capacity,
    int AvailableCapacity,
    bool HasOverdue,
    bool HasPendingTesting,
    bool HasTestingInProgress,
    string OccupancyState);

/// <summary>Full rack overview: the parametric structure plus live occupancy and thresholds.</summary>
public sealed record RackOverviewDto(
    IReadOnlyList<string> Zones,
    int MaxLevel,
    int MaxPosition,
    AgingThresholdsDto Thresholds,
    IReadOnlyList<RackOccupancyDto> Locations);

/// <summary>A rack location with the samples it currently holds.</summary>
public sealed record RackLocationDetailDto(
    Guid Id,
    string Code,
    string Zone,
    int Level,
    int Position,
    string? Description,
    bool IsActive,
    string OccupancyState,
    int SampleCount,
    int Capacity,
    int AvailableCapacity,
    IReadOnlyList<SampleSummaryDto> Samples);

public sealed record AgingThresholdsDto(int NewMaxDays, int NormalMaxDays, int OverdueDays);
