using InnoClinic.Core.Common;
using Profiles.DAL.Entities;

namespace Profiles.BLL.Errors;

public class SpecializationErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound(nameof(Specialization));

    public static readonly Error DuplicateName = CreateConflict(
        "Specialization.DuplicateName",
        "A specialization with this name already exists.");
}
