using SampleRack.SharedKernel.Results;
using MediatR;

namespace SampleRack.Application.Abstractions.Messaging;

/// <summary>
/// Handler for queries that return a <see cref="Result{TResponse}"/> envelope.
/// </summary>
/// <typeparam name="TQuery">The query type being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response payload.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
