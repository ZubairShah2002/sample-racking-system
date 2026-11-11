namespace SampleRack.Application.Exceptions;

/// <summary>
/// Exception thrown when the current user does not have the required
/// permissions to perform the requested operation.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Access is denied. You do not have permission to perform this action.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
