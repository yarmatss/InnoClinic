using InnoClinic.Core.Authorization;
using Microsoft.AspNetCore.Http;

namespace InnoClinic.AspNetCore.Authorization;

internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClinicUser? _user;

    private ClinicUser User => _user ??= ClinicUser.TryCreate(httpContextAccessor.HttpContext?.User, out var user)
        ? user
        : throw new InvalidOperationException(
            "The current request has no authenticated clinic user. Protect the endpoint with a clinic user policy.");

    public string Id => User.Id;
    public UserRole Role => User.Role;
    public Guid ProfileId => User.ProfileId;
}
