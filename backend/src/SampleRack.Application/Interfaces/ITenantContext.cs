using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Application.Interfaces;

/// <summary>
/// Provides access to the current tenant identifier for the request scope.
/// Set by the <see cref="Behaviors.TenantContextBehavior{TRequest,TResponse}"/>
/// pipeline behavior and consumed by repositories and services to enforce
/// multi-tenant data isolation.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The tenant identifier for the current request scope.
    /// </summary>
    TenantId CurrentTenantId { get; }

    /// <summary>
    /// Sets the tenant identifier for the current request scope.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to set.</param>
    void SetTenant(TenantId tenantId);
}
