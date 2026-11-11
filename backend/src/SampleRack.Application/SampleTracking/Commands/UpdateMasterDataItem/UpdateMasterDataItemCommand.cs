using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.UpdateMasterDataItem;

/// <summary>Renames or re-describes an existing reference-data item. The category
/// is immutable once created.</summary>
public sealed record UpdateMasterDataItemCommand(
    Guid Id,
    string Name,
    string? Code,
    string? Description,
    int? SortOrder) : ICommand;

public sealed class UpdateMasterDataItemCommandValidator : AbstractValidator<UpdateMasterDataItemCommand>
{
    public UpdateMasterDataItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

internal sealed class UpdateMasterDataItemCommandHandler : ICommandHandler<UpdateMasterDataItemCommand>
{
    private readonly IMasterDataRepository _repository;

    public UpdateMasterDataItemCommandHandler(IMasterDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateMasterDataItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
            return Result.Failure(SampleTrackingErrors.MasterDataNotFound);

        var name = request.Name.Trim();
        if (await _repository.NameExistsAsync(item.Category, name, item.Id, cancellationToken))
            return Result.Failure(SampleTrackingErrors.MasterDataNameExists);

        item.Update(name, request.Code, request.Description, request.SortOrder);
        await _repository.UpdateAsync(item, cancellationToken);
        return Result.Success();
    }
}
