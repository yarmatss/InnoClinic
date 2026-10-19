namespace InnoClinic.Contracts.Grpc.Auth;

public sealed class ServiceTokenOptions
{
    public required string Domain { get; set; }
    public required string Audience { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public TimeSpan RefreshBeforeExpiry { get; set; } = TimeSpan.FromMinutes(5);
}
