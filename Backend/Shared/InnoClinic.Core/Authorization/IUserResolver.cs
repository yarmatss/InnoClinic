namespace InnoClinic.Core.Authorization;

public interface IUserResolver
{
    User? Resolve(string userId);
}
