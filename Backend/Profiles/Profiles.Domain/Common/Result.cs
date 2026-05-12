namespace Profiles.Domain.Common;

public interface IValueResult
{
    object? Value { get; }
}

public class Result
{
    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    public static Result<T> Created<T>(T value, string location) => new(value, true, Error.None)
    {
        IsCreated = true,
        Location = location
    };

    public bool IsCreated { get; protected set; }
    public string? Location { get; protected set; }
}

public class Result<T> : Result, IValueResult
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    object? IValueResult.Value => Value;

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        return IsSuccess ? mapper(Value) : Error;
    }

    public Result<TOut> Map<TOut>(Func<T, Result<TOut>> mapper)
    {
        return IsSuccess ? mapper(Value) : Error;
    }

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}
