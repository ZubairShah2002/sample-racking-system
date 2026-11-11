using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.ListRackLocations;

/// <summary>Lists rack locations with live occupancy, optionally filtered by search text.</summary>
public sealed record ListRackLocationsQuery(string? Search, bool IncludeInactive)
    : IQuery<IReadOnlyList<RackLocationDto>>;

internal sealed class ListRackLocationsQueryHandler
    : IQueryHandler<ListRackLocationsQuery, IReadOnlyList<RackLocationDto>>
{
    private readonly IRackLocationRepository _locations;
    private readonly ISampleRepository _samples;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public ListRackLocationsQueryHandler(
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

    public async Task<Result<IReadOnlyList<RackLocationDto>>> Handle(
        ListRackLocationsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        var locations = await _locations.ListOrderedAsync(request.IncludeInactive, cancellationToken);
        var stored = await _samples.ListStoredAsync(cancellationToken);
        var byLocation = stored
            .Where(s => s.RackLocationId.HasValue)
            .GroupBy(s => s.RackLocationId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<Sample>)g.ToList());

        var search = request.Search?.Trim().ToUpperInvariant();

        var result = locations
            .Where(l => string.IsNullOrEmpty(search)
                        || l.Code.Contains(search)
                        || l.Zone.Contains(search)
                        || (l.Description ?? string.Empty).ToUpperInvariant().Contains(search))
            .Select(l =>
            {
                var samples = byLocation.TryGetValue(l.Id, out var list) ? list : Array.Empty<Sample>();
                var condition = LocationCondition.Evaluate(samples, today, thresholds);
                return new RackLocationDto(
                    l.Id, l.Code, l.Zone, l.Level, l.Position, l.Description, l.IsActive,
                    condition.SampleCount, l.Capacity, Math.Max(0, l.Capacity - condition.SampleCount),
                    condition.HasOverdue, condition.HasPendingTesting,
                    condition.HasTestingInProgress, condition.PrimaryState);
            })
            .ToList();

        return Result.Success<IReadOnlyList<RackLocationDto>>(result);
    }
}
