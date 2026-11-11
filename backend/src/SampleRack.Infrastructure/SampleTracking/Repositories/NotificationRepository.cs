using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.SampleTracking.Repositories;

public sealed class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
{
    public NotificationRepository(SampleRackDbContext context) : base(context) { }

    public async Task<bool> DedupeKeyExistsAsync(string dedupeKey, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(n => n.DedupeKey == dedupeKey, cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> ListRecentAsync(bool unreadOnly, int take, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountUnreadAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(n => !n.IsRead, cancellationToken);
    }
}
