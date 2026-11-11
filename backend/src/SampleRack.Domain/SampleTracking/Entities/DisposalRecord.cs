using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// A disposal request for a sample. Disposal requires explicit approval from an
/// authorized user before the sample is marked Disposed. The record is retained
/// permanently for traceability — samples are never hard-deleted.
/// </summary>
public sealed class DisposalRecord : Entity<Guid>
{
    public Guid SampleId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTime RequestedAt { get; private set; }
    public string ApprovalStatus { get; private set; } = DisposalApprovalStatus.Pending.ToString();
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateOnly? DisposalDate { get; private set; }
    public string? Remarks { get; private set; }

    private DisposalRecord() { }

    internal static DisposalRecord CreateRequest(
        Guid id,
        TenantId tenantId,
        Guid sampleId,
        string reason,
        string requestedBy,
        DateTime requestedAt,
        string? remarks)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Disposal reason is required.");

        return new DisposalRecord
        {
            Id = id,
            TenantId = tenantId,
            SampleId = sampleId,
            Reason = reason.Trim(),
            RequestedBy = string.IsNullOrWhiteSpace(requestedBy) ? "system" : requestedBy.Trim(),
            RequestedAt = requestedAt,
            ApprovalStatus = DisposalApprovalStatus.Pending.ToString(),
            Remarks = remarks?.Trim()
        };
    }

    internal void Approve(string approvedBy, DateOnly disposalDate, DateTime approvedAt, string? remarks)
    {
        if (ApprovalStatus != DisposalApprovalStatus.Pending.ToString())
            throw new DomainException("Only a pending disposal request can be approved.");

        ApprovalStatus = DisposalApprovalStatus.Approved.ToString();
        ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "system" : approvedBy.Trim();
        ApprovedAt = approvedAt;
        DisposalDate = disposalDate;
        if (!string.IsNullOrWhiteSpace(remarks))
            Remarks = remarks.Trim();
    }

    internal void Reject(string approvedBy, DateTime approvedAt, string? remarks)
    {
        if (ApprovalStatus != DisposalApprovalStatus.Pending.ToString())
            throw new DomainException("Only a pending disposal request can be rejected.");

        ApprovalStatus = DisposalApprovalStatus.Rejected.ToString();
        ApprovedBy = string.IsNullOrWhiteSpace(approvedBy) ? "system" : approvedBy.Trim();
        ApprovedAt = approvedAt;
        if (!string.IsNullOrWhiteSpace(remarks))
            Remarks = remarks.Trim();
    }
}
