using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Events;

public sealed record SampleRegisteredEvent(
    Guid SampleId,
    string SampleCode,
    Guid RackLocationId,
    Guid TenantId) : DomainEvent;
