using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Repositories;

public interface IRackLocationRepository : IRepository<RackLocation>
{
    Task<RackLocation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RackLocation>> ListOrderedAsync(bool includeInactive, CancellationToken cancellationToken = default);
}
