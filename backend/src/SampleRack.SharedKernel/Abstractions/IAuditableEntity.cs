namespace SampleRack.SharedKernel.Abstractions;

/// <summary>
/// Marks an entity as auditable, tracking who created/modified it and when.
/// All timestamps must be UTC.
/// </summary>
public interface IAuditableEntity
{
    string CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    string? ModifiedBy { get; set; }
    DateTime? ModifiedAt { get; set; }
}
