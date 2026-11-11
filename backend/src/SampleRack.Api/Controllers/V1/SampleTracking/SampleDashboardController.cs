using Asp.Versioning;
using SampleRack.Application.SampleTracking.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sample-dashboard")]
[Authorize]
public sealed class SampleDashboardController : SampleTrackingControllerBase
{
    public SampleDashboardController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetDashboardQuery(), cancellationToken));
}
