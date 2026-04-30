using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Profiles.API.Extensions;

public static class AuthExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAuth0Authentication(IConfiguration configuration)
        {
            var domain = configuration["Auth0:Domain"] ?? throw new InvalidOperationException("Auth0:Domain is not configured.");
            var audience = configuration["Auth0:Audience"] ?? throw new InvalidOperationException("Auth0:Audience is not configured.");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{domain}/";
                    options.Audience = audience;
                    options.TokenValidationParameters = new()
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };
                });

            return services;
        }
    }
}
