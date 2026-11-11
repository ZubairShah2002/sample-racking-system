using Asp.Versioning;
using SampleRack.Application.SampleTracking.Commands.RefreshAgingAlerts;
using SampleRack.Application.SampleTracking.Queries.GetAgingReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sample-aging")]
[Authorize]
public sealed class SampleAgingController : SampleTrackingControllerBase
{
    public SampleAgingController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetAgingReportQuery(category), cancellationToken));

    /// <summary>
    /// Materializes any missing aging alert notifications. Aging itself is always
    /// current (computed on read); this only generates the notification rows.
    /// </summary>
    [HttpPost("refresh-alerts")]
    [Authorize(Roles = SampleTrackingRoles.QcUser)]
    public async Task<IActionResult> RefreshAlerts(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new RefreshAgingAlertsCommand(), cancellationToken));
}
