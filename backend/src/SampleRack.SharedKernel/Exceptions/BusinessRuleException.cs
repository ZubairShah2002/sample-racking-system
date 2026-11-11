namespace SampleRack.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a business rule is violated.
/// Carries a rule name for programmatic identification and a human-readable message.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    /// <summary>
    /// A machine-readable identifier for the violated business rule.
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Additional details about the violation, if any.
    /// </summary>
    public string? Details { get; }

    public BusinessRuleException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(string ruleName, string message, string details)
        : base(message)
    {
        RuleName = ruleName;
        Details = details;
    }

    public BusinessRuleException(string ruleName, string message, Exception innerException)
        : base(message, innerException)
    {
        RuleName = ruleName;
    }
}
