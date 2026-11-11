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

namespace SampleRack.Application.SampleTracking.Commands.MoveSample;

/// <summary>Moves a sample to a new rack location. Storage aging is never reset by a move;
/// movement into an inactive location is rejected.</summary>
public sealed record MoveSampleCommand(
    Guid Id,
    Guid ToLocationId,
    string? Reason,
    string? Remarks) : ICommand<SampleDetailDto>;

public sealed class MoveSampleCommandValidator : AbstractValidator<MoveSampleCommand>
{
    public MoveSampleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ToLocationId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

internal sealed class MoveSampleCommandHandler : ICommandHandler<MoveSampleCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly INotificationRepository _notifications;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public MoveSampleCommandHandler(
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

    public async Task<Result<SampleDetailDto>> Handle(MoveSampleCommand request, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.IsDisposed)
            return SampleTrackingErrors.SampleDisposed;

        var target = await _locations.GetByIdAsync(request.ToLocationId, cancellationToken);
        if (target is null)
            return SampleTrackingErrors.LocationNotFound;
        if (!target.IsActive)
            return SampleTrackingErrors.LocationInactive;

        var fromCode = sample.RackLocationId.HasValue
            ? (await _locations.GetByIdAsync(sample.RackLocationId.Value, cancellationToken))?.Code
            : null;

        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        sample.MoveTo(target.Id, target.Code, fromCode, actor, request.Reason, request.Remarks, now);
        await _samples.UpdateAsync(sample, cancellationToken);

        await _notifications.AddAsync(DomainNotification.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, NotificationType.SampleMoved, NotificationSeverity.Info,
            "Sample moved",
            $"Sample {sample.SampleCode} moved from {fromCode ?? "—"} to {target.Code}.",
            sample.Id, sample.SampleCode, dedupeKey: null, now), cancellationToken);

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, target.Code);
    }
}
