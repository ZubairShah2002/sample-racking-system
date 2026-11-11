using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using DomainNotification = SampleRack.Domain.SampleTracking.Entities.Notification;

namespace SampleRack.Application.SampleTracking.Commands.RefreshAgingAlerts;

/// <summary>
/// Recomputes aging for all stored samples and raises any missing aging alerts
/// (approaching limit, overdue, overdue 120+, overdue 180+). Idempotent via a
/// per-sample/per-bucket dedupe key, so repeated calls never duplicate alerts.
/// This replaces a scheduled daily job: aging itself is always current because
/// it is computed on read; this command only materializes the notifications.
/// </summary>
public sealed record RefreshAgingAlertsCommand : ICommand<int>;

internal sealed class RefreshAgingAlertsCommandHandler : ICommandHandler<RefreshAgingAlertsCommand, int>
{
    private readonly ISampleRepository _samples;
    private readonly INotificationRepository _notifications;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly ITenantContext _tenantContext;
    private readonly IClockService _clock;

    public RefreshAgingAlertsCommandHandler(
        ISampleRepository samples,
        INotificationRepository notifications,
        ISampleTrackingSettingsRepository settings,
        ITenantContext tenantContext,
        IClockService clock)
    {
        _samples = samples;
        _notifications = notifications;
        _settings = settings;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RefreshAgingAlertsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _settings.GetForTenantAsync(cancellationToken);
        if (settings is not null && !settings.NotificationsEnabled)
            return 0;

        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var now = _clock.UtcNow;

        var stored = await _samples.ListStoredAsync(cancellationToken);
        var created = 0;

        foreach (var sample in stored)
        {
            var days = Math.Max(0, today.DayNumber - sample.ReceivedDate.DayNumber);
            foreach (var alert in DetermineAlerts(days, thresholds))
            {
                var dedupeKey = $"{sample.Id}:{alert.type}";
                if (await _notifications.DedupeKeyExistsAsync(dedupeKey, cancellationToken))
                    continue;

                await _notifications.AddAsync(DomainNotification.Create(
                    Guid.NewGuid(), _tenantContext.CurrentTenantId, alert.type, alert.severity,
                    alert.title, alert.message(sample.SampleCode, days),
                    sample.Id, sample.SampleCode, dedupeKey, now), cancellationToken);
                created++;
            }
        }

        return created;
    }

    private static IEnumerable<(NotificationType type, NotificationSeverity severity, string title, Func<string, int, string> message)>
        DetermineAlerts(int days, Domain.SampleTracking.ValueObjects.AgingThresholds thresholds)
    {
        var overdue = thresholds.OverdueDays;

        if (days >= overdue + 180)
            yield return (NotificationType.Overdue180, NotificationSeverity.Critical, "Sample overdue 180+ days",
                (code, d) => $"Sample {code} has been stored {d} days ({d - overdue} days overdue).");
        else if (days >= overdue + 120)
            yield return (NotificationType.Overdue120, NotificationSeverity.Critical, "Sample overdue 120+ days",
                (code, d) => $"Sample {code} has been stored {d} days ({d - overdue} days overdue).");
        else if (days >= overdue)
            yield return (NotificationType.Overdue, NotificationSeverity.Critical, "Sample overdue",
                (code, d) => $"Sample {code} has been stored {d} days ({d - overdue} days overdue).");
        else if (days > thresholds.NormalMaxDays)
            yield return (NotificationType.ApproachingLimit, NotificationSeverity.Warning, "Sample approaching limit",
                (code, d) => $"Sample {code} has been stored {d} days, approaching the {overdue}-day limit.");
    }
}
