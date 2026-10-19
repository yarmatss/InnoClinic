namespace InnoClinic.Core.Authorization;

public interface ICurrentUser
{
    string Id { get; }
    UserRole Role { get; }
    Guid ProfileId { get; }
}
