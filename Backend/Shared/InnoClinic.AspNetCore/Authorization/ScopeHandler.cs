using Microsoft.AspNetCore.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

internal sealed class ScopeHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        var isServiceToken = context.User.HasClaim(ClinicClaims.GrantType, ClinicClaims.ClientCredentialsGrant);

        var hasScope = context.User.FindAll(ClinicClaims.Scope)
            .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains(requirement.Scope, StringComparer.Ordinal);

        if (isServiceToken && hasScope)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
