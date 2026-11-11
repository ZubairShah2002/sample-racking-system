using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.Domain.SampleTracking.Repositories;

public interface ISampleRepository : IRepository<Sample>
{
    /// <summary>Loads a sample with all child collections (history, movements, tests, attachments, disposal).</summary>
    Task<Sample?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Sample?> GetByCodeAsync(string sampleCode, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string sampleCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Highest sequence number already issued for the given prefix and year, used
    /// to generate the next unique sample code (e.g. SMP-2026-0007).
    /// </summary>
    Task<int> GetMaxSequenceForYearAsync(string prefix, int year, CancellationToken cancellationToken = default);

    /// <summary>Number of samples currently stored (not disposed) in a location.</summary>
    Task<int> CountStoredInLocationAsync(Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>All samples currently stored in a location (not disposed).</summary>
    Task<IReadOnlyList<Sample>> ListStoredInLocationAsync(Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>All samples that currently occupy any rack location (not disposed), for occupancy/aging roll-ups.</summary>
    Task<IReadOnlyList<Sample>> ListStoredAsync(CancellationToken cancellationToken = default);

    /// <summary>All samples (optionally excluding disposed) for dashboard/report aggregation.</summary>
    Task<IReadOnlyList<Sample>> ListAllForAggregationAsync(bool includeDisposed, CancellationToken cancellationToken = default);

    /// <summary>Movement counts grouped by calendar month from the given date, for dashboard trends.</summary>
    Task<IReadOnlyList<MonthlyCount>> GetMonthlyMovementCountsAsync(DateOnly from, CancellationToken cancellationToken = default);
}

/// <summary>A per-month aggregate count.</summary>
public sealed record MonthlyCount(int Year, int Month, int Count);
