using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.Identity.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.Identity.Commands.SetUserActive;

/// <summary>Activates or deactivates a user account (admin operation).</summary>
public sealed record SetUserActiveCommand(Guid Id, bool Active) : ICommand;

internal sealed class SetUserActiveCommandHandler : ICommandHandler<SetUserActiveCommand>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;

    public SetUserActiveCommandHandler(IUserRepository users, ICurrentUserService currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (!request.Active && request.Id == _currentUser.UserId)
            return Result.Failure(IdentityErrors.CannotDeactivateSelf);

        var user = await _users.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(IdentityErrors.UserNotFound);

        user.SetActive(request.Active);
        await _users.UpdateAsync(user, cancellationToken);
        return Result.Success();
    }
}
