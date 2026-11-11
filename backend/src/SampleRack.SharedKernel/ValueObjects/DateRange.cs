using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Represents a date range with UTC start and end dates.
/// The start date must be before or equal to the end date.
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateTime Start { get; }
    public DateTime End { get; }

    private DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates a new <see cref="DateRange"/> after validating that both dates are UTC
    /// and that the start precedes the end.
    /// </summary>
    public static DateRange Create(DateTime start, DateTime end)
    {
        if (start.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Start date must be in UTC.", nameof(start));

        if (end.Kind != DateTimeKind.Utc)
            throw new ArgumentException("End date must be in UTC.", nameof(end));

        if (start > end)
            throw new ArgumentException("Start date must be before or equal to end date.", nameof(start));

        return new DateRange(start, end);
    }

    /// <summary>
    /// Returns the duration of this date range.
    /// </summary>
    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Determines whether the specified date falls within this range (inclusive).
    /// </summary>
    public bool Contains(DateTime date) => date >= Start && date <= End;

    /// <summary>
    /// Determines whether this range overlaps with another range.
    /// </summary>
    public bool Overlaps(DateRange other) => Start < other.End && other.Start < End;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:O} - {End:O}";
}
