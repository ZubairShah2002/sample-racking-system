using Asp.Versioning;
using SampleRack.Application.Identity.Commands.CreateUser;
using SampleRack.Application.Identity.Commands.ResetPassword;
using SampleRack.Application.Identity.Commands.SetUserActive;
using SampleRack.Application.Identity.Commands.UpdateUser;
using SampleRack.Application.Identity.Queries.ListUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleRack.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize(Roles = ApiRoles.AdminOnly)]
public sealed class UsersController : ApiControllerBase
{
    public UsersController(ISender sender) : base(sender) { }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = true,
        CancellationToken cancellationToken = default)
        => Respond(await Sender.Send(new ListUsersQuery(search, includeInactive), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        => RespondCreated(await Sender.Send(
            new CreateUserCommand(request.Username, request.Email, request.FullName, request.Password, request.Roles),
            cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(
            new UpdateUserCommand(id, request.FullName, request.Email, request.Roles), cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new SetUserActiveCommand(id, true), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new SetUserActiveCommand(id, false), cancellationToken));

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        => RespondNoContent(await Sender.Send(new ResetPasswordCommand(id, request.NewPassword), cancellationToken));
}

public sealed record CreateUserRequest(string Username, string Email, string FullName, string Password, IReadOnlyList<string> Roles);
public sealed record UpdateUserRequest(string FullName, string Email, IReadOnlyList<string> Roles);
public sealed record ResetPasswordRequest(string NewPassword);
