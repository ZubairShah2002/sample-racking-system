using Asp.Versioning;
using SampleRack.Application.Identity.Commands.ChangePassword;
using SampleRack.Application.Identity.Commands.Login;
using SampleRack.Application.Identity.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ApiControllerBase
{
    public AuthController(ISender sender) : base(sender) { }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Respond(await Sender.Send(new LoginCommand(request.Username, request.Password), cancellationToken));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
        => Respond(await Sender.Send(new GetCurrentUserQuery(), cancellationToken));

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(
            new ChangePasswordCommand(request.CurrentPassword, request.NewPassword), cancellationToken));
}

public sealed record LoginRequest(string Username, string Password);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
