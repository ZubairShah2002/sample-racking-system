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

namespace SampleRack.Application.SampleTracking.Commands.RecordTest;

/// <summary>Records a QC testing result against a sample. The sample becomes
/// TestedPass/TestedFail only when an authorized user records an actual outcome.</summary>
public sealed record RecordTestCommand(
    Guid Id,
    string TestingType,
    string? TestingStandard,
    DateTime TestingDate,
    string Outcome,
    string? Remarks,
    string? DefectDescription,
    string? PhotoEvidenceRef,
    string? ReportAttachmentRef) : ICommand<SampleDetailDto>;

public sealed class RecordTestCommandValidator : AbstractValidator<RecordTestCommand>
{
    public RecordTestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TestingType).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TestingStandard).MaximumLength(200);
        RuleFor(x => x.Outcome).NotEmpty();
        RuleFor(x => x.Remarks).MaximumLength(1000);
        RuleFor(x => x.DefectDescription).MaximumLength(2000);
    }
}

internal sealed class RecordTestCommandHandler : ICommandHandler<RecordTestCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly INotificationRepository _notifications;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public RecordTestCommandHandler(
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

    public async Task<Result<SampleDetailDto>> Handle(RecordTestCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TestOutcome>(request.Outcome, ignoreCase: true, out var outcome))
            return SampleTrackingErrors.InvalidTestOutcome;

        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.IsDisposed)
            return SampleTrackingErrors.SampleDisposed;

        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        // Normalize the client-supplied testing date to UTC — PostgreSQL 'timestamptz'
        // columns reject DateTimes with Kind=Unspecified (e.g. a date-only value).
        var testingDate = request.TestingDate.Kind switch
        {
            DateTimeKind.Utc => request.TestingDate,
            DateTimeKind.Local => request.TestingDate.ToUniversalTime(),
            _ => DateTime.SpecifyKind(request.TestingDate, DateTimeKind.Utc),
        };

        sample.RecordTest(
            request.TestingType, request.TestingStandard, testingDate, actor, outcome,
            request.Remarks, request.DefectDescription, request.PhotoEvidenceRef, request.ReportAttachmentRef, now);

        await _samples.UpdateAsync(sample, cancellationToken);

        if (outcome == TestOutcome.Fail)
        {
            await _notifications.AddAsync(DomainNotification.Create(
                Guid.NewGuid(), _tenantContext.CurrentTenantId, NotificationType.TestingFailed, NotificationSeverity.Critical,
                "Sample testing failed",
                $"Sample {sample.SampleCode} failed {request.TestingType} testing.",
                sample.Id, sample.SampleCode, dedupeKey: null, now), cancellationToken);
        }

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var locationCode = sample.RackLocationId.HasValue
            ? (await _locations.GetByIdAsync(sample.RackLocationId.Value, cancellationToken))?.Code
            : null;

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode);
    }
}
