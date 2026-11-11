using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.MarkNotificationRead;

/// <summary>Marks a single notification read or unread.</summary>
public sealed record MarkNotificationReadCommand(Guid Id, bool IsRead) : ICommand;

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

internal sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand>
{
    private readonly INotificationRepository _notifications;
    private readonly IClockService _clock;

    public MarkNotificationReadCommandHandler(INotificationRepository notifications, IClockService clock)
    {
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.Id, cancellationToken);
        if (notification is null)
            return Result.Failure(SampleTrackingErrors.NotificationNotFound);

        if (request.IsRead)
            notification.MarkRead(_clock.UtcNow);
        else
            notification.MarkUnread();

        await _notifications.UpdateAsync(notification, cancellationToken);
        return Result.Success();
    }
}
