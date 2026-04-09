using Profiles.Domain.Common;

namespace Profiles.API.Filters;

public class ResultFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var response = await next(context);

        if (response is Result result)
        {
            if (result.IsFailure)
            {
                return result.Error.Type switch
                {
                    ErrorType.NotFound => Results.NotFound(result.Error),
                    _ => Results.BadRequest(result.Error)
                };
            }

            if (result is IValueResult valueResult)
            {
                return Results.Ok(valueResult.Value);
            }

            return Results.NoContent();
        }

        return response;
    }
}
