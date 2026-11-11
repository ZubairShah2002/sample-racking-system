using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.Identity;

/// <summary>Typed errors for authentication and user management.</summary>
public static class IdentityErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Identity.InvalidCredentials", "The username or password is incorrect.");

    public static readonly Error UserInactive = Error.Unauthorized(
        "Identity.UserInactive", "This account has been deactivated. Contact an administrator.");

    public static readonly Error UserNotFound = Error.NotFound(
        "Identity.UserNotFound", "User not found.");

    public static readonly Error UsernameExists = Error.Conflict(
        "Identity.UsernameExists", "A user with this username already exists.");

    public static readonly Error CurrentPasswordIncorrect = Error.Validation(
        "Identity.CurrentPasswordIncorrect", "The current password is incorrect.");

    public static readonly Error NoRoles = Error.Validation(
        "Identity.NoRoles", "At least one role must be assigned to the user.");

    public static readonly Error CannotDeactivateSelf = Error.Validation(
        "Identity.CannotDeactivateSelf", "You cannot deactivate your own account.");
}
