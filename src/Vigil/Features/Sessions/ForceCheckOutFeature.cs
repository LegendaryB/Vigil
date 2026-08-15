using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Events.Groups;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.Sessions;

internal class ForceCheckOutFeature : IEndpoint
{
    public static string RoutePrefix => Routes.Sessions;

    public static string Tag => Tags.Sessions;

    private record Response(
        Guid Id,
        Guid ClientKeyId,
        string ClientName,
        DateTime CheckedInAt,
        DateTime? CheckedOutAt,
        DateTime? LastSeenAt,
        IReadOnlyDictionary<string, string>? Metadata
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/{id:guid}/close", async (
                [FromRoute] Guid id,
                [FromServices] SessionRepository repository,
                [FromServices] ClientKeyRepository clientKeyRepository,
                [FromServices] EventActionQueue eventQueue,
                [FromServices] GroupCompletionTracker groupTracker,
                [FromServices] ILogger<ForceCheckOutFeature> logger,
                CancellationToken cancellationToken) =>
            {
                logger.LogForceCheckOutRequest(id);

                var forceCheckOutResult = await repository.ForceCheckOutAsync(
                    id,
                    cancellationToken);

                if (forceCheckOutResult.IsSuccess)
                {
                    logger.LogForceCheckOutSucceeded(
                        forceCheckOutResult.Value.ClientName,
                        forceCheckOutResult.Value.Id);

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

                    var client = clientKeyRepository.Get()
                        .FirstOrDefault(k => k.Id == forceCheckOutResult.Value.ClientKeyId);

                    if (client is not null &&
                        !string.IsNullOrWhiteSpace(client.Group) &&
                        groupTracker.RecordCheckOut(client.Group, client.Id))
                    {
                        eventQueue.Enqueue(new EventPayload(
                            VigilEventType.GroupCheckedOut,
                            null,
                            null,
                            null,
                            DateTime.UtcNow,
                            Metadata: null,
                            GroupName: client.Group));
                    }
                }
                else
                {
                    logger.LogForceCheckOutFailed(id);
                }

                var responseResult = forceCheckOutResult.Map(session => new Response(
                    session.Id,
                    session.ClientKeyId,
                    session.ClientName,
                    session.CheckedInAt,
                    session.CheckedOutAt,
                    session.LastSeenAt,
                    session.Metadata
                ));

                return responseResult.ToProblemDetails();
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("ForceCheckOutSession")
            .WithSummary("Forcibly closes an open session.");
    }
}
