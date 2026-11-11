namespace SampleRack.Application.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's identity, tenant,
/// and role information. Implemented in the Infrastructure layer and populated
/// from the HTTP context or ambient auth context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The unique identifier of the authenticated user.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// The tenant identifier the user belongs to.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// The email address of the authenticated user.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// The roles assigned to the authenticated user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Whether the current request has an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks whether the current user is assigned the specified role.
    /// </summary>
    /// <param name="role">The role name to check.</param>
    /// <returns><c>true</c> if the user has the role; otherwise <c>false</c>.</returns>
    bool IsInRole(string role);
}
