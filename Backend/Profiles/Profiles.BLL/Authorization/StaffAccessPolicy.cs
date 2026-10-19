using InnoClinic.Core.Authorization;

namespace Profiles.BLL.Authorization;

internal sealed class StaffAccessPolicy : IAccessPolicy<StaffRef>
{
    public AccessLevel Evaluate(ICurrentUser user, StaffRef staff, Operation operation)
    {
        if (user.IsInRole(UserRole.Administrator))
            return AccessLevel.Full;

        if (operation != Operation.Read)
            return AccessLevel.None;

        return user.IsInRole(UserRole.Patient) || !user.Owns(staff.StaffId)
            ? AccessLevel.Masked
            : AccessLevel.Full;
    }
}
