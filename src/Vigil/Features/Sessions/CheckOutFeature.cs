using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.Sessions;

internal class CheckOutFeature : IEndpoint
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
        app.MapPost(RoutePrefix + "/check-out", async (
                HttpContext httpContext,
                [FromServices] SessionRepository repository,
                [FromServices] EventActionQueue eventQueue,
                [FromServices] ILogger<CheckOutFeature> logger,
                CancellationToken cancellationToken) =>
            {
                var client = httpContext.GetResolvedClientKey();

                logger.LogCheckOutRequest(client.ClientName);

                var checkOutResult = await repository.CheckOutAsync(
                    client,
                    cancellationToken);

                if (checkOutResult.IsSuccess)
                {
                    logger.LogCheckOutSucceeded(
                        client.ClientName,
                        checkOutResult.Value.Id);

                    eventQueue.Enqueue(new EventPayload(
                        VigilEventType.ClientCheckedOut,
                        checkOutResult.Value.ClientName,
                        checkOutResult.Value.ClientKeyId,
                        checkOutResult.Value.Id,
                        checkOutResult.Value.CheckedOutAt!.Value,
                        checkOutResult.Value.Metadata));

                    if (!repository.HasAnyOpenSession())
                    {
                        eventQueue.Enqueue(new EventPayload(
                            VigilEventType.AllClientsCheckedOut,
                            null,
                            null,
                            null,
                            DateTime.UtcNow));
                    }
                }
                else
                {
                    logger.LogCheckOutFailed(client.ClientName);
                }

                var responseResult = checkOutResult.Map(session => new Response(
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
            .RequireClientKey()
            .WithTags(Tag)
            .WithName("CheckOut")
            .WithSummary("Checks out a client.");
    }
}
