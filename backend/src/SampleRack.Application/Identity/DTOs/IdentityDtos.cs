namespace SampleRack.Application.Identity.DTOs;

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime? LastLoginAtUtc);

public sealed record RoleDto(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem);

public sealed record AuthResponseDto(
    string Token,
    DateTime ExpiresAtUtc,
    UserDto User);
