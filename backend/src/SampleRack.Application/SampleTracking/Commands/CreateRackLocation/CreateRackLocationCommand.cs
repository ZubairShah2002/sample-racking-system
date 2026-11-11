using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.CreateRackLocation;

/// <summary>Creates a new rack location. Zone/level/position are derived from the
/// code when not supplied explicitly, supporting future rack expansion.</summary>
public sealed record CreateRackLocationCommand(
    string Code,
    string? Description,
    string? Zone,
    int? Level,
    int? Position,
    int? Capacity) : ICommand<Guid>;

public sealed class CreateRackLocationCommandValidator : AbstractValidator<CreateRackLocationCommand>
{
    public CreateRackLocationCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Zone).MaximumLength(50);
        RuleFor(x => x.Level).GreaterThan(0).When(x => x.Level.HasValue);
        RuleFor(x => x.Position).GreaterThan(0).When(x => x.Position.HasValue);
    }
}

internal sealed class CreateRackLocationCommandHandler : ICommandHandler<CreateRackLocationCommand, Guid>
{
    private readonly IRackLocationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateRackLocationCommandHandler(IRackLocationRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateRackLocationCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        if (await _repository.CodeExistsAsync(code, cancellationToken))
            return SampleTrackingErrors.LocationCodeExists;

        string zone;
        int level, position;

        if (!string.IsNullOrWhiteSpace(request.Zone) && request.Level.HasValue && request.Position.HasValue)
        {
            zone = request.Zone!.Trim();
            level = request.Level.Value;
            position = request.Position.Value;
        }
        else if (!RackCodeParser.TryParse(code, out zone, out level, out position))
        {
            return Error.Validation(
                "SampleTracking.InvalidLocationCode",
                "Could not derive zone/level/position from the code. Provide them explicitly or use the ZONE-LEVEL-POSITION format (e.g. A1-02-03).");
        }

        var location = RackLocation.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, code, zone, level, position, request.Description,
            request.Capacity ?? 4);

        await _repository.AddAsync(location, cancellationToken);
        return location.Id;
    }
}
