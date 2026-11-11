namespace SampleRack.Domain.SampleTracking.Enums;

/// <summary>
/// Operational status of a stored sample. A sample has exactly one current
/// status at any time; every transition is recorded in status history.
/// Testing status and storage aging are tracked independently of this value.
/// </summary>
public enum SampleStatus
{
    Received,
    PendingTesting,
    TestingInProgress,
    TestedPass,
    TestedFail,
    OnHold,
    WaitingForSupplier,
    ReferenceSample,
    ToBeDisposed,
    Disposed
}
