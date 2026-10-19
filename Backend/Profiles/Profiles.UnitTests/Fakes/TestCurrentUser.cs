using InnoClinic.Core.Authorization;

namespace Profiles.UnitTests.Fakes;

internal sealed record TestCurrentUser(string Id, UserRole Role, Guid ProfileId) : ICurrentUser
{
    public static TestCurrentUser Administrator() => new("test-user", UserRole.Administrator, Guid.NewGuid());
}
