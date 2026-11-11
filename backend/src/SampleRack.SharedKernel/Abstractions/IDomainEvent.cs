namespace SampleRack.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// The Application layer bridges this to MediatR.INotification.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
