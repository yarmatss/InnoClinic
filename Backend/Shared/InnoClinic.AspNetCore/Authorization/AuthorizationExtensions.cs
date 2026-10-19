using InnoClinic.Core.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InnoClinic.AspNetCore.Authorization;

public static class AuthorizationExtensions
{
    public const string InternalScheme = "Internal";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddClinicAuth(IConfiguration configuration)
        {
            services.AddOptions<Auth0Options>()
                .Bind(configuration.GetSection(Auth0Options.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer()
                .AddJwtBearer(InternalScheme);

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<Auth0Options>>((jwt, auth0) =>
                {
                    ConfigureJwt(jwt, auth0.Value.Domain, auth0.Value.Audience);

                    // gRPC is only used between services, so its calls are authenticated as internal tokens.
                    jwt.ForwardDefaultSelector = context =>
                        context.Request.ContentType?.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase) == true
                            ? InternalScheme
                            : null;
                });

            services.AddOptions<JwtBearerOptions>(InternalScheme)
                .Configure<IOptions<Auth0Options>>((jwt, auth0) =>
                    ConfigureJwt(jwt, auth0.Value.Domain, auth0.Value.InternalAudience));

            var clinicUserPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ClinicUserRequirement())
                .Build();

            var authorization = services.AddAuthorizationBuilder()
                .SetDefaultPolicy(clinicUserPolicy)
                .SetFallbackPolicy(clinicUserPolicy);

            foreach (var permission in ClinicPermissions.All)
            {
                authorization.AddPolicy(permission, policy => policy
                    .Combine(clinicUserPolicy)
                    .AddRequirements(new PermissionRequirement(permission)));
            }

            foreach (var scope in InternalScopes.All)
            {
                authorization.AddPolicy(scope, policy => policy
                    .AddAuthenticationSchemes(InternalScheme)
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ScopeRequirement(scope)));
            }

            services.AddSingleton<IAuthorizationHandler, ClinicUserHandler>();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, HttpCurrentUser>();
            services.AddScoped<IAccessResolver, AccessResolver>();

            return services;
        }
    }

    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        public TBuilder RequirePermission(string permission) => builder.RequireAuthorization(permission);
        public TBuilder RequireInternalScope(string scope) => builder.RequireAuthorization(scope);
    }

    private static void ConfigureJwt(JwtBearerOptions options, string domain, string audience)
    {
        options.Authority = $"https://{domain}/";
        options.Audience = audience;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
        options.TokenValidationParameters.NameClaimType = ClinicClaims.Subject;
        options.TokenValidationParameters.RoleClaimType = ClinicClaims.Role;
    }
}
