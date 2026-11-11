using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Immutable audit metadata recording who created/modified an entity and when.
/// All timestamps are UTC.
/// </summary>
public sealed class AuditMetadata : ValueObject
{
    public string CreatedBy { get; }
    public DateTime CreatedAt { get; }
    public string? ModifiedBy { get; }
    public DateTime? ModifiedAt { get; }

    private AuditMetadata(string createdBy, DateTime createdAt, string? modifiedBy, DateTime? modifiedAt)
    {
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        ModifiedBy = modifiedBy;
        ModifiedAt = modifiedAt;
    }

    /// <summary>
    /// Creates initial audit metadata for a new entity.
    /// </summary>
    public static AuditMetadata Create(string createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy is required.", nameof(createdBy));

        return new AuditMetadata(createdBy, DateTime.UtcNow, null, null);
    }

    /// <summary>
    /// Creates audit metadata with explicit timestamps (useful for deserialization).
    /// </summary>
    public static AuditMetadata Create(string createdBy, DateTime createdAt, string? modifiedBy, DateTime? modifiedAt)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy is required.", nameof(createdBy));

        if (createdAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("CreatedAt must be in UTC.", nameof(createdAt));

        if (modifiedAt.HasValue && modifiedAt.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("ModifiedAt must be in UTC.", nameof(modifiedAt));

        return new AuditMetadata(createdBy, createdAt, modifiedBy, modifiedAt);
    }

    /// <summary>
    /// Returns a new AuditMetadata reflecting a modification event.
    /// </summary>
    public AuditMetadata WithModification(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new ArgumentException("ModifiedBy is required.", nameof(modifiedBy));

        return new AuditMetadata(CreatedBy, CreatedAt, modifiedBy, DateTime.UtcNow);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CreatedBy;
        yield return CreatedAt;
        yield return ModifiedBy;
        yield return ModifiedAt;
    }

    public override string ToString() =>
        ModifiedAt.HasValue
            ? $"Created by {CreatedBy} at {CreatedAt:O}, modified by {ModifiedBy} at {ModifiedAt:O}"
            : $"Created by {CreatedBy} at {CreatedAt:O}";
}
