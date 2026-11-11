using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.GetSampleByCode;

/// <summary>Resolves a sample by its code — used by QR/barcode scanning.</summary>
public sealed record GetSampleByCodeQuery(string Code) : IQuery<SampleDetailDto>;

internal sealed class GetSampleByCodeQueryHandler : IQueryHandler<GetSampleByCodeQuery, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public GetSampleByCodeQueryHandler(
        ISampleRepository samples, IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings, IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(GetSampleByCodeQuery request, CancellationToken cancellationToken)
    {
        var header = await _samples.GetByCodeAsync(request.Code, cancellationToken);
        if (header is null)
            return SampleTrackingErrors.SampleNotFound;

        var sample = await _samples.GetFullDetailAsync(header.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var locationCode = sample.RackLocationId.HasValue
            ? (await _locations.GetByIdAsync(sample.RackLocationId.Value, cancellationToken))?.Code
            : null;

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode);
    }
}
