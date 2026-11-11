using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.SampleTracking.Repositories;

public sealed class MasterDataRepository : RepositoryBase<MasterDataItem>, IMasterDataRepository
{
    public MasterDataRepository(SampleRackDbContext context) : base(context) { }

    public async Task<IReadOnlyList<MasterDataItem>> ListAsync(string? category, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);
        if (!includeInactive)
            query = query.Where(m => m.IsActive);

        return await query
            .OrderBy(m => m.Category)
            .ThenBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string category, string name, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return await DbSet.AnyAsync(
            m => m.Category == category && m.Name == normalized && (!excludeId.HasValue || m.Id != excludeId.Value),
            cancellationToken);
    }
}
