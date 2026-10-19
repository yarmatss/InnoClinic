using InnoClinic.Core.Authorization;

namespace Profiles.UnitTests.Fakes;

internal sealed class TestAccessResolver(ICurrentUser user, params object[] policies) : IAccessResolver
{
    public AccessLevel Check<TResource>(TResource resource, Operation operation)
    {
        var policy = policies.OfType<IAccessPolicy<TResource>>().FirstOrDefault()
            ?? throw new InvalidOperationException($"No policy for {typeof(TResource).Name}.");

        return policy.Evaluate(user, resource, operation);
    }
}
