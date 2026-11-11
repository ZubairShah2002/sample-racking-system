using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Commands.SetMasterDataItemActive;

/// <summary>Activates or deactivates a reference-data item. Deactivating hides it
/// from pickers without affecting historical records that reference its name.</summary>
public sealed record SetMasterDataItemActiveCommand(Guid Id, bool Active) : ICommand;

internal sealed class SetMasterDataItemActiveCommandHandler : ICommandHandler<SetMasterDataItemActiveCommand>
{
    private readonly IMasterDataRepository _repository;

    public SetMasterDataItemActiveCommandHandler(IMasterDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SetMasterDataItemActiveCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result.Failure(SampleTrackingErrors.MasterDataNotFound);

        item.SetActive(request.Active);
        await _repository.UpdateAsync(item, cancellationToken);
        return Result.Success();
    }
}
