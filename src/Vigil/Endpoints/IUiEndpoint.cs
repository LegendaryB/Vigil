namespace Vigil.Endpoints;

public interface IUiEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
