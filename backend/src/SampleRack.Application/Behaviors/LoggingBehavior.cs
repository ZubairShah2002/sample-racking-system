using System.Diagnostics;
using SampleRack.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SampleRack.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs command and query execution details including
/// the request name, authenticated user, tenant context, and elapsed time.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.IsAuthenticated ? currentUserService.UserId.ToString() : "Anonymous";
        var tenantId = currentUserService.IsAuthenticated ? currentUserService.TenantId.ToString() : "N/A";

        logger.LogInformation(
            "Handling {RequestName} | User: {UserId} | Tenant: {TenantId}",
            requestName,
            userId,
            tenantId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms | User: {UserId} | Tenant: {TenantId}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId,
                tenantId);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms | User: {UserId} | Tenant: {TenantId}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId,
                tenantId);

            throw;
        }
    }
}
