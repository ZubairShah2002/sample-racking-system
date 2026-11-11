using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.GetRackOverview;

/// <summary>
/// Returns the full rack structure plus live occupancy for every location,
/// driving the database-connected 3D visualization and rack overview.
/// </summary>
public sealed record GetRackOverviewQuery : IQuery<RackOverviewDto>;

internal sealed class GetRackOverviewQueryHandler : IQueryHandler<GetRackOverviewQuery, RackOverviewDto>
{
    private readonly IRackLocationRepository _locations;
    private readonly ISampleRepository _samples;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public GetRackOverviewQueryHandler(
        IRackLocationRepository locations,
        ISampleRepository samples,
        ISampleTrackingSettingsRepository settings,
        IClockService clock)
    {
        _locations = locations;
        _samples = samples;
        _settings = settings;
        _clock = clock;
    }

    public async Task<Result<RackOverviewDto>> Handle(GetRackOverviewQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        var locations = await _locations.ListOrderedAsync(includeInactive: true, cancellationToken);
        var stored = await _samples.ListStoredAsync(cancellationToken);
        var byLocation = stored
            .Where(s => s.RackLocationId.HasValue)
            .GroupBy(s => s.RackLocationId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<Sample>)g.ToList());

        var occupancy = locations.Select(l =>
        {
            var samples = byLocation.TryGetValue(l.Id, out var list) ? list : Array.Empty<Sample>();
            var condition = LocationCondition.Evaluate(samples, today, thresholds);
            return new RackOccupancyDto(
                l.Id, l.Code, l.Zone, l.Level, l.Position, l.IsActive,
                condition.SampleCount, l.Capacity, Math.Max(0, l.Capacity - condition.SampleCount),
                condition.HasOverdue, condition.HasPendingTesting,
                condition.HasTestingInProgress, condition.PrimaryState);
        }).ToList();

        var zones = locations.Select(l => l.Zone).Distinct().OrderBy(z => z).ToList();
        var maxLevel = locations.Count > 0 ? locations.Max(l => l.Level) : 0;
        var maxPosition = locations.Count > 0 ? locations.Max(l => l.Position) : 0;

        var dto = new RackOverviewDto(
            zones,
            maxLevel,
            maxPosition,
            new AgingThresholdsDto(thresholds.NewMaxDays, thresholds.NormalMaxDays, thresholds.OverdueDays),
            occupancy);

        return dto;
    }
}
