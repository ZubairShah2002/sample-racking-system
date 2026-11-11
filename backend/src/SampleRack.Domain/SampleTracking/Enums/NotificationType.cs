namespace SampleRack.Domain.SampleTracking.Enums;

/// <summary>
/// Category of a notification raised by the sample tracking system.
/// </summary>
public enum NotificationType
{
    SampleRegistered,
    ApproachingLimit,
    Overdue,
    Overdue120,
    Overdue180,
    AwaitingTesting,
    TestingFailed,
    WaitingForSupplier,
    SampleMoved,
    DisposalPendingApproval
}
