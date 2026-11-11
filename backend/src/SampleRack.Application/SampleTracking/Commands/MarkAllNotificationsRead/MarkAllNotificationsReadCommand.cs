using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Commands.MarkAllNotificationsRead;

/// <summary>Marks every unread notification read for the current tenant.</summary>
public sealed record MarkAllNotificationsReadCommand : ICommand<int>;

internal sealed class MarkAllNotificationsReadCommandHandler : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    private readonly INotificationRepository _notifications;
    private readonly IClockService _clock;

    public MarkAllNotificationsReadCommandHandler(INotificationRepository notifications, IClockService clock)
    {
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _notifications.ListRecentAsync(unreadOnly: true, take: 1000, cancellationToken);
        var now = _clock.UtcNow;
        foreach (var n in unread)
        {
            n.MarkRead(now);
            await _notifications.UpdateAsync(n, cancellationToken);
        }
        return unread.Count;
    }
}
