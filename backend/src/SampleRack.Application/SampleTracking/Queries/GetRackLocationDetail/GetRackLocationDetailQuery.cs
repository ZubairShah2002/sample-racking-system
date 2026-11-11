using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.GetRackLocationDetail;

/// <summary>Returns a rack location with the samples it currently holds. Resolves by id or code.</summary>
public sealed record GetRackLocationDetailQuery(Guid? Id, string? Code) : IQuery<RackLocationDetailDto>;

internal sealed class GetRackLocationDetailQueryHandler
    : IQueryHandler<GetRackLocationDetailQuery, RackLocationDetailDto>
{
    private readonly IRackLocationRepository _locations;
    private readonly ISampleRepository _samples;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public GetRackLocationDetailQueryHandler(
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

    public async Task<Result<RackLocationDetailDto>> Handle(
        GetRackLocationDetailQuery request, CancellationToken cancellationToken)
    {
        var location = request.Id.HasValue
            ? await _locations.GetByIdAsync(request.Id.Value, cancellationToken)
            : !string.IsNullOrWhiteSpace(request.Code)
                ? await _locations.GetByCodeAsync(request.Code!, cancellationToken)
                : null;

        if (location is null)
            return SampleTrackingErrors.LocationNotFound;

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        var stored = await _samples.ListStoredInLocationAsync(location.Id, cancellationToken);
        var condition = LocationCondition.Evaluate(stored, today, thresholds);

        var samples = stored
            .Select(s => SampleTrackingMapper.ToSummaryDto(s, today, thresholds, location.Code))
            .OrderBy(s => s.SampleCode)
            .ToList();

        var dto = new RackLocationDetailDto(
            location.Id, location.Code, location.Zone, location.Level, location.Position,
            location.Description, location.IsActive, condition.PrimaryState, condition.SampleCount,
            location.Capacity, Math.Max(0, location.Capacity - condition.SampleCount), samples);

        return dto;
    }
}
