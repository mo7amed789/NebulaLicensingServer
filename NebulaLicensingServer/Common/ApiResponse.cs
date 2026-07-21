namespace NebulaLicensingServer.Common;

public sealed class ApiResponse<T>
{
    private ApiResponse(bool success, string? message, string? errorCode, DateTime timestamp, T? data)
    {
        Success = success;
        Message = message;
        ErrorCode = errorCode;
        Timestamp = timestamp;
        Data = data;
    }

    public bool Success { get; }

    public string? Message { get; }

    public string? ErrorCode { get; }

    public DateTime Timestamp { get; }

    public T? Data { get; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, message, null, DateTime.UtcNow, data);

    public static ApiResponse<T> Fail(string message, string? errorCode = null) => new(false, message, errorCode, DateTime.UtcNow, default);
}
