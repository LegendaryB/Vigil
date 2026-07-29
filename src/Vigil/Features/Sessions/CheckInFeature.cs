using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.Sessions;

internal class CheckInFeature : IEndpoint
{
    public static string RoutePrefix => Routes.Sessions;

    public static string Tag => Tags.Sessions;

    private record Response(
        Guid Id,
        Guid ClientKeyId,
        string ClientName,
        DateTime CheckedInAt
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/check-in", async (
                HttpContext httpContext,
                [FromServices] SessionRepository repository,
                [FromServices] EventActionQueue eventQueue,
                [FromServices] ILogger<CheckInFeature> logger,
                CancellationToken cancellationToken) =>
            {
                var client = httpContext.GetResolvedClientKey();

                logger.LogCheckInRequest(client.ClientName);

                var checkInResult = await repository.CheckInAsync(
                    client,
                    cancellationToken);

                if (checkInResult.IsSuccess)
                {
                    logger.LogCheckInSucceeded(
                        client.ClientName,
                        checkInResult.Value.Id);

                    eventQueue.Enqueue(new EventPayload(
                        VigilEventType.ClientCheckedIn,
                        checkInResult.Value.ClientName,
                        checkInResult.Value.ClientKeyId,
                        checkInResult.Value.Id,
                        checkInResult.Value.CheckedInAt));
                }
                else
                {
                    logger.LogCheckInFailed(client.ClientName);
                }

                var responseResult = checkInResult.Map(session => new Response(
                    session.Id,
                    session.ClientKeyId,
                    session.ClientName,
                    session.CheckedInAt
                ));

                return responseResult.ToProblemDetails();
            })
            .RequireClientKey()
            .WithTags(Tag)
            .WithName("CheckIn")
            .WithSummary("Checks in a client.");
    }
}
