using SampleRack.Application.Interfaces;
using SampleRack.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SampleRack.Application.Behaviors;

/// <summary>
/// Pipeline behavior that reads the tenant identifier from the current user context
/// and sets it on the <see cref="ITenantContext"/> so that downstream repositories
/// and services operate within the correct tenant boundary.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class TenantContextBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ITenantContext tenantContext,
    ILogger<TenantContextBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            logger.LogDebug(
                "Skipping tenant context for {RequestName} - user is not authenticated",
                typeof(TRequest).Name);

            return await next();
        }

        var tenantId = TenantId.From(currentUserService.TenantId);
        tenantContext.SetTenant(tenantId);

        logger.LogDebug(
            "Tenant context set to {TenantId} for {RequestName}",
            tenantId,
            typeof(TRequest).Name);

        return await next();
    }
}
