using Microsoft.AspNetCore.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

public sealed class ScopeHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        var hasPermission = context.User.FindAll("permissions")
            .Any(c => string.Equals(c.Value, requirement.Scope, StringComparison.Ordinal));

        if (hasPermission)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var scopeClaim = context.User.FindFirst(c => c.Type == "scope");
        if (scopeClaim is not null)
        {
            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (scopes.Contains(requirement.Scope, StringComparer.Ordinal))
                context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
