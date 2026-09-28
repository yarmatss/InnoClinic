namespace InnoClinic.Core.Common;

public abstract class DomainErrors
{
    protected static Error CreateNotFound(string entityName) => new(
        $"{entityName}.NotFound",
        $"The requested {entityName.ToLower()} was not found.",
        ErrorType.NotFound);

    protected static Error CreateConflict(string code, string description) => new(
        code,
        description,
        ErrorType.Conflict);

    protected static Error CreateUnauthorized(string code = "Auth.Unauthorized", string description = "The request is unauthorized.") => new(
        code,
        description,
        ErrorType.Unauthorized);

    protected static Error CreateForbidden(string code = "Auth.Forbidden", string description = "You do not have permission to access this resource.") => new(
        code,
        description,
        ErrorType.Forbidden);
}
