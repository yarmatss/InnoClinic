using Microsoft.AspNetCore.Authorization;

namespace Profiles.API.Authorization;

public sealed class ScopeRequirement(string scope) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
}
