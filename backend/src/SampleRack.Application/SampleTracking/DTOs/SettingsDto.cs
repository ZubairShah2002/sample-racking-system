namespace SampleRack.Application.SampleTracking.DTOs;

public sealed record SettingsDto(
    string? CompanyName,
    string? DepartmentName,
    string SampleIdPrefix,
    int AgingNewMaxDays,
    int AgingNormalMaxDays,
    int AgingOverdueDays,
    string DateFormat,
    string TimeZone,
    bool NotificationsEnabled);
