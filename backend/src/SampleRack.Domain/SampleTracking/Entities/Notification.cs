using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// A notification raised by the sample tracking system (new sample, aging
/// alerts, testing failures, disposal pending approval, etc.). Supports
/// read/unread state and links back to the originating sample where applicable.
/// </summary>
public sealed class Notification : AggregateRoot<Guid>
{
    public string Type { get; private set; } = string.Empty;
    public string Severity { get; private set; } = NotificationSeverity.Info.ToString();
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public Guid? SampleId { get; private set; }
    public string? SampleCode { get; private set; }

    /// <summary>
    /// Stable de-duplication key so recurring derived alerts (e.g. an aging
    /// overdue alert for a given sample) are not raised twice.
    /// </summary>
    public string? DedupeKey { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid id,
        TenantId tenantId,
        NotificationType type,
        NotificationSeverity severity,
        string title,
        string message,
        Guid? sampleId,
        string? sampleCode,
        string? dedupeKey,
        DateTime now)
    {
        return new Notification
        {
            Id = id,
            TenantId = tenantId,
            Type = type.ToString(),
            Severity = severity.ToString(),
            Title = title.Trim(),
            Message = message.Trim(),
            SampleId = sampleId,
            SampleCode = sampleCode,
            DedupeKey = dedupeKey,
            IsRead = false,
            CreatedAt = now
        };
    }

    public void MarkRead(DateTime now)
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = now;
    }

    public void MarkUnread()
    {
        IsRead = false;
        ReadAt = null;
    }
}
