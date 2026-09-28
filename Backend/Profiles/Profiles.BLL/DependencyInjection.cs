using Auth0.ManagementApi;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Profiles.BLL.Interfaces;
using Profiles.BLL.Options;
using Profiles.BLL.Services;
using Profiles.DAL;

namespace Profiles.BLL;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBusinessLogicLayer(IConfiguration configuration)
        {
            services.AddDataAccess(configuration);
            services.AddMapster();

            services.AddOptions<Auth0ManagementOptions>()
                .Bind(configuration.GetSection(Auth0ManagementOptions.SectionName))
                .Configure(opts => opts.Domain ??= configuration["Auth0:Domain"]);

            services.AddSingleton<IManagementApiClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<Auth0ManagementOptions>>().Value;
                if (!options.IsConfigured)
                {
                    return null!;
                }

                return new ManagementClient(new ManagementClientOptions
                {
                    Domain = options.Domain!,
                    TokenProvider = new ClientCredentialsTokenProvider(
                        domain: options.Domain!,
                        clientId: options.ClientId!,
                        clientSecret: options.ClientSecret!
                    )
                });
            });

            services.AddScoped<IAuth0ManagementService, Auth0ManagementService>();

            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IMedicalStaffService, MedicalStaffService>();
            services.AddScoped<ISpecializationService, SpecializationService>();

            return services;
        }
    }
}
