using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Profiles.API.Extensions;
using Profiles.Domain.Constants;
using System.Diagnostics;

namespace Profiles.API.Middlewares;

internal sealed class BadRequestExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException)
        {
            return false;
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogBadRequestPayload(
            httpContext.Request.Path,
            traceId,
            exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ValidationConstants.ValidationFailedTitle,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (exception.InnerException is System.Text.Json.JsonException jsonException
            && !string.IsNullOrEmpty(jsonException.Path))
        {
            var fieldName = jsonException.Path == "$" 
                ? "body" 
                : jsonException.Path.Replace("$.", "");

            errors[fieldName] = [$"Invalid data format. Expected a valid value."];
        }
        else if (!string.IsNullOrEmpty(exception.Message)
            && !exception.Message.Contains("version"))
        {
            // Fallback for other standard BadHttpRequest exceptions
            errors["body"] = [exception.Message];
        }
        else
        {
            errors["body"] = ["The provided payload could not be parsed or contains invalid data."];
        }

        problemDetails.Extensions.Add("code", ValidationConstants.ValidationFailed);
        problemDetails.Extensions.Add("errors", errors);
        problemDetails.Extensions.Add("traceId", traceId);

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
