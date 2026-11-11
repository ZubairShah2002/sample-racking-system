using System.Data;
using SampleRack.Domain.Identity;
using SampleRack.Domain.Identity.Entities;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Application.Interfaces;
using SampleRack.Infrastructure.Persistence;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SampleRack.Api.Data;

/// <summary>
/// Applies the idempotent database schema script at startup and seeds the
/// default administrator account and module settings if they are missing.
/// </summary>
public sealed class DatabaseInitializer
{
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SampleRackDbContext>();

        await WaitForDatabaseAsync(db, cancellationToken);
        await RunSchemaScriptAsync(db, cancellationToken);
        await SeedAsync(scope.ServiceProvider, cancellationToken);
    }

    private async Task WaitForDatabaseAsync(SampleRackDbContext db, CancellationToken ct)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.OpenConnectionAsync(ct);
                await db.Database.CloseConnectionAsync();
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(2 * attempt, 10));
                _logger.LogWarning("Database not ready (attempt {Attempt}/{Max}): {Message}. Retrying in {Delay}s.",
                    attempt, maxAttempts, ex.Message, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task RunSchemaScriptAsync(SampleRackDbContext db, CancellationToken ct)
    {
        var scriptPath = ResolveScriptPath();
        if (scriptPath is null)
        {
            _logger.LogWarning("Schema script not found; skipping schema initialization. " +
                               "Set Database:InitScriptPath or place init-db.sql beside the app.");
            return;
        }

        _logger.LogInformation("Applying database schema from {Path}", scriptPath);
        var sql = await File.ReadAllTextAsync(scriptPath, ct);

        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        var opened = connection.State != ConnectionState.Open;
        if (opened) await connection.OpenAsync(ct);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 120;
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            if (opened) await connection.CloseAsync();
        }
        _logger.LogInformation("Database schema applied.");
    }

    private string? ResolveScriptPath()
    {
        var configured = _configuration["Database:InitScriptPath"];
        var candidates = new[]
        {
            configured,
            Path.Combine(AppContext.BaseDirectory, "init-db.sql"),
            Path.Combine(AppContext.BaseDirectory, "docker", "init-db.sql"),
        };
        return candidates.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p));
    }

    private async Task SeedAsync(IServiceProvider sp, CancellationToken ct)
    {
        var db = sp.GetRequiredService<SampleRackDbContext>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var tenantId = TenantId.From(DefaultTenantId);

        // Ensure the system roles exist (SQL seeds them too; this is a safety net).
        if (!await db.Roles.AnyAsync(ct))
        {
            foreach (var (name, description) in SampleRackRoles.All)
            {
                var role = Role.Create(Guid.NewGuid(), tenantId, name, description, isSystem: true);
                role.CreatedBy = "system";
                await db.Roles.AddAsync(role, ct);
            }
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded system roles.");
        }

        // Seed the default administrator if there are no users yet.
        if (!await db.Users.AnyAsync(ct))
        {
            var username = _configuration["Seed:AdminUsername"] ?? "admin";
            var password = _configuration["Seed:AdminPassword"] ?? "Admin@12345";
            var email = _configuration["Seed:AdminEmail"] ?? "admin@samplerack.local";

            var admin = User.Create(
                Guid.NewGuid(), tenantId, username, email, "System Administrator",
                hasher.Hash(password), new[] { SampleRackRoles.Administrator });
            admin.CreatedBy = "system";
            await db.Users.AddAsync(admin, ct);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded default administrator '{Username}'.", username);
        }

        // Seed default module settings if missing.
        if (!await db.SampleTrackingSettings.AnyAsync(ct))
        {
            var settings = SampleTrackingSettings.CreateDefault(Guid.NewGuid(), tenantId);
            await db.SampleTrackingSettings.AddAsync(settings, ct);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded default sample-tracking settings.");
        }
    }
}
