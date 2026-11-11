using SampleRack.SharedKernel.Results;

namespace SampleRack.Application.SampleTracking;

/// <summary>
/// Typed errors for the rack management and sample tracking module.
/// </summary>
public static class SampleTrackingErrors
{
    // Rack locations
    public static readonly Error LocationNotFound = Error.NotFound(
        "SampleTracking.LocationNotFound", "Rack location not found.");

    public static readonly Error LocationCodeExists = Error.Conflict(
        "SampleTracking.LocationCodeExists", "A rack location with this code already exists.");

    public static readonly Error LocationInactive = Error.Validation(
        "SampleTracking.LocationInactive", "The selected rack location is inactive and cannot store samples.");

    public static readonly Error LocationHasActiveSamples = Error.Conflict(
        "SampleTracking.LocationHasActiveSamples",
        "The location cannot be deactivated while active samples remain stored in it.");

    // Samples
    public static readonly Error SampleNotFound = Error.NotFound(
        "SampleTracking.SampleNotFound", "Sample not found.");

    public static readonly Error SampleCodeExists = Error.Conflict(
        "SampleTracking.SampleCodeExists", "A sample with this code already exists.");

    public static readonly Error InvalidStatus = Error.Validation(
        "SampleTracking.InvalidStatus", "The provided sample status is not recognized.");

    public static readonly Error InvalidTestOutcome = Error.Validation(
        "SampleTracking.InvalidTestOutcome", "The test outcome must be Pass or Fail.");

    public static readonly Error InvalidAttachmentCategory = Error.Validation(
        "SampleTracking.InvalidAttachmentCategory", "The attachment category is not recognized.");

    public static readonly Error NoPendingDisposal = Error.Validation(
        "SampleTracking.NoPendingDisposal", "There is no pending disposal request for this sample.");

    public static readonly Error SampleDisposed = Error.Validation(
        "SampleTracking.SampleDisposed", "The operation is not valid for a disposed sample.");

    // Notifications / settings
    public static readonly Error NotificationNotFound = Error.NotFound(
        "SampleTracking.NotificationNotFound", "Notification not found.");

    public static readonly Error SettingsNotFound = Error.NotFound(
        "SampleTracking.SettingsNotFound", "Sample tracking settings not found.");

    // Master data
    public static readonly Error MasterDataNotFound = Error.NotFound(
        "SampleTracking.MasterDataNotFound", "Reference data item not found.");

    public static readonly Error MasterDataCategoryInvalid = Error.Validation(
        "SampleTracking.MasterDataCategoryInvalid",
        "The reference-data category must be one of Department, ProductCategory, MaterialType or TestingMethod.");

    public static readonly Error MasterDataNameExists = Error.Conflict(
        "SampleTracking.MasterDataNameExists", "A reference-data item with this name already exists in this category.");
}
