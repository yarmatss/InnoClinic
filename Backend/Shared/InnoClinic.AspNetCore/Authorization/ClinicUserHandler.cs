using Microsoft.AspNetCore.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

internal sealed class ClinicUserHandler : AuthorizationHandler<ClinicUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClinicUserRequirement requirement)
    {
        if (ClinicUser.TryCreate(context.User, out _))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
