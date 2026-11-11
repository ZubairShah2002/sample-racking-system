namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a user. Wraps a <see cref="Guid"/>.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(value));

        return new UserId(value);
    }

    public static UserId Parse(string input) => From(Guid.Parse(input));

    public override string ToString() => Value.ToString();
}
