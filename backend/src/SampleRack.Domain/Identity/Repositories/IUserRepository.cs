using SampleRack.Domain.Identity.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.Identity.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> ListAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default);
}
