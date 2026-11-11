using System.Text.RegularExpressions;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Represents a validated email address.
/// </summary>
public sealed partial class EmailAddress : ValueObject
{
    // RFC 5322-simplified pattern; not exhaustive, but covers practical cases.
    private static readonly Regex EmailPattern = EmailRegex();

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="EmailAddress"/> after format validation.
    /// </summary>
    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address is required.", nameof(email));

        var trimmed = email.Trim();

        if (trimmed.Length > 254)
            throw new ArgumentException("Email address cannot exceed 254 characters.", nameof(email));

        if (!EmailPattern.IsMatch(trimmed))
            throw new ArgumentException($"'{trimmed}' is not a valid email address.", nameof(email));

        return new EmailAddress(trimmed.ToLowerInvariant());
    }

    /// <summary>
    /// The local part of the email address (before the @).
    /// </summary>
    public string LocalPart => Value[..Value.IndexOf('@')];

    /// <summary>
    /// The domain part of the email address (after the @).
    /// </summary>
    public string Domain => Value[(Value.IndexOf('@') + 1)..];

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 250)]
    private static partial Regex EmailRegex();
}
