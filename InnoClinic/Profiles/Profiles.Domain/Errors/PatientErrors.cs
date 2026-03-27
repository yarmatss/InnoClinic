using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public static class PatientErrors
{
    public static readonly Error NotFound = new(
        "Patient.NotFound", "The requested patient was not found.");
}
