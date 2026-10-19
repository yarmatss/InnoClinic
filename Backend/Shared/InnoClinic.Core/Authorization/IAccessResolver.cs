namespace InnoClinic.Core.Authorization;

public interface IAccessResolver
{
    AccessLevel Check<TResource>(TResource resource, Operation operation);
}
