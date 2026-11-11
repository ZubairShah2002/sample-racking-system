using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.UpdateSettings;

/// <summary>Updates the per-tenant sample tracking configuration, including aging thresholds.</summary>
public sealed record UpdateSettingsCommand(
    string? CompanyName,
    string? DepartmentName,
    string? SampleIdPrefix,
    int? AgingNewMaxDays,
    int? AgingNormalMaxDays,
    int? AgingOverdueDays,
    string? DateFormat,
    string? TimeZone,
    bool? NotificationsEnabled) : ICommand<SettingsDto>;

public sealed class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.DepartmentName).MaximumLength(200);
        RuleFor(x => x.SampleIdPrefix).MaximumLength(10);
        RuleFor(x => x.AgingNewMaxDays).GreaterThan(0).When(x => x.AgingNewMaxDays.HasValue);
        RuleFor(x => x.AgingNormalMaxDays).GreaterThan(0).When(x => x.AgingNormalMaxDays.HasValue);
        RuleFor(x => x.AgingOverdueDays).GreaterThan(0).When(x => x.AgingOverdueDays.HasValue);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}

internal sealed class UpdateSettingsCommandHandler : ICommandHandler<UpdateSettingsCommand, SettingsDto>
{
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly ITenantContext _tenantContext;

    public UpdateSettingsCommandHandler(ISampleTrackingSettingsRepository settings, ITenantContext tenantContext)
    {
        _settings = settings;
        _tenantContext = tenantContext;
    }

    public async Task<Result<SettingsDto>> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken);
        if (settings is null)
        {
            settings = SampleTrackingSettings.CreateDefault(Guid.NewGuid(), _tenantContext.CurrentTenantId);
            await _settings.AddAsync(settings, cancellationToken);
        }

        settings.Update(
            request.CompanyName, request.DepartmentName, request.SampleIdPrefix,
            request.AgingNewMaxDays, request.AgingNormalMaxDays, request.AgingOverdueDays,
            request.DateFormat, request.TimeZone, request.NotificationsEnabled);

        await _settings.UpdateAsync(settings, cancellationToken);

        return new SettingsDto(
            settings.CompanyName, settings.DepartmentName, settings.SampleIdPrefix,
            settings.AgingNewMaxDays, settings.AgingNormalMaxDays, settings.AgingOverdueDays,
            settings.DateFormat, settings.TimeZone, settings.NotificationsEnabled);
    }
}
