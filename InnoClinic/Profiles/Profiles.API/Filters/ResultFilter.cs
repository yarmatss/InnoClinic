using Profiles.Domain.Common;
using Profiles.Domain.Constants;

namespace Profiles.API.Filters;

public class ResultFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var response = await next(context);

        if (response is not Result result)
        {
            return response;
        }

        if (result.IsFailure)
        {
            return HandleFailure(context, result);
        }

        return HandleSuccess(context, result);
    }

    private static IResult HandleFailure(EndpointFilterInvocationContext context, Result result)
    {
        var statusCode = result.Error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        if (result.Error is ValidationError validationError)
        {
            return Results.Problem(
                statusCode: statusCode,
                title: ValidationConstants.ValidationFailedTitle,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                instance: context.HttpContext.Request.Path,
                extensions: new Dictionary<string, object?>
                {
                    { "errors", validationError.Errors },
                    { "code", result.Error.Code }
                }
            );
        }

        return Results.Problem(
            statusCode: statusCode,
            title: "An error occurred while processing the request.",
            detail: result.Error.Description,
            extensions: new Dictionary<string, object?> { { "code", result.Error.Code } }
        );
    }

    private static IResult HandleSuccess(EndpointFilterInvocationContext context, Result result)
    {
        if (result is not IValueResult valueResult)
        {
            return Results.NoContent();
        }

        if (result.IsCreated)
        {
            return !string.IsNullOrEmpty(result.Location)
                ? Results.Created(result.Location, valueResult.Value)
                : Results.Created(context.HttpContext.Request.Path, valueResult.Value);
        }

        return Results.Ok(valueResult.Value);
    }
}
