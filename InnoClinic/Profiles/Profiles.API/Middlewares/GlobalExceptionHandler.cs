using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Profiles.API.Extensions;
using System.Diagnostics;

namespace Profiles.API.Middlewares;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogCriticalException(
            exception,
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId);

        var statusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Internal Server Error",
            Detail = "An unexpected system fault occurred. Please try again later.",
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", traceId);

        if (environment.IsDevelopment())
        {
            problemDetails.Extensions.Add("exceptionType", exception.GetType().Name);
            problemDetails.Extensions.Add("exceptionMessage", exception.Message);
            problemDetails.Extensions.Add("stackTrace", exception.StackTrace);
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
