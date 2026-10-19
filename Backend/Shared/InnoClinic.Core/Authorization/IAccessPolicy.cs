namespace InnoClinic.Core.Authorization;

public interface IAccessPolicy<TResource>
{
    AccessLevel Evaluate(ICurrentUser user, TResource resource, Operation operation);
}
