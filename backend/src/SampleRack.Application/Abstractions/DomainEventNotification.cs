using SampleRack.SharedKernel.Abstractions;
using MediatR;

namespace SampleRack.Application.Abstractions;

/// <summary>
/// Wraps an <see cref="IDomainEvent"/> from the SharedKernel as a MediatR
/// <see cref="INotification"/>, bridging the domain layer to the application
/// layer's event handling pipeline.
/// </summary>
/// <typeparam name="TEvent">The domain event type being wrapped.</typeparam>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
