using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Events;

public sealed record SampleMovedEvent(
    Guid SampleId,
    string SampleCode,
    string? FromLocationCode,
    string ToLocationCode,
    Guid TenantId) : DomainEvent;
