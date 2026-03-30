using Profiles.Domain.Common;

namespace Profiles.Domain.Errors;

public abstract class DomainErrors
{
    protected static Error CreateNotFound(string entityName) => new(
        $"{entityName}.NotFound",
        $"The requested {entityName.ToLower()} was not found.");
}
