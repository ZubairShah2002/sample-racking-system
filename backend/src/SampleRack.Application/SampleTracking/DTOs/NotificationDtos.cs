namespace SampleRack.Application.SampleTracking.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Severity,
    string Title,
    string Message,
    Guid? SampleId,
    string? SampleCode,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);

public sealed record NotificationListDto(
    int UnreadCount,
    IReadOnlyList<NotificationDto> Items);
