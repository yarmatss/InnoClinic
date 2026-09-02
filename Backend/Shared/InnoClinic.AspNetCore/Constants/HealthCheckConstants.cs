namespace InnoClinic.AspNetCore.Constants;

public static class HealthCheckConstants
{
    public const string LiveTag = "live";
    public const string ReadyTag = "ready";

    public const string LiveEndpoint = "/health/live";
    public const string ReadyEndpoint = "/health/ready";

    public const string Self = "self";
    public const string Postgres = "postgres";
    public const string Redis = "redis";
    public const string CloudAmqp = "cloudamqp";
}
