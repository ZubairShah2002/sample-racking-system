namespace SampleRack.Application.SampleTracking.Common;

/// <summary>
/// Parses the physical rack code into the module's axes. Codes follow the
/// Zone-Bay-Shelf convention seen on the real rack, e.g. "A1-02-30":
/// zone "A1", bay 2 (horizontal column), shelf level 30 → level 3 (bottom shelf
/// is 10, top is 40). The horizontal bay maps to <paramref name="position"/> and
/// the vertical shelf to <paramref name="level"/>, which is how the 3D view and
/// rack overview lay the grid out. Parsing is best-effort; callers may supply the
/// structure explicitly to override it.
/// </summary>
public static class RackCodeParser
{
    public static bool TryParse(string code, out string zone, out int level, out int position)
    {
        zone = string.Empty;
        level = 0;
        position = 0;

        if (string.IsNullOrWhiteSpace(code))
            return false;

        var parts = code.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 3)
            return false;

        var shelfPart = parts[^1];   // e.g. "30"  → shelf level 3
        var bayPart = parts[^2];     // e.g. "02"  → bay (column) 2
        var zonePart = string.Join('-', parts[..^2]);

        if (!int.TryParse(bayPart, out var bay) || !int.TryParse(shelfPart, out var shelfRaw))
            return false;

        // Shelf numbers are issued in tens on the physical rack (10/20/30/40);
        // collapse to a 1-based level. A plain 1-9 is accepted as-is.
        var shelf = (shelfRaw >= 10 && shelfRaw % 10 == 0) ? shelfRaw / 10 : shelfRaw;

        if (string.IsNullOrWhiteSpace(zonePart) || bay <= 0 || shelf <= 0)
            return false;

        zone = zonePart.ToUpperInvariant();
        level = shelf;      // vertical shelf
        position = bay;     // horizontal bay/column
        return true;
    }
}
