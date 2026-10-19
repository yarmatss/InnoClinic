namespace InnoClinic.Core.Authorization;

public static class CurrentUserExtensions
{
    extension(ICurrentUser user)
    {
        public bool IsInRole(UserRole role) => user.Role == role;

        public bool Owns(Guid profileId) => user.ProfileId == profileId;
    }
}
