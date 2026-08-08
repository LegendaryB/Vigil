using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Slices.Sessions;

namespace Vigil.Features.Dashboard;

internal class CloseSessionDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.SessionCloseTemplate, async (
                [FromRoute] Guid id,
                SessionRepository repository,
                EventActionQueue eventQueue,
                CancellationToken cancellationToken) =>
            {
                var forceCheckOutResult = await repository.ForceCheckOutAsync(id, cancellationToken);

                if (!forceCheckOutResult.IsSuccess)
                    return Results.Content(string.Empty, "text/html", statusCode: StatusCodes.Status400BadRequest);

                eventQueue.Enqueue(new EventPayload(
                    VigilEventType.ClientForceCheckedOut,
                    forceCheckOutResult.Value.ClientName,
                    forceCheckOutResult.Value.ClientKeyId,
                    forceCheckOutResult.Value.Id,
                    forceCheckOutResult.Value.CheckedOutAt!.Value,
                    forceCheckOutResult.Value.Metadata));

                if (!repository.HasAnyOpenSession())
                {
                    eventQueue.Enqueue(new EventPayload(
                        VigilEventType.AllClientsCheckedOut,
                        null,
                        null,
                        null,
                        DateTime.UtcNow));
                }

                return Results.RazorSlice<_Row, Session>(forceCheckOutResult.Value);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
