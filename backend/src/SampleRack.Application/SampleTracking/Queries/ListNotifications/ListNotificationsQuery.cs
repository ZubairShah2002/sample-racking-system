using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.ListNotifications;

public sealed record ListNotificationsQuery(bool UnreadOnly, int Take) : IQuery<NotificationListDto>;

internal sealed class ListNotificationsQueryHandler : IQueryHandler<ListNotificationsQuery, NotificationListDto>
{
    private readonly INotificationRepository _notifications;

    public ListNotificationsQueryHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<Result<NotificationListDto>> Handle(ListNotificationsQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take is < 1 or > 200 ? 50 : request.Take;
        var items = await _notifications.ListRecentAsync(request.UnreadOnly, take, cancellationToken);
        var unread = await _notifications.CountUnreadAsync(cancellationToken);

        var dtos = items.Select(n => new NotificationDto(
            n.Id, n.Type, n.Severity, n.Title, n.Message, n.SampleId, n.SampleCode, n.IsRead, n.CreatedAt, n.ReadAt)).ToList();

        return new NotificationListDto(unread, dtos);
    }
}
