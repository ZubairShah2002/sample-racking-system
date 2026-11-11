using Asp.Versioning;
using SampleRack.Application.SampleTracking.Commands.CreateRackLocation;
using SampleRack.Application.SampleTracking.Commands.SetRackLocationActive;
using SampleRack.Application.SampleTracking.Commands.UpdateRackLocation;
using SampleRack.Application.SampleTracking.Queries.GetRackLocationDetail;
using SampleRack.Application.SampleTracking.Queries.ListRackLocations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rack-locations")]
[Authorize]
public sealed class RackLocationsController : SampleTrackingControllerBase
{
    public RackLocationsController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = true,
        CancellationToken cancellationToken = default)
        => Respond(await Sender.Send(new ListRackLocationsQuery(search, includeInactive), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetRackLocationDetailQuery(id, null), cancellationToken));

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetRackLocationDetailQuery(null, code), cancellationToken));

    [HttpPost]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateRackLocationRequest request, CancellationToken cancellationToken)
        => RespondCreated(await Sender.Send(
            new CreateRackLocationCommand(request.Code, request.Description, request.Zone, request.Level, request.Position, request.Capacity),
            cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRackLocationRequest request, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(
            new UpdateRackLocationCommand(id, request.Description, request.Zone, request.Level, request.Position, request.Capacity),
            cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new SetRackLocationActiveCommand(id, true), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new SetRackLocationActiveCommand(id, false), cancellationToken));
}

public sealed record CreateRackLocationRequest(string Code, string? Description, string? Zone, int? Level, int? Position, int? Capacity);
public sealed record UpdateRackLocationRequest(string? Description, string? Zone, int? Level, int? Position, int? Capacity);
