using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace InnoClinic.AspNetCore.Authorization;

public static class AuthorizationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopeAuthorizationHandler()
        {
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();
            return services;
        }
    }

    extension(AuthorizationBuilder builder)
    {
        public AuthorizationBuilder AddScopePolicy(string policyName, string scope)
        {
            return builder.AddPolicy(policyName, policy =>
                policy.Requirements.Add(new ScopeRequirement(scope)));
        }
    }
}
