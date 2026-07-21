namespace NebulaLicensingServer.Common;

public class Result<T> : Result
{
    private Result(bool isSuccess, T? value, string? errorCode, string? error)
        : base(isSuccess, errorCode, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public new static Result<T> Failure(string errorCode, string error) => new(false, default, errorCode, error);
}
