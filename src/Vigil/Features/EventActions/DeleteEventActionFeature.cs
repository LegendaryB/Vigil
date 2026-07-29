using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.EventActions;

internal class DeleteEventActionFeature : IEndpoint
{
    public static string RoutePrefix => Routes.EventActions;

    public static string Tag => Tags.EventActions;

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(RoutePrefix + "/{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] EventActionRepository repository,
                [FromServices] ILogger<DeleteEventActionFeature> logger,
                CancellationToken cancellationToken) =>
            {
                logger.LogDeleteEventActionRequest(id);

                var deleteResult = await repository.DeleteAsync(id, cancellationToken);

                if (deleteResult.IsSuccess)
                    logger.LogEventActionDeletedSuccessfully(id);
                else
                    logger.LogEventActionDeletionFailed(id);

                return deleteResult.ToProblemDetails();
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("DeleteEventAction")
            .WithSummary("Deletes an existing event action.");
    }
}
