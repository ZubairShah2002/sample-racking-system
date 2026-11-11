using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// An immutable record of a single operational status transition for a sample.
/// History is append-only and the original record is never overwritten.
/// </summary>
public sealed class SampleStatusHistory : Entity<Guid>
{
    public Guid SampleId { get; private set; }
    public string? PreviousStatus { get; private set; }
    public string NewStatus { get; private set; } = string.Empty;
    public string ChangedBy { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }
    public string? Reason { get; private set; }
    public string? Remarks { get; private set; }

    private SampleStatusHistory() { }

    internal static SampleStatusHistory Create(
        Guid id,
        TenantId tenantId,
        Guid sampleId,
        string? previousStatus,
        string newStatus,
        string changedBy,
        DateTime changedAt,
        string? reason,
        string? remarks)
    {
        return new SampleStatusHistory
        {
            Id = id,
            TenantId = tenantId,
            SampleId = sampleId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "system" : changedBy.Trim(),
            ChangedAt = changedAt,
            Reason = reason?.Trim(),
            Remarks = remarks?.Trim()
        };
    }
}
