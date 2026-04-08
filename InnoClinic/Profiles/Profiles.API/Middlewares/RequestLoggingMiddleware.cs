using Profiles.API.Extensions;
using System.Diagnostics;

namespace Profiles.API.Middlewares;

public partial class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogRequestStarted(context.Request.Method, context.Request.Path);
        long startTimestamp = Stopwatch.GetTimestamp();
        await next(context);

        TimeSpan elapsed = Stopwatch.GetElapsedTime(startTimestamp);

        logger.LogRequestFinished(
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
}
