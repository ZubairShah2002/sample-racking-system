using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.CreateMasterDataItem;

/// <summary>Creates a reference-data item in one of the four managed categories.</summary>
public sealed record CreateMasterDataItemCommand(
    string Category,
    string Name,
    string? Code,
    string? Description,
    int? SortOrder) : ICommand<Guid>;

public sealed class CreateMasterDataItemCommandValidator : AbstractValidator<CreateMasterDataItemCommand>
{
    public CreateMasterDataItemCommandValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

internal sealed class CreateMasterDataItemCommandHandler : ICommandHandler<CreateMasterDataItemCommand, Guid>
{
    private readonly IMasterDataRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateMasterDataItemCommandHandler(IMasterDataRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateMasterDataItemCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MasterDataCategory>(request.Category?.Trim(), ignoreCase: true, out var category))
            return SampleTrackingErrors.MasterDataCategoryInvalid;

        var name = request.Name.Trim();
        if (await _repository.NameExistsAsync(category.ToString(), name, null, cancellationToken))
            return SampleTrackingErrors.MasterDataNameExists;

        var item = MasterDataItem.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, category, name,
            request.Code, request.Description, request.SortOrder ?? 0);

        await _repository.AddAsync(item, cancellationToken);
        return item.Id;
    }
}
