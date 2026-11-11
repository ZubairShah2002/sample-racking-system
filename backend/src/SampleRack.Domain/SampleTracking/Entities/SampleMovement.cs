using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// An immutable record of a sample moving between rack locations. Movement
/// timestamps are tracked separately from the sample's original received date,
/// which is never reset by a move.
/// </summary>
public sealed class SampleMovement : Entity<Guid>
{
    public Guid SampleId { get; private set; }
    public Guid? FromLocationId { get; private set; }
    public string? FromLocationCode { get; private set; }
    public Guid ToLocationId { get; private set; }
    public string ToLocationCode { get; private set; } = string.Empty;
    public string MovedBy { get; private set; } = string.Empty;
    public DateTime MovedAt { get; private set; }
    public string? Reason { get; private set; }
    public string? Remarks { get; private set; }

    private SampleMovement() { }

    internal static SampleMovement Create(
        Guid id,
        TenantId tenantId,
        Guid sampleId,
        Guid? fromLocationId,
        string? fromLocationCode,
        Guid toLocationId,
        string toLocationCode,
        string movedBy,
        DateTime movedAt,
        string? reason,
        string? remarks)
    {
        return new SampleMovement
        {
            Id = id,
            TenantId = tenantId,
            SampleId = sampleId,
            FromLocationId = fromLocationId,
            FromLocationCode = fromLocationCode,
            ToLocationId = toLocationId,
            ToLocationCode = toLocationCode,
            MovedBy = string.IsNullOrWhiteSpace(movedBy) ? "system" : movedBy.Trim(),
            MovedAt = movedAt,
            Reason = reason?.Trim(),
            Remarks = remarks?.Trim()
        };
    }
}
