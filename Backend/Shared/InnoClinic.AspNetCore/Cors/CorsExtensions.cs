using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InnoClinic.AspNetCore.Cors;

public static class CorsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFrontendCors(IConfiguration configuration)
        {
            services.AddOptions<FrontendCorsOptions>()
                .Bind(configuration.GetSection(FrontendCorsOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(
                    o => o.AllowedOrigins is null || o.AllowedOrigins.All(IsOrigin),
                    "Cors:AllowedOrigins must contain origins only, e.g. https://app.example.com (no path or trailing slash).")
                .ValidateOnStart();

            services.AddCors();
            services.AddOptions<CorsOptions>()
                .Configure<IOptions<FrontendCorsOptions>>((cors, frontend) =>
                    cors.AddDefaultPolicy(policy => policy
                        .WithOrigins(frontend.Value.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

            return services;
        }
    }

    private static bool IsOrigin(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
        && string.Equals(uri.GetLeftPart(UriPartial.Authority), value, StringComparison.OrdinalIgnoreCase);
}
