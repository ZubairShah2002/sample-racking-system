using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Identity.DTOs;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.Identity.Queries.ListRoles;

/// <summary>Lists all assignable roles for the user-management UI.</summary>
public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleDto>>;

internal sealed class ListRolesQueryHandler : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IRoleRepository _roles;

    public ListRolesQueryHandler(IRoleRepository roles)
    {
        _roles = roles;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roles.ListAsync(cancellationToken);
        var result = roles
            .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.IsSystem))
            .ToList();
        return Result.Success<IReadOnlyList<RoleDto>>(result);
    }
}
