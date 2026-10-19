namespace InnoClinic.AspNetCore.Authorization;

public static class InternalScopes
{
    public const string Profiles = "internal:profiles";
    public const string Appointments = "internal:appointments";

    public static IReadOnlyList<string> All { get; } = [Profiles, Appointments];
}
