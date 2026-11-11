using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Identity.DTOs;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.Identity.Queries.GetCurrentUser;

/// <summary>Returns the profile of the currently authenticated user.</summary>
public sealed record GetCurrentUserQuery : IQuery<UserDto>;

internal sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(IUserRepository users, ICurrentUserService currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user is null)
            return IdentityErrors.UserNotFound;

        return new UserDto(
            user.Id, user.Username, user.Email, user.FullName,
            user.Roles.ToList(), user.IsActive, user.LastLoginAt);
    }
}
