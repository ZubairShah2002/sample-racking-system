using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.SetRackLocationActive;

/// <summary>
/// Activates or deactivates a rack location. Deactivation is blocked while active
/// (non-disposed) samples remain stored in the location.
/// </summary>
public sealed record SetRackLocationActiveCommand(Guid Id, bool IsActive) : ICommand;

public sealed class SetRackLocationActiveCommandValidator : AbstractValidator<SetRackLocationActiveCommand>
{
    public SetRackLocationActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

internal sealed class SetRackLocationActiveCommandHandler : ICommandHandler<SetRackLocationActiveCommand>
{
    private readonly IRackLocationRepository _locations;
    private readonly ISampleRepository _samples;

    public SetRackLocationActiveCommandHandler(IRackLocationRepository locations, ISampleRepository samples)
    {
        _locations = locations;
        _samples = samples;
    }

    public async Task<Result> Handle(SetRackLocationActiveCommand request, CancellationToken cancellationToken)
    {
        var location = await _locations.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
            return Result.Failure(SampleTrackingErrors.LocationNotFound);

        if (request.IsActive)
        {
            location.Activate();
        }
        else
        {
            var stored = await _samples.CountStoredInLocationAsync(location.Id, cancellationToken);
            if (stored > 0)
                return Result.Failure(SampleTrackingErrors.LocationHasActiveSamples);
            location.Deactivate();
        }

        await _locations.UpdateAsync(location, cancellationToken);
        return Result.Success();
    }
}
