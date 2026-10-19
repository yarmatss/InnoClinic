using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using InnoClinic.Core.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

internal sealed record ClinicUser(string Id, UserRole Role, Guid ProfileId) : ICurrentUser
{
    public static bool TryCreate(ClaimsPrincipal? principal, [NotNullWhen(true)] out ClinicUser? user)
    {
        user = null;

        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        var id = principal.FindFirstValue(ClinicClaims.Subject);
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (!TryParseRole(principal.FindFirstValue(ClinicClaims.Role), out var role))
            return false;

        if (!Guid.TryParse(principal.FindFirstValue(ClinicClaims.ProfileId), out var profileId))
            return false;

        user = new ClinicUser(id, role, profileId);
        return true;
    }

    private static bool TryParseRole(string? value, out UserRole role)
    {
        var name = Enum.GetNames<UserRole>()
            .FirstOrDefault(n => string.Equals(n, value, StringComparison.OrdinalIgnoreCase));

        role = name is null ? default : Enum.Parse<UserRole>(name);
        return name is not null;
    }
}
