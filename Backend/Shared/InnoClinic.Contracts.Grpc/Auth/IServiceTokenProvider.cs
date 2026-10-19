namespace InnoClinic.Contracts.Grpc.Auth;

public interface IServiceTokenProvider
{
    ValueTask<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
