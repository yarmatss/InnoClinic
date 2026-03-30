using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public class PatientErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound("Patient");
}
