using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Repositories;

public interface IMasterDataRepository : IRepository<MasterDataItem>
{
    Task<IReadOnlyList<MasterDataItem>> ListAsync(string? category, bool includeInactive, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string category, string name, Guid? excludeId, CancellationToken cancellationToken = default);
}
