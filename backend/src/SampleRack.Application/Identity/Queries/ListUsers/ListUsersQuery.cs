using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Identity.DTOs;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.Identity.Queries.ListUsers;

/// <summary>Lists users, optionally filtered by search text; inactive included for admin.</summary>
public sealed record ListUsersQuery(string? Search, bool IncludeInactive)
    : IQuery<IReadOnlyList<UserDto>>;

internal sealed class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _users;

    public ListUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.ListAsync(request.Search, request.IncludeInactive, cancellationToken);
        var result = users
            .Select(u => new UserDto(
                u.Id, u.Username, u.Email, u.FullName, u.Roles.ToList(), u.IsActive, u.LastLoginAt))
            .ToList();
        return Result.Success<IReadOnlyList<UserDto>>(result);
    }
}
