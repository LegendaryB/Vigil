using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.ClientKeys;

internal class DeleteClientKeyFeature : IEndpoint
{
    public static string RoutePrefix => Routes.ClientKeys;

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(RoutePrefix + "/{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] ClientKeyRepository repository,
                [FromServices] ILogger<DeleteClientKeyFeature> logger,
                CancellationToken cancellationToken) =>
            {
                logger.LogDeleteClientKeyRequest(id);

                var deleteResult = await repository.DeleteKeyAsync(id, cancellationToken);

                if (deleteResult.IsSuccess)
                {
                    logger.LogClientKeyDeletedSuccessfully(id);
                }
                else
                {
                    logger.LogClientKeyDeletionFailed(id);
                }

                return deleteResult.ToProblemDetails();
            })
            .RequireAdminKey()
            .WithName("DeleteClientKey")
            .WithSummary("Deletes an existing client key.");
    }
}