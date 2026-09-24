using System.Security.Claims;

namespace InnoClinic.AspNetCore.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        public string? GetUserId()
        {
            if (principal is null)
                return null;

            return principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirst("sub")?.Value;
        }

        public string? GetEmail()
        {
            if (principal is null)
                return null;

            return principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirst("email")?.Value;
        }
    }
}
