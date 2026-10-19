using Auth0.ManagementApi;
using InnoClinic.Core.Authorization;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Profiles.BLL.Authorization;
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
                    throw new InvalidOperationException(
                        $"{Auth0ManagementOptions.SectionName} (Domain, ClientId, ClientSecret) must be configured to provision user accounts.");
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

            services.AddSingleton(sp => new Lazy<IManagementApiClient>(sp.GetRequiredService<IManagementApiClient>));
            services.AddScoped<IAuthManagementService, Auth0ManagementService>();

            services.AddSingleton<IAccessPolicy<PatientRef>, PatientAccessPolicy>();
            services.AddSingleton<IAccessPolicy<PatientDirectory>, PatientAccessPolicy>();
            services.AddSingleton<IAccessPolicy<StaffRef>, StaffAccessPolicy>();

            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IMedicalStaffService, MedicalStaffService>();
            services.AddScoped<ISpecializationService, SpecializationService>();

            return services;
        }
    }
}
