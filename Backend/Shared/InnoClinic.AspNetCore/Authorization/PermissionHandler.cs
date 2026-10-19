using Microsoft.AspNetCore.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

internal sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.FindAll(ClinicClaims.Permissions)
            .Any(c => string.Equals(c.Value, requirement.Permission, StringComparison.Ordinal));

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
