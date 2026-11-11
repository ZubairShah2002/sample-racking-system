using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SampleRack.SharedKernel.Guards;

/// <summary>
/// Provides guard clause helpers for validating method arguments.
/// All methods throw immediately if the condition is violated.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Contains guard methods that throw when a condition is met (guard "against" bad input).
    /// </summary>
    public static class Against
    {
        /// <summary>
        /// Throws <see cref="ArgumentNullException"/> if <paramref name="value"/> is null.
        /// </summary>
        public static T Null<T>(
            [NotNull] T? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value is null)
                throw new ArgumentNullException(paramName, $"{paramName} must not be null.");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the string is null, empty, or whitespace.
        /// </summary>
        public static string NullOrWhiteSpace(
            [NotNull] string? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} must not be null, empty, or whitespace.", paramName);

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the string is null or empty.
        /// </summary>
        public static string NullOrEmpty(
            [NotNull] string? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{paramName} must not be null or empty.", paramName);

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the collection is null or has no elements.
        /// </summary>
        public static IReadOnlyCollection<T> NullOrEmpty<T>(
            [NotNull] IReadOnlyCollection<T>? value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value is null || value.Count == 0)
                throw new ArgumentException($"{paramName} must not be null or empty.", paramName);

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the GUID is <see cref="Guid.Empty"/>.
        /// </summary>
        public static Guid Empty(
            Guid value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value == Guid.Empty)
                throw new ArgumentException($"{paramName} must not be an empty GUID.", paramName);

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/>
        /// is outside the range [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        public static T OutOfRange<T>(
            T value,
            T min,
            T max,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    value,
                    $"{paramName} must be between {min} and {max} (inclusive).");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
        /// </summary>
        public static int Negative(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must not be negative.");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
        /// </summary>
        public static decimal Negative(
            decimal value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0m)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must not be negative.");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
        /// </summary>
        public static long Negative(
            long value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0L)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must not be negative.");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero or negative.
        /// </summary>
        public static int ZeroOrNegative(
            int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be greater than zero.");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero or negative.
        /// </summary>
        public static decimal ZeroOrNegative(
            decimal value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0m)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be greater than zero.");

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the string exceeds the specified maximum length.
        /// </summary>
        public static string LengthExceeding(
            string value,
            int maxLength,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            Guard.Against.Null(value, paramName);

            if (value.Length > maxLength)
                throw new ArgumentException(
                    $"{paramName} must not exceed {maxLength} characters. Actual length: {value.Length}.",
                    paramName);

            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the condition is true.
        /// </summary>
        public static void InvalidInput(
            bool condition,
            string paramName,
            string message)
        {
            if (condition)
                throw new ArgumentException(message, paramName);
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the DateTime is not UTC.
        /// </summary>
        public static DateTime NotUtc(
            DateTime value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException($"{paramName} must be a UTC DateTime.", paramName);

            return value;
        }
    }
}
