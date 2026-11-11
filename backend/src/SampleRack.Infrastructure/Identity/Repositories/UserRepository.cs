using SampleRack.Domain.Identity.Entities;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.Identity.Repositories;

public sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(SampleRackDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim();
        return await DbSet.FirstOrDefaultAsync(
            u => u.Username.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        return await DbSet.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim();
        return await DbSet.AnyAsync(
            u => u.Username.ToLower() == normalized.ToLower() && (!excludeId.HasValue || u.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (!includeInactive)
            query = query.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(s) ||
                u.FullName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        return await query.OrderBy(u => u.Username).ToListAsync(cancellationToken);
    }
}
