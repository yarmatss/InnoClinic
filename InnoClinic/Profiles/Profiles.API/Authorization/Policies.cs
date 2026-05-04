using Microsoft.AspNetCore.Authorization;

namespace Profiles.API.Authorization;

public static class Policies
{
    public const string ReadPatients = nameof(ReadPatients);
    public const string WritePatients = nameof(WritePatients);
    public const string ReadStaff = nameof(ReadStaff);
    public const string WriteStaff = nameof(WriteStaff);
    public const string WriteSpecializations = nameof(WriteSpecializations);

    public const string ScopeReadPatients = "read:patients";
    public const string ScopeWritePatients = "write:patients";
    public const string ScopeReadStaff = "read:staff";
    public const string ScopeWriteStaff = "write:staff";
    public const string ScopeWriteSpecializations = "write:specializations";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopePolicies()
        {
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();

            services.AddAuthorizationBuilder()
                .AddPolicy(ReadPatients, p => p.Requirements.Add(new ScopeRequirement(ScopeReadPatients)))
                .AddPolicy(WritePatients, p => p.Requirements.Add(new ScopeRequirement(ScopeWritePatients)))
                .AddPolicy(ReadStaff, p => p.Requirements.Add(new ScopeRequirement(ScopeReadStaff)))
                .AddPolicy(WriteStaff, p => p.Requirements.Add(new ScopeRequirement(ScopeWriteStaff)))
                .AddPolicy(WriteSpecializations, p => p.Requirements.Add(new ScopeRequirement(ScopeWriteSpecializations)));

            return services;
        }
    }
}
