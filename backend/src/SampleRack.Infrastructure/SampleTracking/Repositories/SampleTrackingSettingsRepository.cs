using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.SampleTracking.Repositories;

public sealed class SampleTrackingSettingsRepository : RepositoryBase<SampleTrackingSettings>, ISampleTrackingSettingsRepository
{
    public SampleTrackingSettingsRepository(SampleRackDbContext context) : base(context) { }

    public async Task<SampleTrackingSettings?> GetForTenantAsync(CancellationToken cancellationToken = default)
    {
        // The global tenant query filter scopes this to the current tenant;
        // at most one settings row exists per tenant.
        return await DbSet.FirstOrDefaultAsync(cancellationToken);
    }
}
