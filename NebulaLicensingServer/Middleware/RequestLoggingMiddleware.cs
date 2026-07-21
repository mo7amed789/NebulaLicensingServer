using Serilog;
using System.Diagnostics;

namespace NebulaLicensingServer.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();
        await next(context);
        var elapsed = Stopwatch.GetElapsedTime(start);

        Log.Information("HTTP {Method} {Path} {StatusCode} {ExecutionTimeMs}ms",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
}
