using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;

namespace Vigil.Features.ClientKeys;

internal class GetClientKeysFeature : IEndpoint
{
    public static string RoutePrefix => Routes.ClientKeys;
    
    private record GetClientKeyResponse(
        Guid Id,
        string ClientName,
        string ApiKey,
        DateTime CreatedAt
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(RoutePrefix + "/", (ClientKeyRepository repository) =>
            {
                var keys = repository.Get();

                var response = keys.Select(k => new GetClientKeyResponse(
                    k.Id,
                    k.ClientName,
                    k.ApiKey,
                    k.CreatedAt
                ));

                return Results.Ok(response);
            })
            .WithName("GetClientKeys")
            .WithSummary("Gets all client API-Keys.");
    }
}