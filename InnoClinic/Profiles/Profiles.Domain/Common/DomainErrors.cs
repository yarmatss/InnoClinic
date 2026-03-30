namespace Profiles.Domain.Common;

public abstract class DomainErrors
{
    protected static Error CreateNotFound(string entityName) => new(
        $"{entityName}.NotFound",
        $"The requested {entityName.ToLower()} was not found.");
}
