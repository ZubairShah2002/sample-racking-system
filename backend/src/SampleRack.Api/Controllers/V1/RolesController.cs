using Asp.Versioning;
using SampleRack.Application.Identity.Queries.ListRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
[Authorize]
public sealed class RolesController : ApiControllerBase
{
    public RolesController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new ListRolesQuery(), cancellationToken));
}
