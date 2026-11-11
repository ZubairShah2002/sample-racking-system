namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a tenant. Wraps a <see cref="Guid"/>.
/// </summary>
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(value));

        return new TenantId(value);
    }

    public static TenantId Parse(string input) => From(Guid.Parse(input));

    public override string ToString() => Value.ToString();
}
