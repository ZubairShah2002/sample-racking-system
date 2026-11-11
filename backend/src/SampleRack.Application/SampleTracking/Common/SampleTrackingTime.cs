using SampleRack.Application.Interfaces;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.ValueObjects;

namespace SampleRack.Application.SampleTracking.Common;

/// <summary>
/// Resolves the "current date" used for aging and the active aging thresholds,
/// honouring the configured time zone (default Asia/Kuala_Lumpur). Aging is a
/// pure function of the received date and this date, so it is always current
/// without any scheduled job.
/// </summary>
public static class SampleTrackingTime
{
    public const string DefaultTimeZone = "Asia/Kuala_Lumpur";

    public static DateOnly Today(SampleTrackingSettings? settings, IClockService clock)
    {
        var tz = string.IsNullOrWhiteSpace(settings?.TimeZone) ? DefaultTimeZone : settings!.TimeZone;
        var utcNow = DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc);
        try
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById(tz);
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, info));
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return DateOnly.FromDateTime(utcNow);
        }
    }

    public static AgingThresholds Thresholds(SampleTrackingSettings? settings)
        => settings?.GetThresholds() ?? AgingThresholds.Default;
}
