using Profiles.DAL.Entities;
using Profiles.Domain.Common;

namespace Profiles.BLL.Errors;

public class SpecializationErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound(nameof(Specialization));
}
