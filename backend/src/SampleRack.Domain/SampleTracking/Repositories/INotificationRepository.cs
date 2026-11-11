using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<bool> DedupeKeyExistsAsync(string dedupeKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> ListRecentAsync(bool unreadOnly, int take, CancellationToken cancellationToken = default);
    Task<int> CountUnreadAsync(CancellationToken cancellationToken = default);
}
