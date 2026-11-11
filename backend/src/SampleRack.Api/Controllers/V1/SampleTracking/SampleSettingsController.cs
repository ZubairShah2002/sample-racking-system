using Asp.Versioning;
using SampleRack.Application.SampleTracking.Commands.UpdateSettings;
using SampleRack.Application.SampleTracking.Queries.GetSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1.SampleTracking;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sample-settings")]
[Authorize]
public sealed class SampleSettingsController : SampleTrackingControllerBase
{
    public SampleSettingsController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetSettingsQuery(), cancellationToken));

    [HttpPut]
    [Authorize(Roles = SampleTrackingRoles.Admin)]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest r, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new UpdateSettingsCommand(
            r.CompanyName, r.DepartmentName, r.SampleIdPrefix, r.AgingNewMaxDays, r.AgingNormalMaxDays,
            r.AgingOverdueDays, r.DateFormat, r.TimeZone, r.NotificationsEnabled), cancellationToken));
}

public sealed record UpdateSettingsRequest(
    string? CompanyName, string? DepartmentName, string? SampleIdPrefix, int? AgingNewMaxDays,
    int? AgingNormalMaxDays, int? AgingOverdueDays, string? DateFormat, string? TimeZone, bool? NotificationsEnabled);
