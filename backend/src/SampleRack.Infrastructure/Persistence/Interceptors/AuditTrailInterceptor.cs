using System.Text.Json;
using SampleRack.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SampleRack.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Writes a flat audit-trail row for every created, modified, or deleted entity,
/// capturing who, when, and the before/after values.
/// </summary>
public sealed class AuditTrailInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly IClockService _clockService;

    public AuditTrailInterceptor(
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        IClockService clockService)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _clockService = clockService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not SampleRackDbContext context)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = new List<AuditLogEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLogEntry)
                continue;

            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };
            if (action is null) continue;

            var oldValues = entry.State != EntityState.Added ? GetValues(entry.OriginalValues) : null;
            var newValues = entry.State != EntityState.Deleted ? GetValues(entry.CurrentValues) : null;

            auditEntries.Add(new AuditLogEntry
            {
                TenantId = _tenantContext.CurrentTenantId.Value,
                UserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : Guid.Empty,
                Timestamp = _clockService.UtcNow,
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = action,
                OldValue = oldValues is not null ? JsonSerializer.Serialize(oldValues) : null,
                NewValue = newValues is not null ? JsonSerializer.Serialize(newValues) : null,
                ModuleName = GetModuleName(entry.Entity.GetType())
            });
        }

        if (auditEntries.Count > 0)
            context.AuditLogs.AddRange(auditEntries);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static Guid GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue is Guid id ? id : Guid.Empty;
    }

    private static Dictionary<string, object?> GetValues(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues values)
    {
        var result = new Dictionary<string, object?>();
        foreach (var property in values.Properties)
        {
            var value = values[property];
            if (value is byte[]) continue;
            result[property.Name] = value;
        }
        return result;
    }

    private static string GetModuleName(Type entityType)
    {
        var ns = entityType.Namespace ?? string.Empty;
        var parts = ns.Split('.');
        return parts.Length >= 3 ? parts[2] : "Shared";
    }
}
