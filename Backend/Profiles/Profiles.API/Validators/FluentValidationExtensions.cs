using FluentValidation;
using System.Text.Json;

namespace Profiles.API.Validators;

public static class FluentValidationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureFluentValidation()
        {
            ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) =>
                member != null
                    ? JsonNamingPolicy.CamelCase.ConvertName(member.Name)
                    : null;

            ValidatorOptions.Global.DisplayNameResolver = ValidatorOptions.Global.PropertyNameResolver;

            return services;
        }
    }
}
