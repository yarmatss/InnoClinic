using InnoClinic.AspNetCore.Extensions;
using InnoClinic.Core.Authorization;
using Microsoft.AspNetCore.Http;

namespace InnoClinic.AspNetCore.Authorization;

public class ClaimsUserResolver(IHttpContextAccessor httpContextAccessor) : IUserResolver
{
    public User? Resolve(string userId)
    {
        var user = httpContextAccessor.HttpContext?.User.GetUser();
        if (user is null || !string.Equals(user.UserId, userId, StringComparison.Ordinal))
            return null;

        return user;
    }
}
