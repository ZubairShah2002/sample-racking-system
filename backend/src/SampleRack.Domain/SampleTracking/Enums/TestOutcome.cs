namespace SampleRack.Domain.SampleTracking.Enums;

/// <summary>
/// Result of a single QC testing record. A sample only becomes
/// TestedPass / TestedFail when an authorized user records an actual outcome.
/// </summary>
public enum TestOutcome
{
    Pass,
    Fail
}
