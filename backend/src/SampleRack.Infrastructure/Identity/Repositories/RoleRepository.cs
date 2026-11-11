using SampleRack.Domain.Identity.Entities;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.Identity.Repositories;

public sealed class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(SampleRackDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken = default)
        => await DbSet.OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return await DbSet.FirstOrDefaultAsync(r => r.Name.ToLower() == normalized.ToLower(), cancellationToken);
    }
}
