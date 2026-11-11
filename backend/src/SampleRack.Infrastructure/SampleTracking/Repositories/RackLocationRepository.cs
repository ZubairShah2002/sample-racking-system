using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.SampleTracking.Repositories;

public sealed class RackLocationRepository : RepositoryBase<RackLocation>, IRackLocationRepository
{
    public RackLocationRepository(SampleRackDbContext context) : base(context) { }

    public async Task<RackLocation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.FirstOrDefaultAsync(l => l.Code == normalized, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(l => l.Code == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<RackLocation>> ListOrderedAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (!includeInactive)
            query = query.Where(l => l.IsActive);

        return await query
            .OrderBy(l => l.Zone)
            .ThenBy(l => l.Level)
            .ThenBy(l => l.Position)
            .ToListAsync(cancellationToken);
    }
}
