namespace SampleRack.Application.Interfaces;

/// <summary>
/// Abstraction over the system clock to enable deterministic testing
/// and consistent timestamp generation across the application.
/// </summary>
public interface IClockService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
