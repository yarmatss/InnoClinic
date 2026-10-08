using Profiles.DAL.Entities;
using InnoClinic.Core.Common;

namespace Profiles.BLL.Errors;

public class SpecializationErrors : DomainErrors
{
    public static readonly Error NotFound = CreateNotFound(nameof(Specialization));

    public static readonly Error DuplicateName = CreateConflict(
        "Specialization.DuplicateName",
        "A specialization with this name already exists.");

    public static readonly Error Unauthorized = CreateUnauthorized(
        "Specialization.Unauthorized",
        "User identifier claim not found.");

    public static readonly Error Forbidden = CreateForbidden(
        "Specialization.Forbidden",
        "You do not have permission to perform this action or access this resource.");
}
