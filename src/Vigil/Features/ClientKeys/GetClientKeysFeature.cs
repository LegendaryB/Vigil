using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.ClientKeys;

internal class GetClientKeysFeature : IEndpoint
{
    public static string RoutePrefix => Routes.ClientKeys;

    public static string Tag => Tags.ClientKeys;

    private record GetClientKeyResponse(
        Guid Id,
        string ClientName,
        string ApiKey,
        DateTime CreatedAt,
        DateTime? LastUsedAt,
        string? Group
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(RoutePrefix + "/", (
                ClientKeyRepository repository,
                [FromServices] ILogger<GetClientKeysFeature> logger) =>
            {
                logger.LogGetClientKeysRequest();

                var keys = repository.Get();

                var response = keys.Select(k => new GetClientKeyResponse(
                    k.Id,
                    k.ClientName,
                    k.ApiKey,
                    k.CreatedAt,
                    k.LastUsedAt,
                    k.Group
                )).ToList();

                logger.LogGetClientKeysSuccess(response.Count);

                return Results.Ok(response);
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("GetClientKeys")
            .WithSummary("Gets all client API-Keys.");
    }
}