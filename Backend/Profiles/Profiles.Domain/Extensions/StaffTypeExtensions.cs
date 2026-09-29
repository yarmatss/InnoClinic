using Profiles.Domain.Enums;

namespace Profiles.Domain.Extensions;

public static class StaffTypeExtensions
{
    extension(StaffType staffType)
    {
        public UserRole ToUserRole() => staffType switch
        {
            StaffType.Doctor => UserRole.Doctor,
            StaffType.Administrator => UserRole.Administrator,
            StaffType.Receptionist => UserRole.Receptionist,
            _ => throw new ArgumentOutOfRangeException(nameof(staffType), staffType, "Invalid staff type for user role mapping.")
        };
    }
}
