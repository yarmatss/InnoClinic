using Microsoft.AspNetCore.Authorization;

namespace InnoClinic.AspNetCore.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
