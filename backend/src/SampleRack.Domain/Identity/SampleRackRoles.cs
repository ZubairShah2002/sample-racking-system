namespace SampleRack.Domain.Identity;

/// <summary>
/// The system roles seeded for every tenant. These names are the single source of
/// truth for role-based authorization and are mirrored by the API role groups.
/// </summary>
public static class SampleRackRoles
{
    public const string Administrator = "Administrator";
    public const string QcManager = "QC Manager";
    public const string QcTechnician = "QC Technician";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<(string Name, string Description)> All = new[]
    {
        (Administrator, "Full system access: user management, settings, master data, and disposal approval."),
        (QcManager, "Manages racks and samples, approves disposals, and oversees QC operations."),
        (QcTechnician, "Registers, tests, and moves samples; records day-to-day QC activity."),
        (Viewer, "Read-only access to dashboards, inventory, and reports.")
    };
}
