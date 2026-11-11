namespace SampleRack.SharedKernel.Abstractions;

/// <summary>
/// Base record implementing <see cref="IDomainEvent"/>.
/// Records provide structural equality and immutability by default.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
