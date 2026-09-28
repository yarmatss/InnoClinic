using InnoClinic.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Appointments.API.Authorization;

public static class Policies
{
    public const string ReadAppointments = nameof(ReadAppointments);
    public const string WriteAppointments = nameof(WriteAppointments);
    public const string ConfirmAppointments = nameof(ConfirmAppointments);
    public const string ReadResults = nameof(ReadResults);
    public const string WriteResults = nameof(WriteResults);

    public const string ScopeReadAppointments = ClinicPermissions.Appointments.Read;
    public const string ScopeWriteAppointments = ClinicPermissions.Appointments.Write;
    public const string ScopeConfirmAppointments = ClinicPermissions.Appointments.Confirm;
    public const string ScopeReadResults = ClinicPermissions.Results.Read;
    public const string ScopeWriteResults = ClinicPermissions.Results.Write;

    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopePolicies()
        {
            services.AddScopeAuthorizationHandler();

            services.AddAuthorizationBuilder()
                .AddScopePolicy(ReadAppointments, ClinicPermissions.Appointments.Read)
                .AddScopePolicy(WriteAppointments, ClinicPermissions.Appointments.Write)
                .AddScopePolicy(ConfirmAppointments, ClinicPermissions.Appointments.Confirm)
                .AddScopePolicy(ReadResults, ClinicPermissions.Results.Read)
                .AddScopePolicy(WriteResults, ClinicPermissions.Results.Write);

            return services;
        }
    }
}
