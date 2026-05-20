using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InnoClinic.AspNetCore.Extensions;
using InnoClinic.Core.Constants;
using System.Diagnostics;
using System.Text.Json;

namespace InnoClinic.AspNetCore.Middlewares;

public sealed class ValidationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var errors = validationException.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

        logger.LogValidationFailure(
            httpContext.Request.Path,
            traceId,
            JsonSerializer.Serialize(errors));

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ValidationConstants.ValidationFailedTitle,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        problemDetails.Extensions.Add("code", ValidationConstants.ValidationFailed);
        problemDetails.Extensions.Add("errors", errors);
        problemDetails.Extensions.Add("traceId", traceId);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
