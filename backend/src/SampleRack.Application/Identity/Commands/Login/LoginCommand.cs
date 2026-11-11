using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Identity.DTOs;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.Identity.Commands.Login;

/// <summary>Authenticates a user by username/password and returns a signed JWT.</summary>
public sealed record LoginCommand(string Username, string Password) : ICommand<AuthResponseDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IClockService _clock;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IClockService clock)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _clock = clock;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return IdentityErrors.InvalidCredentials;

        if (!user.IsActive)
            return IdentityErrors.UserInactive;

        user.RecordLogin(_clock.UtcNow);
        await _users.UpdateAsync(user, cancellationToken);

        var (token, expiresAt) = _jwt.GenerateToken(user);

        var dto = new UserDto(
            user.Id, user.Username, user.Email, user.FullName,
            user.Roles.ToList(), user.IsActive, user.LastLoginAt);

        return new AuthResponseDto(token, expiresAt, dto);
    }
}
