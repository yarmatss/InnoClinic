using Microsoft.AspNetCore.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

public sealed class ScopeRequirement(string scope) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
}
