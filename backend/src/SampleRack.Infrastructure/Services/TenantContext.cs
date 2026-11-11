using SampleRack.Application.Interfaces;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Infrastructure.Services;

public sealed class TenantContext : ITenantContext
{
    public TenantId CurrentTenantId { get; private set; }

    public void SetTenant(TenantId tenantId) => CurrentTenantId = tenantId;
}
