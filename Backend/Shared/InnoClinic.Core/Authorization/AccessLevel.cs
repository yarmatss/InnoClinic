namespace InnoClinic.Core.Authorization;

public enum AccessLevel
{
    None = 0,
    Masked = 1,
    Full = 2
}

public static class AccessLevelExtensions
{
    extension(AccessLevel level)
    {
        public bool IsAllowed => level != AccessLevel.None;
    }
}
