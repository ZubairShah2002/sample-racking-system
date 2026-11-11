namespace SampleRack.SharedKernel.Exceptions;

/// <summary>
/// Base exception for domain-level errors. All domain exceptions should derive from this.
/// </summary>
public class DomainException : Exception
{
    public DomainException() { }

    public DomainException(string message)
        : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
