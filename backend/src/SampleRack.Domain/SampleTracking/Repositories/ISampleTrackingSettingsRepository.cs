using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Repositories;

public interface ISampleTrackingSettingsRepository : IRepository<SampleTrackingSettings>
{
    /// <summary>Returns the settings row for the current tenant, or null if not yet created.</summary>
    Task<SampleTrackingSettings?> GetForTenantAsync(CancellationToken cancellationToken = default);
}
