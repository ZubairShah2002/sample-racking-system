using SampleRack.SharedKernel.Abstractions;
using MediatR;

namespace SampleRack.Application.Abstractions.Messaging;

/// <summary>
/// Handler for domain event notifications. Bridges domain events from the SharedKernel
/// to MediatR's notification pipeline via <see cref="DomainEventNotification{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">The domain event type being handled.</typeparam>
public interface IDomainEventHandler<TEvent> : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent;
