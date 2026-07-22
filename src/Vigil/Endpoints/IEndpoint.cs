namespace Vigil.Endpoints;

public interface IEndpoint
{
    static abstract string RoutePrefix { get; }
    
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}