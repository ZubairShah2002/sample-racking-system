using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.GetAgingReport;

/// <summary>Returns stored samples with their computed aging, optionally filtered by category,
/// ordered most-overdue first.</summary>
public sealed record GetAgingReportQuery(string? Category) : IQuery<IReadOnlyList<SampleSummaryDto>>;

internal sealed class GetAgingReportQueryHandler : IQueryHandler<GetAgingReportQuery, IReadOnlyList<SampleSummaryDto>>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public GetAgingReportQueryHandler(
        ISampleRepository samples, IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings, IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<SampleSummaryDto>>> Handle(GetAgingReportQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        var locations = await _locations.ListOrderedAsync(includeInactive: true, cancellationToken);
        var codeById = locations.ToDictionary(l => l.Id, l => l.Code);

        AgingCategory? filter = null;
        if (!string.IsNullOrWhiteSpace(request.Category)
            && Enum.TryParse<AgingCategory>(request.Category, ignoreCase: true, out var parsed))
            filter = parsed;

        var stored = await _samples.ListStoredAsync(cancellationToken);

        var result = stored
            .Select(s => new
            {
                Sample = s,
                Category = AgingCalculator.Classify(Math.Max(0, today.DayNumber - s.ReceivedDate.DayNumber), thresholds)
            })
            .Where(x => filter is null || x.Category == filter)
            .Select(x => SampleTrackingMapper.ToSummaryDto(
                x.Sample, today, thresholds,
                x.Sample.RackLocationId.HasValue && codeById.TryGetValue(x.Sample.RackLocationId.Value, out var c) ? c : null))
            .OrderByDescending(d => d.TotalDaysStored)
            .ToList();

        return Result.Success<IReadOnlyList<SampleSummaryDto>>(result);
    }
}
