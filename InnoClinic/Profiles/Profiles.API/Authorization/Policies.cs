using Microsoft.AspNetCore.Authorization;

namespace Profiles.API.Authorization;

public static class Policies
{
    public const string ReadPatients = nameof(ReadPatients);
    public const string WritePatients = nameof(WritePatients);
    public const string ReadStaff = nameof(ReadStaff);
    public const string WriteStaff = nameof(WriteStaff);
    public const string WriteSpecializations = nameof(WriteSpecializations);

    private const string ScopeReadPatients = "read:patients";
    private const string ScopeWritePatients = "write:patients";
    private const string ScopeReadStaff = "read:staff";
    private const string ScopeWriteStaff = "write:staff";
    private const string ScopeWriteSpecializations = "write:specializations";

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
