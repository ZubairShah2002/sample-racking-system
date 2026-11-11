namespace SampleRack.Application.SampleTracking.DTOs;

/// <summary>A single labelled value for a chart series.</summary>
public sealed record ChartPointDto(string Label, int Value);

/// <summary>An overdue-sample alert row for the dashboard alert panel.</summary>
public sealed record OverdueAlertDto(
    Guid SampleId,
    string SampleCode,
    string SampleName,
    string? RackLocationCode,
    DateOnly ReceivedDate,
    int TotalDaysStored,
    int DaysOverdue,
    string AgingCategory);

/// <summary>Database-driven KPIs for the management dashboard.</summary>
public sealed record DashboardKpisDto(
    int TotalRegisteredSamples,
    int CurrentlyStoredSamples,
    int AvailableRackLocations,
    int OccupiedRackLocations,
    int PendingTesting,
    int TestingInProgress,
    int TestedPass,
    int TestedFail,
    int OverdueSamples,
    int ApproachingLimit,
    int WaitingForSupplier,
    int ToBeDisposed);

/// <summary>Complete dashboard payload: KPIs, charts and the overdue alert list.</summary>
public sealed record DashboardDto(
    DashboardKpisDto Kpis,
    IReadOnlyList<ChartPointDto> StatusDistribution,
    IReadOnlyList<ChartPointDto> AgingDistribution,
    IReadOnlyList<ChartPointDto> RackOccupancy,
    IReadOnlyList<ChartPointDto> MonthlyReceiving,
    IReadOnlyList<ChartPointDto> TestingPassFail,
    IReadOnlyList<ChartPointDto> SamplesBySupplier,
    IReadOnlyList<ChartPointDto> SamplesByCategory,
    IReadOnlyList<ChartPointDto> SamplesByDepartment,
    IReadOnlyList<ChartPointDto> MonthlyMovement,
    IReadOnlyList<OverdueAlertDto> OverdueAlerts,
    IReadOnlyList<SampleSummaryDto> RecentlyReceived);
