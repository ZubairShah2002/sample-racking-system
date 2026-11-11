namespace SampleRack.Application.Exceptions;

/// <summary>
/// Exception thrown when one or more FluentValidation failures are detected
/// in the <see cref="Behaviors.ValidationBehavior{TRequest,TResponse}"/> pipeline.
/// Carries a dictionary of field names to their respective error messages.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// A dictionary mapping field/property names to their validation error messages.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };
    }
}
