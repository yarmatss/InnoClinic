using InnoClinic.Core.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace InnoClinic.AspNetCore.Authorization;

internal sealed class AccessResolver(ICurrentUser user, IServiceProvider serviceProvider) : IAccessResolver
{
    public AccessLevel Check<TResource>(TResource resource, Operation operation)
    {
        var policy = serviceProvider.GetService<IAccessPolicy<TResource>>()
            ?? throw new InvalidOperationException(
                $"No {nameof(IAccessPolicy<>)} is registered for {typeof(TResource).Name}.");

        return policy.Evaluate(user, resource, operation);
    }
}
