using Microsoft.AspNetCore.Routing;

namespace InnoClinic.AspNetCore.Abstract;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
