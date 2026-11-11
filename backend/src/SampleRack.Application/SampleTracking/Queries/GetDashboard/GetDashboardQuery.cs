using System.Globalization;
using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.GetDashboard;

/// <summary>Database-driven KPIs, charts and overdue alerts for the management dashboard.</summary>
public sealed record GetDashboardQuery : IQuery<DashboardDto>;

internal sealed class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public GetDashboardQueryHandler(
        ISampleRepository samples, IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings, IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _clock = clock;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        var all = await _samples.ListAllForAggregationAsync(includeDisposed: true, cancellationToken);
        var locations = await _locations.ListOrderedAsync(includeInactive: true, cancellationToken);
        var codeById = locations.ToDictionary(l => l.Id, l => l.Code);

        var disposedStatus = SampleStatus.Disposed.ToString();
        var active = all.Where(s => s.Status != disposedStatus).ToList();
        var stored = active.Where(s => s.RackLocationId.HasValue).ToList();

        int StatusCount(SampleStatus st) => active.Count(s => s.Status == st.ToString());

        var storedByLocation = stored
            .GroupBy(s => s.RackLocationId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
        var activeLocations = locations.Where(l => l.IsActive).ToList();
        var occupied = activeLocations.Count(l => storedByLocation.ContainsKey(l.Id));
        var available = activeLocations.Count - occupied;

        int AgingCount(AgingCategory cat) => stored.Count(s =>
            AgingCalculator.Classify(Math.Max(0, today.DayNumber - s.ReceivedDate.DayNumber), thresholds) == cat);

        var kpis = new DashboardKpisDto(
            TotalRegisteredSamples: all.Count,
            CurrentlyStoredSamples: stored.Count,
            AvailableRackLocations: available,
            OccupiedRackLocations: occupied,
            PendingTesting: StatusCount(SampleStatus.PendingTesting),
            TestingInProgress: StatusCount(SampleStatus.TestingInProgress),
            TestedPass: StatusCount(SampleStatus.TestedPass),
            TestedFail: StatusCount(SampleStatus.TestedFail),
            OverdueSamples: AgingCount(AgingCategory.Overdue),
            ApproachingLimit: AgingCount(AgingCategory.ApproachingLimit),
            WaitingForSupplier: StatusCount(SampleStatus.WaitingForSupplier),
            ToBeDisposed: StatusCount(SampleStatus.ToBeDisposed));

        // Status distribution (all statuses present among active samples)
        var statusDistribution = active
            .GroupBy(s => s.Status)
            .Select(g => new ChartPointDto(Humanize(g.Key), g.Count()))
            .OrderByDescending(p => p.Value)
            .ToList();

        var agingDistribution = new List<ChartPointDto>
        {
            new("New", AgingCount(AgingCategory.New)),
            new("Normal", AgingCount(AgingCategory.Normal)),
            new("Approaching Limit", AgingCount(AgingCategory.ApproachingLimit)),
            new("Overdue", AgingCount(AgingCategory.Overdue)),
        };

        var rackOccupancy = new List<ChartPointDto>
        {
            new("Occupied", occupied),
            new("Available", available),
        };

        var monthlyReceiving = BuildMonthlySeries(today, 12, month =>
            all.Count(s => s.ReceivedDate.Year == month.Year && s.ReceivedDate.Month == month.Month));

        var testingPassFail = new List<ChartPointDto>
        {
            new("Pass", StatusCount(SampleStatus.TestedPass)),
            new("Fail", StatusCount(SampleStatus.TestedFail)),
        };

        var samplesBySupplier = active
            .Where(s => !string.IsNullOrWhiteSpace(s.SupplierName))
            .GroupBy(s => s.SupplierName!)
            .Select(g => new ChartPointDto(g.Key, g.Count()))
            .OrderByDescending(p => p.Value)
            .Take(8)
            .ToList();

        var samplesByCategory = active
            .Where(s => !string.IsNullOrWhiteSpace(s.ProductCategory))
            .GroupBy(s => s.ProductCategory!)
            .Select(g => new ChartPointDto(g.Key, g.Count()))
            .OrderByDescending(p => p.Value)
            .Take(8)
            .ToList();

        var samplesByDepartment = active
            .Where(s => !string.IsNullOrWhiteSpace(s.Department))
            .GroupBy(s => s.Department!)
            .Select(g => new ChartPointDto(g.Key, g.Count()))
            .OrderByDescending(p => p.Value)
            .Take(8)
            .ToList();

        var recentlyReceived = all
            .OrderByDescending(s => s.ReceivedDate)
            .ThenByDescending(s => s.CreatedAt)
            .Take(8)
            .Select(s => SampleTrackingMapper.ToSummaryDto(
                s, today, thresholds,
                s.RackLocationId.HasValue && codeById.TryGetValue(s.RackLocationId.Value, out var c) ? c : null))
            .ToList();

        var fromMonth = new DateOnly(today.AddMonths(-11).Year, today.AddMonths(-11).Month, 1);
        var movementCounts = await _samples.GetMonthlyMovementCountsAsync(fromMonth, cancellationToken);
        var movementLookup = movementCounts.ToDictionary(m => (m.Year, m.Month), m => m.Count);
        var monthlyMovement = BuildMonthlySeries(today, 12, month =>
            movementLookup.TryGetValue((month.Year, month.Month), out var c) ? c : 0);

        var overdueAlerts = stored
            .Select(s => new
            {
                Sample = s,
                Snap = AgingCalculator.Calculate(s.ReceivedDate, today, thresholds)
            })
            .Where(x => x.Snap.Category == AgingCategory.Overdue)
            .OrderByDescending(x => x.Snap.DaysOverdue)
            .Take(50)
            .Select(x => new OverdueAlertDto(
                x.Sample.Id, x.Sample.SampleCode, x.Sample.SampleName,
                x.Sample.RackLocationId.HasValue && codeById.TryGetValue(x.Sample.RackLocationId.Value, out var c) ? c : null,
                x.Sample.ReceivedDate, x.Snap.TotalDaysStored, x.Snap.DaysOverdue, x.Snap.Category.ToString()))
            .ToList();

        return new DashboardDto(
            kpis, statusDistribution, agingDistribution, rackOccupancy, monthlyReceiving,
            testingPassFail, samplesBySupplier, samplesByCategory, samplesByDepartment,
            monthlyMovement, overdueAlerts, recentlyReceived);
    }

    private static List<ChartPointDto> BuildMonthlySeries(DateOnly today, int months, Func<DateOnly, int> counter)
    {
        var series = new List<ChartPointDto>();
        for (var i = months - 1; i >= 0; i--)
        {
            var month = today.AddMonths(-i);
            var label = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month.Month)} {month:yy}";
            series.Add(new ChartPointDto(label, counter(month)));
        }
        return series;
    }

    private static string Humanize(string status)
    {
        // Insert spaces before capitals: "PendingTesting" -> "Pending Testing".
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < status.Length; i++)
        {
            if (i > 0 && char.IsUpper(status[i])) sb.Append(' ');
            sb.Append(status[i]);
        }
        return sb.ToString();
    }
}
