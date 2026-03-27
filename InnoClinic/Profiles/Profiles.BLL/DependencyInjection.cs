using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profiles.DAL;
using Mapster;
using Profiles.BLL.Services;
using Profiles.BLL.Interfaces;

namespace Profiles.BLL;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBusinessLogicLayer(IConfiguration configuration)
        {
            services.AddDataAccess(configuration);
            services.AddMapster();

            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IMedicalStaffService, MedicalStaffService>();
            services.AddScoped<ISpecializationService, SpecializationService>();

            return services;
        }
    }
}
