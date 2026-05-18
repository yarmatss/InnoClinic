using FluentValidation;
using FluentValidation.Results;
using InnoClinic.Core.Common;
using MediatR;

namespace Appointments.API.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errorsDictionary = validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .GroupBy(
                x => x.PropertyName,
                x => x.ErrorMessage,
                (propertyName, errorMessages) => new
                {
                    Key = propertyName,
                    Values = errorMessages.Distinct().ToArray()
                })
            .ToDictionary(x => x.Key, x => x.Values);

        if (errorsDictionary.Count != 0)
        {
            return CreateValidationResult(errorsDictionary);
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateValidationResult(IDictionary<string, string[]> errors)
    {
        var validationError = new ValidationError(errors);

        if (typeof(TResponse) == typeof(Result))
        {
            return (Result.Failure(validationError) as TResponse)!;
        }

        object result = typeof(Result)
            .GetMethods()
            .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
            .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0])
            .Invoke(null, [validationError])!;

        return (TResponse)result;
    }
}
