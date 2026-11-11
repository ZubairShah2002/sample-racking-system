using SampleRack.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SampleRack.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IExecutionStrategyFactory. Delegates to the context's
/// configured execution strategy so retrying strategies work with user transactions.
/// </summary>
public sealed class EfExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly SampleRackDbContext _dbContext;

    public EfExecutionStrategyFactory(SampleRackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(operation);
    }
}
