using System.Security.Claims;
using InnoClinic.Core.Authorization;

namespace InnoClinic.AspNetCore.Extensions;

public static class ClaimsPrincipalExtensions
{
    public const string ClaimNamespace = "https://innoclinic";
    public const string ProfileIdClaim = $"{ClaimNamespace}/profile_id";
    public const string RoleClaim = $"{ClaimNamespace}/role";

    extension(ClaimsPrincipal? principal)
    {
        public string? GetUserId() =>
            principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue("sub");

        public string? GetEmail() =>
            principal?.FindFirstValue(ClaimTypes.Email)
            ?? principal?.FindFirstValue("email");

        public Guid? GetProfileId() =>
            Guid.TryParse(
                principal?.FindFirstValue(ProfileIdClaim) ?? principal?.FindFirstValue("profile_id"),
                out var id)
                ? id
                : null;

        public UserRole? GetUserRole() =>
            Enum.TryParse<UserRole>(
                principal?.FindFirstValue(RoleClaim)
                ?? principal?.FindFirstValue(ClaimTypes.Role)
                ?? principal?.FindFirstValue("role"),
                ignoreCase: true,
                out var role)
                ? role
                : null;

        public User? GetUser()
        {
            var userId = principal?.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var role = principal.GetUserRole();
            if (role is null)
                return null;

            var profileId = principal.GetProfileId();

            return new User(
                UserId: userId,
                PatientId: role == UserRole.Patient ? profileId : null,
                StaffId: role != UserRole.Patient ? profileId : null,
                Role: role.Value
            );
        }
    }
}
