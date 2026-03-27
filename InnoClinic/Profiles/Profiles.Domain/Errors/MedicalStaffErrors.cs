using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public static class MedicalStaffErrors
{
    public static readonly Error NotFound = new(
        "MedicalStaff.NotFound", "The requested medical staff member was not found.");
}
