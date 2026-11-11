using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Represents a person's name with first name, last name, and a computed display name.
/// </summary>
public sealed class PersonName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }

    /// <summary>
    /// The full display name, formatted as "FirstName LastName".
    /// </summary>
    public string DisplayName { get; }

    private PersonName(string firstName, string lastName, string displayName)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
    }

    /// <summary>
    /// Creates a new <see cref="PersonName"/> with the display name computed as "FirstName LastName".
    /// </summary>
    public static PersonName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        var trimmedFirst = firstName.Trim();
        var trimmedLast = lastName.Trim();

        if (trimmedFirst.Length > 100)
            throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));

        if (trimmedLast.Length > 100)
            throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));

        return new PersonName(trimmedFirst, trimmedLast, $"{trimmedFirst} {trimmedLast}");
    }

    /// <summary>
    /// Creates a new <see cref="PersonName"/> with an explicit display name.
    /// </summary>
    public static PersonName Create(string firstName, string lastName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        return new PersonName(firstName.Trim(), lastName.Trim(), displayName.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return DisplayName;
    }

    public override string ToString() => DisplayName;
}
