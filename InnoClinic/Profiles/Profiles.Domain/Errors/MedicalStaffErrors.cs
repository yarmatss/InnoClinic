using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public class MedicalStaffErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound("MedicalStaff");
}
