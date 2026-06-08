using Microsoft.AspNetCore.Authorization;

namespace Appointments.API.Authorization;

public sealed class ScopeRequirement(string scope) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
}
