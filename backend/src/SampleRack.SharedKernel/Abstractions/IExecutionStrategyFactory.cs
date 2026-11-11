namespace SampleRack.SharedKernel.Abstractions;

/// <summary>
/// Abstraction for database execution strategy, enabling retrying execution strategies
/// to work correctly with user-initiated transactions.
/// </summary>
public interface IExecutionStrategyFactory
{
    /// <summary>
    /// Executes the given operation within the configured execution strategy.
    /// </summary>
    Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation);
}
