using Profiles.API.Extensions;
using System.Diagnostics;

namespace Profiles.API.Middlewares;

internal sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        logger.LogRequestStarted(
            context.Request.Method,
            context.Request.Path,
            traceId);

        long startTimestamp = Stopwatch.GetTimestamp();

        await next(context);

        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTimestamp);

        logger.LogRequestFinished(
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds,
            traceId);
    }
}
