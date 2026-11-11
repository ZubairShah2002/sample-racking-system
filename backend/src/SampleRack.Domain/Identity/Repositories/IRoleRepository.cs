using SampleRack.Domain.Identity.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.Identity.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
