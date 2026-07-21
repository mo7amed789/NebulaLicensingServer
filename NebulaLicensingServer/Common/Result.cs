namespace NebulaLicensingServer.Common;

public class Result
{
    protected Result(bool isSuccess, string? errorCode, string? error)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? ErrorCode { get; }

    public string? Error { get; }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string errorCode, string error) => new(false, errorCode, error);
}
