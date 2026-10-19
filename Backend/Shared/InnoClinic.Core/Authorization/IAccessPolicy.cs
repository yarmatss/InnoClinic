namespace InnoClinic.Core.Authorization;

public interface IAccessPolicy<in TResource>
{
    AccessLevel Evaluate(ICurrentUser user, TResource resource, Operation operation);
}
