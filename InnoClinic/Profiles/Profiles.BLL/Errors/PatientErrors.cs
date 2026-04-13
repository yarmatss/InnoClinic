using Profiles.DAL.Entities;
using Profiles.Domain.Common;

namespace Profiles.BLL.Errors;

public class PatientErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound(nameof(Patient));

    public static readonly Error DuplicateNationalId = CreateConflict(
        "Patient.DuplicateNationalId", 
        "A patient with this National ID already exists.");

    public static readonly Error DuplicateInsuranceNumber = CreateConflict(
        "Patient.DuplicateInsuranceNumber", 
        "A patient with this Insurance Number already exists.");

    public static readonly Error PrimaryDoctorNotFound = CreateNotFound("ActivePrimaryDoctor");
}
