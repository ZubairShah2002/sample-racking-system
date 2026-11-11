namespace SampleRack.Application.SampleTracking.DTOs;

/// <summary>A single administrator-managed reference value (department, product
/// category, material type or testing method).</summary>
public sealed record MasterDataItemDto(
    Guid Id,
    string Category,
    string Name,
    string? Code,
    string? Description,
    int SortOrder,
    bool IsActive);
