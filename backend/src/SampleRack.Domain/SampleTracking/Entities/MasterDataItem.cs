using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// A reference/master-data value managed by administrators — a department,
/// product category, material type or testing method. Referenced by name from
/// samples and testing records; deactivating an item hides it from pickers
/// without affecting historical records that already use its name.
/// </summary>
public sealed class MasterDataItem : AggregateRoot<Guid>, IAuditableEntity
{
    public string Category { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private MasterDataItem() { }

    public static MasterDataItem Create(
        Guid id,
        TenantId tenantId,
        MasterDataCategory category,
        string name,
        string? code,
        string? description,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        return new MasterDataItem
        {
            Id = id,
            TenantId = tenantId,
            Category = category.ToString(),
            Name = name.Trim(),
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            Description = description?.Trim(),
            SortOrder = sortOrder,
            IsActive = true
        };
    }

    public void Update(string name, string? code, string? description, int? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        Name = name.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        Description = description?.Trim();
        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;
    }

    public void SetActive(bool active) => IsActive = active;
}
