using Asp.Versioning;
using SampleRack.Application.SampleTracking.Commands.MarkAllNotificationsRead;
using SampleRack.Application.SampleTracking.Commands.MarkNotificationRead;
using SampleRack.Application.SampleTracking.Queries.ListNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sample-notifications")]
[Authorize]
public sealed class SampleNotificationsController : SampleTrackingControllerBase
{
    public SampleNotificationsController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Respond(await Sender.Send(new ListNotificationsQuery(unreadOnly, take), cancellationToken));

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new MarkNotificationReadCommand(id, true), cancellationToken));

    [HttpPost("{id:guid}/unread")]
    public async Task<IActionResult> MarkUnread(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new MarkNotificationReadCommand(id, false), cancellationToken));

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken));
}
