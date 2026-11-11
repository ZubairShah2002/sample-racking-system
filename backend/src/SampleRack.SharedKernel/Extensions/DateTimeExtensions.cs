namespace SampleRack.SharedKernel.Extensions;

/// <summary>
/// Extension methods for <see cref="DateTime"/>.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts the DateTime to UTC. If the kind is Unspecified, it is assumed to be UTC.
    /// </summary>
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) // Unspecified -> treat as UTC
        };
    }

    /// <summary>
    /// Returns true if the date falls on a Saturday or Sunday.
    /// </summary>
    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    /// <summary>
    /// Returns true if the date falls on a weekday (Monday through Friday).
    /// </summary>
    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }

    /// <summary>
    /// Returns the start of the day (00:00:00.000) for the given date, preserving the Kind.
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Returns the end of the day (23:59:59.9999999) for the given date, preserving the Kind.
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999, dateTime.Kind)
            .AddTicks(9999); // 23:59:59.9999999
    }

    /// <summary>
    /// Returns the start of the month for the given date.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Returns the end of the month for the given date.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        return new DateTime(dateTime.Year, dateTime.Month, daysInMonth, 23, 59, 59, 999, dateTime.Kind)
            .AddTicks(9999);
    }

    /// <summary>
    /// Returns the number of business days (weekdays) between two dates (inclusive of start, exclusive of end).
    /// </summary>
    public static int BusinessDaysUntil(this DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentException("'from' date must be before or equal to 'to' date.");

        var count = 0;
        var current = from.Date;
        var endDate = to.Date;

        while (current < endDate)
        {
            if (current.IsWeekday())
                count++;

            current = current.AddDays(1);
        }

        return count;
    }
}
