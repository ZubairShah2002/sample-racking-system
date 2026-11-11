using SampleRack.Application.Abstractions;
using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking.Queries.ListSamples;

/// <summary>
/// The global search / inventory query. Supports free-text search across all
/// identifying fields plus structured filters, server-side sorting and
/// pagination. Aging-category filtering is applied against the always-current
/// computed aging.
/// </summary>
public sealed record ListSamplesQuery(
    string? Search,
    string? Status,
    string? AgingCategory,
    string? SupplierName,
    string? ProductCategory,
    string? Department,
    string? LocationCode,
    DateOnly? ReceivedFrom,
    DateOnly? ReceivedTo,
    bool IncludeDisposed,
    string? SortBy,
    string? SortDir,
    int Page,
    int PageSize) : IQuery<PagedResult<SampleSummaryDto>>;

internal sealed class ListSamplesQueryHandler : IQueryHandler<ListSamplesQuery, PagedResult<SampleSummaryDto>>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public ListSamplesQueryHandler(
        ISampleRepository samples,
        IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings,
        IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _clock = clock;
    }

    public async Task<Result<PagedResult<SampleSummaryDto>>> Handle(ListSamplesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);

        var locations = await _locations.ListOrderedAsync(includeInactive: true, cancellationToken);
        var codeById = locations.ToDictionary(l => l.Id, l => l.Code);
        Guid? locationId = null;
        if (!string.IsNullOrWhiteSpace(request.LocationCode))
        {
            var match = locations.FirstOrDefault(l =>
                l.Code.Equals(request.LocationCode.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match is null)
                return new PagedResult<SampleSummaryDto>(Array.Empty<SampleSummaryDto>(), 0, page, pageSize);
            locationId = match.Id;
        }

        AgingCategory? agingFilter = null;
        if (!string.IsNullOrWhiteSpace(request.AgingCategory)
            && Enum.TryParse<AgingCategory>(request.AgingCategory, ignoreCase: true, out var parsed))
        {
            agingFilter = parsed;
        }

        var spec = new SampleSearchSpecification(request, locationId, agingFilter is null, page, pageSize);

        if (agingFilter is null)
        {
            var items = await _samples.ListAsync(spec, cancellationToken);
            var total = await _samples.CountAsync(new SampleSearchSpecification(request, locationId, applyPaging: false, page, pageSize), cancellationToken);
            var dtos = items.Select(s => SampleTrackingMapper.ToSummaryDto(
                s, today, thresholds, s.RackLocationId.HasValue && codeById.TryGetValue(s.RackLocationId.Value, out var c) ? c : null)).ToList();
            return new PagedResult<SampleSummaryDto>(dtos, total, page, pageSize);
        }

        // Aging-category filter: evaluate against computed aging, then page in memory.
        var filtered = await _samples.ListAsync(
            new SampleSearchSpecification(request, locationId, applyPaging: false, page, pageSize), cancellationToken);

        var matched = filtered
            .Where(s => AgingCalculator.Classify(Math.Max(0, today.DayNumber - s.ReceivedDate.DayNumber), thresholds) == agingFilter)
            .ToList();

        var paged = matched.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => SampleTrackingMapper.ToSummaryDto(
                s, today, thresholds, s.RackLocationId.HasValue && codeById.TryGetValue(s.RackLocationId.Value, out var c) ? c : null))
            .ToList();

        return new PagedResult<SampleSummaryDto>(paged, matched.Count, page, pageSize);
    }
}

internal sealed class SampleSearchSpecification : Specification<Sample>
{
    public SampleSearchSpecification(ListSamplesQuery q, Guid? locationId, bool applyPaging, int page, int pageSize)
    {
        var term = q.Search?.Trim().ToUpperInvariant();
        var hasTerm = !string.IsNullOrEmpty(term);
        var status = q.Status?.Trim();
        var hasStatus = !string.IsNullOrEmpty(status);
        var supplier = q.SupplierName?.Trim().ToUpperInvariant();
        var hasSupplier = !string.IsNullOrEmpty(supplier);
        var category = q.ProductCategory?.Trim();
        var hasCategory = !string.IsNullOrEmpty(category);
        var department = q.Department?.Trim();
        var hasDepartment = !string.IsNullOrEmpty(department);
        var from = q.ReceivedFrom;
        var to = q.ReceivedTo;
        var disposedStatus = SampleStatus.Disposed.ToString();

        ApplyCriteria(s =>
            (q.IncludeDisposed || s.Status != disposedStatus) &&
            (!locationId.HasValue || s.RackLocationId == locationId.Value) &&
            (!hasStatus || s.Status == status) &&
            (!hasSupplier || (s.SupplierName != null && s.SupplierName.ToUpper().Contains(supplier!))) &&
            (!hasCategory || s.ProductCategory == category) &&
            (!hasDepartment || s.Department == department) &&
            (!from.HasValue || s.ReceivedDate >= from.Value) &&
            (!to.HasValue || s.ReceivedDate <= to.Value) &&
            (!hasTerm ||
                s.SampleCode.ToUpper().Contains(term!) ||
                s.SampleName.ToUpper().Contains(term!) ||
                (s.ItemCode != null && s.ItemCode.ToUpper().Contains(term!)) ||
                (s.Model != null && s.Model.ToUpper().Contains(term!)) ||
                (s.Size != null && s.Size.ToUpper().Contains(term!)) ||
                (s.Variant != null && s.Variant.ToUpper().Contains(term!)) ||
                (s.SupplierName != null && s.SupplierName.ToUpper().Contains(term!)) ||
                (s.SupplierCode != null && s.SupplierCode.ToUpper().Contains(term!)) ||
                (s.PurchaseOrderNumber != null && s.PurchaseOrderNumber.ToUpper().Contains(term!)) ||
                (s.GrnNumber != null && s.GrnNumber.ToUpper().Contains(term!))));

        ApplySort(q.SortBy, q.SortDir);

        if (applyPaging)
            ApplyPaging((page - 1) * pageSize, pageSize);
    }

    private void ApplySort(string? sortBy, string? sortDir)
    {
        var desc = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        switch ((sortBy ?? "received").ToLowerInvariant())
        {
            case "code":
                if (desc) ApplyOrderByDescending(s => s.SampleCode); else ApplyOrderBy(s => s.SampleCode);
                break;
            case "name":
                if (desc) ApplyOrderByDescending(s => s.SampleName); else ApplyOrderBy(s => s.SampleName);
                break;
            case "status":
                if (desc) ApplyOrderByDescending(s => s.Status); else ApplyOrderBy(s => s.Status);
                break;
            default: // received
                if (desc) ApplyOrderByDescending(s => s.ReceivedDate); else ApplyOrderBy(s => s.ReceivedDate);
                break;
        }
    }
}
