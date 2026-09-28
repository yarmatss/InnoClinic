using InnoClinic.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Profiles.API.Authorization;

public static class Policies
{
    public const string ReadPatients = nameof(ReadPatients);
    public const string WritePatients = nameof(WritePatients);
    public const string ReadStaff = nameof(ReadStaff);
    public const string WriteStaff = nameof(WriteStaff);
    public const string WriteSpecializations = nameof(WriteSpecializations);

    public const string ScopeReadPatients = ClinicPermissions.Patients.Read;
    public const string ScopeWritePatients = ClinicPermissions.Patients.Write;
    public const string ScopeReadStaff = ClinicPermissions.Staff.Read;
    public const string ScopeWriteStaff = ClinicPermissions.Staff.Write;
    public const string ScopeWriteSpecializations = ClinicPermissions.Specializations.Write;

    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopePolicies()
        {
            services.AddScopeAuthorizationHandler();

            services.AddAuthorizationBuilder()
                .AddScopePolicy(ReadPatients, ClinicPermissions.Patients.Read)
                .AddScopePolicy(WritePatients, ClinicPermissions.Patients.Write)
                .AddScopePolicy(ReadStaff, ClinicPermissions.Staff.Read)
                .AddScopePolicy(WriteStaff, ClinicPermissions.Staff.Write)
                .AddScopePolicy(WriteSpecializations, ClinicPermissions.Specializations.Write);

            return services;
        }
    }
}
