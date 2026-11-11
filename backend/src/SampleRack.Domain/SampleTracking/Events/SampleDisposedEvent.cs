using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Events;

public sealed record SampleDisposedEvent(
    Guid SampleId,
    string SampleCode,
    Guid TenantId) : DomainEvent;
