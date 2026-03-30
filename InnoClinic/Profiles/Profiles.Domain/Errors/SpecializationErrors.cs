using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public class SpecializationErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound("Specialization");
}
