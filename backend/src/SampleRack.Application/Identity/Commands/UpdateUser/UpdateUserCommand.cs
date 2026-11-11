using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.Identity.Commands.UpdateUser;

/// <summary>Updates a user's profile and role assignments (admin operation).</summary>
public sealed record UpdateUserCommand(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles) : ICommand;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Roles).NotEmpty();
    }
}

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _users;

    public UpdateUserCommandHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(IdentityErrors.UserNotFound);

        if (request.Roles is null || request.Roles.Count == 0)
            return Result.Failure(IdentityErrors.NoRoles);

        user.UpdateProfile(request.FullName, request.Email);
        user.SetRoles(request.Roles);
        await _users.UpdateAsync(user, cancellationToken);
        return Result.Success();
    }
}
