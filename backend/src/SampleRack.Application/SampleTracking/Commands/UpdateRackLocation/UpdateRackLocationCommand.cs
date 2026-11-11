using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.UpdateRackLocation;

public sealed record UpdateRackLocationCommand(
    Guid Id,
    string? Description,
    string? Zone,
    int? Level,
    int? Position,
    int? Capacity) : ICommand;

public sealed class UpdateRackLocationCommandValidator : AbstractValidator<UpdateRackLocationCommand>
{
    public UpdateRackLocationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Zone).MaximumLength(50);
        RuleFor(x => x.Level).GreaterThan(0).When(x => x.Level.HasValue);
        RuleFor(x => x.Position).GreaterThan(0).When(x => x.Position.HasValue);
    }
}

internal sealed class UpdateRackLocationCommandHandler : ICommandHandler<UpdateRackLocationCommand>
{
    private readonly IRackLocationRepository _repository;

    public UpdateRackLocationCommandHandler(IRackLocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateRackLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (location is null)
            return Result.Failure(SampleTrackingErrors.LocationNotFound);

        var zone = string.IsNullOrWhiteSpace(request.Zone) ? location.Zone : request.Zone!.Trim();
        var level = request.Level ?? location.Level;
        var position = request.Position ?? location.Position;

        location.UpdateDetails(zone, level, position, request.Description, request.Capacity);
        await _repository.UpdateAsync(location, cancellationToken);
        return Result.Success();
    }
}
