using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.SampleTracking.Repositories;

public sealed class SampleRepository : RepositoryBase<Sample>, ISampleRepository
{
    private static readonly string DisposedStatus = SampleStatus.Disposed.ToString();

    public SampleRepository(SampleRackDbContext context) : base(context) { }

    public async Task<Sample?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.StatusHistory)
            .Include(s => s.Movements)
            .Include(s => s.TestingRecords)
            .Include(s => s.Attachments)
            .Include(s => s.Disposal)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sample?> GetByCodeAsync(string sampleCode, CancellationToken cancellationToken = default)
    {
        var normalized = sampleCode.Trim().ToUpperInvariant();
        return await DbSet.FirstOrDefaultAsync(s => s.SampleCode == normalized, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string sampleCode, CancellationToken cancellationToken = default)
    {
        var normalized = sampleCode.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(s => s.SampleCode == normalized, cancellationToken);
    }

    public async Task<int> GetMaxSequenceForYearAsync(string prefix, int year, CancellationToken cancellationToken = default)
    {
        var start = $"{prefix.ToUpperInvariant()}-{year}-";
        var codes = await DbSet
            .Where(s => s.SampleCode.StartsWith(start))
            .Select(s => s.SampleCode)
            .ToListAsync(cancellationToken);

        var max = 0;
        foreach (var code in codes)
        {
            var lastDash = code.LastIndexOf('-');
            if (lastDash < 0 || lastDash == code.Length - 1) continue;
            if (int.TryParse(code[(lastDash + 1)..], out var seq) && seq > max)
                max = seq;
        }
        return max;
    }

    public async Task<int> CountStoredInLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(
            s => s.RackLocationId == locationId && s.Status != DisposedStatus, cancellationToken);
    }

    public async Task<IReadOnlyList<Sample>> ListStoredInLocationAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.RackLocationId == locationId && s.Status != DisposedStatus)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sample>> ListStoredAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.RackLocationId != null && s.Status != DisposedStatus)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sample>> ListAllForAggregationAsync(bool includeDisposed, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (!includeDisposed)
            query = query.Where(s => s.Status != DisposedStatus);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyCount>> GetMonthlyMovementCountsAsync(DateOnly from, CancellationToken cancellationToken = default)
    {
        var fromUtc = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, DateTimeKind.Utc);
        return await Context.Set<SampleMovement>()
            .Where(m => m.MovedAt >= fromUtc)
            .GroupBy(m => new { m.MovedAt.Year, m.MovedAt.Month })
            .Select(g => new MonthlyCount(g.Key.Year, g.Key.Month, g.Count()))
            .ToListAsync(cancellationToken);
    }
}
