namespace SampleRack.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// Follows the Result pattern to avoid throwing exceptions for expected failures.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// Supports implicit conversions, Map, and Bind for functional composition.
/// </summary>
/// <typeparam name="T">The type of the value on success.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    /// <summary>
    /// Gets the value. Throws if the result is a failure.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access the value of a failed result. Error: {Error.Code} - {Error.Message}");

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Transforms the value using the provided mapping function if the result is successful.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapFunc)
    {
        return IsSuccess
            ? Result<TOut>.Success(mapFunc(Value))
            : Result<TOut>.Failure(Error);
    }

    /// <summary>
    /// Chains a result-returning operation if the current result is successful.
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bindFunc)
    {
        return IsSuccess
            ? bindFunc(Value)
            : Result<TOut>.Failure(Error);
    }

    /// <summary>
    /// Executes an action on the value if the result is successful, returning the original result.
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(Value);

        return this;
    }

    /// <summary>
    /// Returns the value if successful, or the provided default value if failed.
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value : defaultValue;
    }

    /// <summary>
    /// Pattern matches on success or failure, returning a unified result type.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
