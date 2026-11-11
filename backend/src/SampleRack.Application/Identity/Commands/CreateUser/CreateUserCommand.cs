using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.Identity.Entities;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.Identity.Commands.CreateUser;

/// <summary>Creates a new application user with an initial password and role set.</summary>
public sealed record CreateUserCommand(
    string Username,
    string Email,
    string FullName,
    string Password,
    IReadOnlyList<string> Roles) : ICommand<Guid>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FullName).MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        RuleFor(x => x.Roles).NotEmpty();
    }
}

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;

    public CreateUserCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, ITenantContext tenantContext)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (await _users.UsernameExistsAsync(username, null, cancellationToken))
            return IdentityErrors.UsernameExists;

        if (request.Roles is null || request.Roles.Count == 0)
            return IdentityErrors.NoRoles;

        var user = User.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, username, request.Email,
            request.FullName, _passwordHasher.Hash(request.Password), request.Roles);

        await _users.AddAsync(user, cancellationToken);
        return user.Id;
    }
}
