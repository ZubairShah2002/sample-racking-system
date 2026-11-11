using Asp.Versioning;
using SampleRack.Application.SampleTracking.Commands.CreateMasterDataItem;
using SampleRack.Application.SampleTracking.Commands.SetMasterDataItemActive;
using SampleRack.Application.SampleTracking.Commands.UpdateMasterDataItem;
using SampleRack.Application.SampleTracking.Queries.ListMasterData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

/// <summary>
/// Administrator-managed reference data (departments, product categories, material
/// types, testing methods) used to populate pickers across the sample-tracking
/// module. Reads are open to any authenticated user; writes require admin roles.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sample-master-data")]
[Authorize]
public sealed class SampleMasterDataController : SampleTrackingControllerBase
{
    public SampleMasterDataController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? category,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
        => Respond(await Sender.Send(new ListMasterDataQuery(category, includeInactive), cancellationToken));

    [HttpPost]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateMasterDataRequest request, CancellationToken cancellationToken)
        => RespondCreated(await Sender.Send(
            new CreateMasterDataItemCommand(request.Category, request.Name, request.Code, request.Description, request.SortOrder),
            cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMasterDataRequest request, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(
            new UpdateMasterDataItemCommand(id, request.Name, request.Code, request.Description, request.SortOrder),
            cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new SetMasterDataItemActiveCommand(id, true), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new SetMasterDataItemActiveCommand(id, false), cancellationToken));
}

public sealed record CreateMasterDataRequest(string Category, string Name, string? Code, string? Description, int? SortOrder);
public sealed record UpdateMasterDataRequest(string Name, string? Code, string? Description, int? SortOrder);
