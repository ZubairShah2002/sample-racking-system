using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.Identity.Entities;

/// <summary>
/// An application user with credentials and one or more role assignments.
/// Passwords are stored only as BCrypt hashes. Roles are stored as role names
/// (a PostgreSQL text[] column) and projected into JWT claims at sign-in.
/// </summary>
public sealed class User : AggregateRoot<Guid>, IAuditableEntity
{
    private List<string> _roles = new();

    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private User() { }

    public static User Create(
        Guid id,
        TenantId tenantId,
        string username,
        string email,
        string fullName,
        string passwordHash,
        IEnumerable<string> roles)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        var user = new User
        {
            Id = id,
            TenantId = tenantId,
            Username = username.Trim(),
            Email = email?.Trim() ?? string.Empty,
            FullName = string.IsNullOrWhiteSpace(fullName) ? username.Trim() : fullName.Trim(),
            PasswordHash = passwordHash,
            IsActive = true
        };
        user.SetRoles(roles);
        return user;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");
        PasswordHash = passwordHash;
    }

    public void UpdateProfile(string fullName, string email)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            FullName = fullName.Trim();
        Email = email?.Trim() ?? string.Empty;
    }

    public void SetRoles(IEnumerable<string> roles)
    {
        _roles.Clear();
        if (roles is null) return;
        foreach (var role in roles
                     .Where(r => !string.IsNullOrWhiteSpace(r))
                     .Select(r => r.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _roles.Add(role);
        }
    }

    public void SetActive(bool active) => IsActive = active;

    public void RecordLogin(DateTime at) => LastLoginAt = at;

    public bool IsInRole(string role) =>
        _roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
}
