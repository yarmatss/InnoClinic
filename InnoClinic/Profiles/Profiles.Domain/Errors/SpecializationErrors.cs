using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public static class SpecializationErrors
{
    public static readonly Error NotFound = new(
        "Specialization.NotFound", "The requested specialization was not found.");
}
