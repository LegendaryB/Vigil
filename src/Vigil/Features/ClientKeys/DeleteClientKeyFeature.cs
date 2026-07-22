using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;

namespace Vigil.Features.ClientKeys;

internal class DeleteClientKeyFeature : IEndpoint
{
    public static string RoutePrefix => Routes.ClientKeys;

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(RoutePrefix + "/{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                var deleteResult = await repository.DeleteKeyAsync(id, cancellationToken);

                return deleteResult.ToProblemDetails();
            })
            .WithName("DeleteClientKey")
            .WithSummary("Deletes an existing client key.");
    }
}