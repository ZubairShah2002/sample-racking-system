using System.Linq.Expressions;

namespace SampleRack.SharedKernel.Abstractions;

/// <summary>
/// Specification pattern interface. Encapsulates a query predicate along with
/// include expressions, ordering, and pagination configuration.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// The filter criteria expression.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Navigation property includes for eager loading.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// String-based includes for nested navigation properties.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Order-by expression (ascending).
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order-by expression (descending).
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Number of items to take (for pagination).
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Number of items to skip (for pagination).
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether pagination is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }
}
