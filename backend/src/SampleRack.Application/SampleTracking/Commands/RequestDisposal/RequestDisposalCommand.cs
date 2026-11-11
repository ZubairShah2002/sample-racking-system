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

namespace SampleRack.Application.SampleTracking.Commands.RequestDisposal;

/// <summary>Raises a disposal request requiring authorized approval. Overdue samples are
/// never auto-disposed — disposal is always an explicit, approved action.</summary>
public sealed record RequestDisposalCommand(Guid Id, string Reason, string? Remarks) : ICommand<SampleDetailDto>;

public sealed class RequestDisposalCommandValidator : AbstractValidator<RequestDisposalCommand>
{
    public RequestDisposalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

internal sealed class RequestDisposalCommandHandler : ICommandHandler<RequestDisposalCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly INotificationRepository _notifications;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public RequestDisposalCommandHandler(
        ISampleRepository samples, IRackLocationRepository locations, ISampleTrackingSettingsRepository settings,
        INotificationRepository notifications, ITenantContext tenantContext, ICurrentUserService currentUser, IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _notifications = notifications;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(RequestDisposalCommand request, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.IsDisposed)
            return SampleTrackingErrors.SampleDisposed;

        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        sample.RequestDisposal(request.Reason, actor, request.Remarks, now);
        await _samples.UpdateAsync(sample, cancellationToken);

        await _notifications.AddAsync(DomainNotification.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, NotificationType.DisposalPendingApproval, NotificationSeverity.Warning,
            "Disposal pending approval",
            $"Disposal of sample {sample.SampleCode} is pending approval.",
            sample.Id, sample.SampleCode, dedupeKey: null, now), cancellationToken);

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var locationCode = sample.RackLocationId.HasValue
            ? (await _locations.GetByIdAsync(sample.RackLocationId.Value, cancellationToken))?.Code
            : null;

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode);
    }
}
