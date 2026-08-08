using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;

namespace Vigil.Features.Dashboard;

internal class DeleteEventActionDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.EventActionDeleteTemplate, async (
                [FromRoute] Guid id,
                EventActionRepository repository,
                CancellationToken cancellationToken) =>
            {
                await repository.DeleteAsync(id, cancellationToken);

                return Results.Content(string.Empty, "text/html");
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
