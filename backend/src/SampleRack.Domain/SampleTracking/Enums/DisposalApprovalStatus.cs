namespace SampleRack.Domain.SampleTracking.Enums;

/// <summary>
/// Approval state of a disposal request. Disposal requires explicit approval
/// from an authorized user before the sample is marked Disposed.
/// </summary>
public enum DisposalApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
