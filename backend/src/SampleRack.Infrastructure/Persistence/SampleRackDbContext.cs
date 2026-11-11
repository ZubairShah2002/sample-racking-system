using SampleRack.Application.Interfaces;
using SampleRack.Domain.Identity.Entities;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SampleRack.Infrastructure.Persistence;

/// <summary>
/// The Sample Racking database context. Owns all persistence, applies tenant
/// isolation via global query filters (defense-in-depth alongside PostgreSQL RLS),
/// and sets audit fields on save.
/// </summary>
public class SampleRackDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;
    private readonly IClockService _clockService;
    private readonly ICurrentUserService _currentUserService;
    private IDbContextTransaction? _currentTransaction;

    public SampleRackDbContext(
        DbContextOptions<SampleRackDbContext> options,
        ITenantContext tenantContext,
        IClockService clockService,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _tenantContext = tenantContext;
        _clockService = clockService;
        _currentUserService = currentUserService;
    }

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    // Sample tracking
    public DbSet<RackLocation> RackLocations => Set<RackLocation>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<SampleStatusHistory> SampleStatusHistory => Set<SampleStatusHistory>();
    public DbSet<SampleMovement> SampleMovements => Set<SampleMovement>();
    public DbSet<TestingRecord> TestingRecords => Set<TestingRecord>();
    public DbSet<SampleAttachment> SampleAttachments => Set<SampleAttachment>();
    public DbSet<DisposalRecord> DisposalRecords => Set<DisposalRecord>();
    public DbSet<Notification> SampleNotifications => Set<Notification>();
    public DbSet<SampleTrackingSettings> SampleTrackingSettings => Set<SampleTrackingSettings>();
    public DbSet<MasterDataItem> MasterDataItems => Set<MasterDataItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shared");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleRackDbContext).Assembly);

        // Global query filters for tenant isolation on every Entity<Guid>.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity<Guid>).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.ClrType == typeof(AuditLogEntry))
                continue;

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(Entity<Guid>.TenantId));
            var tenantIdValue = System.Linq.Expressions.Expression.Property(tenantIdProperty, "Value");
            var currentTenantExpr = System.Linq.Expressions.Expression.Property(
                System.Linq.Expressions.Expression.Constant(_tenantContext), nameof(ITenantContext.CurrentTenantId));
            var currentTenantValue = System.Linq.Expressions.Expression.Property(currentTenantExpr, "Value");
            var emptyGuid = System.Linq.Expressions.Expression.Constant(Guid.Empty);

            var equals = System.Linq.Expressions.Expression.Equal(tenantIdValue, currentTenantValue);
            var isEmpty = System.Linq.Expressions.Expression.Equal(currentTenantValue, emptyGuid);
            var combined = System.Linq.Expressions.Expression.OrElse(equals, isEmpty);

            var lambda = System.Linq.Expressions.Expression.Lambda(combined, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    private void SetAuditFields()
    {
        var now = _clockService.UtcNow;
        var userId = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : Guid.Empty.ToString();

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = userId;
                    break;
            }
        }
    }
}

/// <summary>Flat audit-trail row written by the <c>AuditTrailInterceptor</c>.</summary>
public class AuditLogEntry
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? SourceIp { get; set; }
    public string? CorrelationId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
}
