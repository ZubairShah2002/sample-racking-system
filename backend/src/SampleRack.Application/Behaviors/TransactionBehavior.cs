using SampleRack.Application.Abstractions.Messaging;
using SampleRack.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SampleRack.Application.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command execution in a unit-of-work transaction.
/// Only applies to requests implementing <see cref="ICommand"/> or <see cref="ICommand{TResponse}"/>;
/// queries pass through without transactional wrapping.
/// Uses the IExecutionStrategyFactory to support retrying execution strategies.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    IExecutionStrategyFactory executionStrategyFactory,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand())
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        return await executionStrategyFactory.ExecuteAsync(async () =>
        {
            logger.LogDebug("Beginning transaction for {RequestName}", requestName);

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var response = await next();

                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogDebug("Committed transaction for {RequestName}", requestName);

                return response;
            }
            catch
            {
                logger.LogWarning("Rolling back transaction for {RequestName}", requestName);

                await unitOfWork.RollbackTransactionAsync(cancellationToken);

                throw;
            }
        });
    }

    private static bool IsCommand()
    {
        var requestType = typeof(TRequest);
        return typeof(ICommand).IsAssignableFrom(requestType) ||
               requestType.GetInterfaces().Any(i =>
                   i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
