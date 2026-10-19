namespace InnoClinic.AspNetCore.Authorization;

public static class ClinicClaims
{
    public const string Namespace = "https://innoclinic";

    public const string Subject = "sub";
    public const string Role = $"{Namespace}/role";
    public const string ProfileId = $"{Namespace}/profile_id";
    public const string Permissions = "permissions";
    public const string Scope = "scope";
    public const string GrantType = "gty";

    public const string ClientCredentialsGrant = "client-credentials";
}
