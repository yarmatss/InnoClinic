using Microsoft.AspNetCore.Authorization;

namespace Appointments.API.Authorization;

public static class Policies
{
    public const string ReadAppointments = nameof(ReadAppointments);
    public const string WriteAppointments = nameof(WriteAppointments);
    public const string ConfirmAppointments = nameof(ConfirmAppointments);
    public const string ReadResults = nameof(ReadResults);
    public const string WriteResults = nameof(WriteResults);

    public const string ScopeReadAppointments = "read:appointments";
    public const string ScopeWriteAppointments = "write:appointments";
    public const string ScopeConfirmAppointments = "confirm:appointments";
    public const string ScopeReadResults = "read:results";
    public const string ScopeWriteResults = "write:results";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopePolicies()
        {
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();

            services.AddAuthorizationBuilder()
                .AddPolicy(ReadAppointments, p => p.Requirements.Add(new ScopeRequirement(ScopeReadAppointments)))
                .AddPolicy(WriteAppointments, p => p.Requirements.Add(new ScopeRequirement(ScopeWriteAppointments)))
                .AddPolicy(ConfirmAppointments, p => p.Requirements.Add(new ScopeRequirement(ScopeConfirmAppointments)))
                .AddPolicy(ReadResults, p => p.Requirements.Add(new ScopeRequirement(ScopeReadResults)))
                .AddPolicy(WriteResults, p => p.Requirements.Add(new ScopeRequirement(ScopeWriteResults)));

            return services;
        }
    }
}
