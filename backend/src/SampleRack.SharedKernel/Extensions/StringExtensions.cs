using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SampleRack.SharedKernel.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Converts a string to a URL-friendly slug.
    /// Lowercases, replaces spaces and non-alphanumeric characters with hyphens,
    /// and collapses consecutive hyphens.
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Normalize to decomposed form, strip diacritics
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC);

        // Lowercase
        result = result.ToLowerInvariant();

        // Replace non-alphanumeric characters with hyphens
        result = NonAlphanumericRegex().Replace(result, "-");

        // Collapse consecutive hyphens
        result = ConsecutiveHyphensRegex().Replace(result, "-");

        // Trim leading/trailing hyphens
        return result.Trim('-');
    }

    /// <summary>
    /// Truncates the string to the specified maximum length, appending a suffix if truncated.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must not be negative.");

        if (value.Length <= maxLength)
            return value;

        if (maxLength <= suffix.Length)
            return value[..maxLength];

        return string.Concat(value.AsSpan(0, maxLength - suffix.Length), suffix);
    }

    /// <summary>
    /// Returns true if the string is null, empty, or consists only of whitespace.
    /// An extension-method alias for <see cref="string.IsNullOrWhiteSpace"/>.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Returns true if the string contains a non-whitespace value.
    /// </summary>
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Returns the string, or the specified default if it is null or whitespace.
    /// </summary>
    public static string OrDefault(this string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
    private static partial Regex ConsecutiveHyphensRegex();
}
