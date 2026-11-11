using Asp.Versioning;
using SampleRack.Application.SampleTracking.Queries.GetRackOverview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

/// <summary>Serves the live, database-driven occupancy model consumed by the 3D rack visualization.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rack-overview")]
[Authorize]
public sealed class RackOverviewController : SampleTrackingControllerBase
{
    public RackOverviewController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetRackOverviewQuery(), cancellationToken));
}
