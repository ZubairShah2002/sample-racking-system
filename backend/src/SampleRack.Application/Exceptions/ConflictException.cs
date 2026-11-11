namespace SampleRack.Application.Exceptions;

/// <summary>
/// Exception thrown when a concurrent modification conflict is detected,
/// typically from optimistic concurrency checks on aggregate roots.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException()
        : base("A conflict occurred due to a concurrent modification. Please reload and try again.")
    {
    }

    public ConflictException(string entityName, object entityId)
        : base($"A concurrent modification conflict was detected for {entityName} with identifier {entityId}.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
