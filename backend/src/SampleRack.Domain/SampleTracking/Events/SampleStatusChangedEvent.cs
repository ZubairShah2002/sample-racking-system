using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Events;

public sealed record SampleStatusChangedEvent(
    Guid SampleId,
    string SampleCode,
    string? PreviousStatus,
    string NewStatus,
    Guid TenantId) : DomainEvent;
