using Profiles.DAL.Entities;
using Profiles.Domain.Common;

namespace Profiles.BLL.Errors;

public class MedicalStaffErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound(nameof(MedicalStaff));

    public static readonly Error DuplicateNationalId = CreateConflict(
        "MedicalStaff.DuplicateNationalId", 
        "A medical staff member with this National ID already exists.");

    public static readonly Error DuplicateLicenseNumber = CreateConflict(
        "MedicalStaff.DuplicateLicenseNumber", 
        "A medical staff member with this License Number already exists.");

    public static readonly Error SupervisorNotFound = CreateNotFound("ActiveSupervisor");

    public static readonly Error InvalidSpecialization = CreateConflict(
        "MedicalStaff.InvalidSpecialization", 
        "One or more assigned specializations do not exist.");
}
