using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Application.SampleTracking.Queries.GetSettings;

public sealed record GetSettingsQuery : IQuery<SettingsDto>;

internal sealed class GetSettingsQueryHandler : IQueryHandler<GetSettingsQuery, SettingsDto>
{
    private readonly ISampleTrackingSettingsRepository _settings;

    public GetSettingsQueryHandler(ISampleTrackingSettingsRepository settings)
    {
        _settings = settings;
    }

    public async Task<Result<SettingsDto>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken)
            ?? SampleTrackingSettings.CreateDefault(Guid.Empty, TenantId.From(Guid.NewGuid()));

        return new SettingsDto(
            settings.CompanyName, settings.DepartmentName, settings.SampleIdPrefix,
            settings.AgingNewMaxDays, settings.AgingNormalMaxDays, settings.AgingOverdueDays,
            settings.DateFormat, settings.TimeZone, settings.NotificationsEnabled);
    }
}
