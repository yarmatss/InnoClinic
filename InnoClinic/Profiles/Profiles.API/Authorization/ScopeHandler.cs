using Microsoft.AspNetCore.Authorization;

namespace Profiles.API.Authorization;

public sealed class ScopeHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        var scopeClaim = context.User.FindFirst(c => c.Type == "scope");

        if (scopeClaim is null)
            return Task.CompletedTask;

        var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (scopes.Contains(requirement.Scope, StringComparer.Ordinal))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
