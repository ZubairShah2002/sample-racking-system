using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.ListMasterData;

/// <summary>Lists reference-data items, optionally filtered by category. Inactive
/// items are excluded unless <paramref name="IncludeInactive"/> is set (admin view).</summary>
public sealed record ListMasterDataQuery(string? Category, bool IncludeInactive)
    : IQuery<IReadOnlyList<MasterDataItemDto>>;

internal sealed class ListMasterDataQueryHandler
    : IQueryHandler<ListMasterDataQuery, IReadOnlyList<MasterDataItemDto>>
{
    private readonly IMasterDataRepository _repository;

    public ListMasterDataQueryHandler(IMasterDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<MasterDataItemDto>>> Handle(
        ListMasterDataQuery request, CancellationToken cancellationToken)
    {
        var category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        var items = await _repository.ListAsync(category, request.IncludeInactive, cancellationToken);

        var result = items
            .Select(m => new MasterDataItemDto(
                m.Id, m.Category, m.Name, m.Code, m.Description, m.SortOrder, m.IsActive))
            .ToList();

        return Result.Success<IReadOnlyList<MasterDataItemDto>>(result);
    }
}
