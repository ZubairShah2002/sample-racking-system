using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;
using DomainNotification = SampleRack.Domain.SampleTracking.Entities.Notification;

namespace SampleRack.Application.SampleTracking.Commands.ChangeSampleStatus;

/// <summary>Changes a sample's operational status (independent of aging). Appends to history.</summary>
public sealed record ChangeSampleStatusCommand(
    Guid Id,
    string Status,
    string? Reason,
    string? Remarks) : ICommand<SampleDetailDto>;

public sealed class ChangeSampleStatusCommandValidator : AbstractValidator<ChangeSampleStatusCommand>
{
    public ChangeSampleStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

internal sealed class ChangeSampleStatusCommandHandler : ICommandHandler<ChangeSampleStatusCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly INotificationRepository _notifications;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public ChangeSampleStatusCommandHandler(
        ISampleRepository samples,
        IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings,
        INotificationRepository notifications,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _notifications = notifications;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(ChangeSampleStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SampleStatus>(request.Status, ignoreCase: true, out var status))
            return SampleTrackingErrors.InvalidStatus;

        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.IsDisposed)
            return SampleTrackingErrors.SampleDisposed;
        if (status == SampleStatus.Disposed)
            return Error.Validation("SampleTracking.UseDisposalWorkflow",
                "Use the disposal workflow to dispose a sample.");

        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        sample.ChangeStatus(status, actor, request.Reason, request.Remarks, now);
        await _samples.UpdateAsync(sample, cancellationToken);

        await RaiseStatusNotificationAsync(sample, status, now, cancellationToken);

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var locationCode = sample.RackLocationId.HasValue
            ? (await _locations.GetByIdAsync(sample.RackLocationId.Value, cancellationToken))?.Code
            : null;

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode);
    }

    private async Task RaiseStatusNotificationAsync(Sample sample, SampleStatus status, DateTime now, CancellationToken ct)
    {
        (NotificationType type, NotificationSeverity severity, string title, string message)? n = status switch
        {
            SampleStatus.PendingTesting => (NotificationType.AwaitingTesting, NotificationSeverity.Info,
                "Sample awaiting testing", $"Sample {sample.SampleCode} is pending testing."),
            SampleStatus.WaitingForSupplier => (NotificationType.WaitingForSupplier, NotificationSeverity.Warning,
                "Sample waiting for supplier", $"Sample {sample.SampleCode} is waiting for supplier action."),
            _ => null
        };

        if (n is null) return;

        await _notifications.AddAsync(DomainNotification.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, n.Value.type, n.Value.severity,
            n.Value.title, n.Value.message, sample.Id, sample.SampleCode, dedupeKey: null, now), ct);
    }
}
