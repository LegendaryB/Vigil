using Ardalis.Result;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.Sessions;

internal class HeartbeatFeature : IEndpoint
{
    public static string RoutePrefix => Routes.Sessions;

    public static string Tag => Tags.Sessions;

    private record Response(
        Guid Id,
        Guid ClientKeyId,
        string ClientName,
        DateTime CheckedInAt,
        DateTime? LastSeenAt
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/heartbeat", async (
                HttpContext httpContext,
                SessionRepository repository,
                ILogger<HeartbeatFeature> logger,
                CancellationToken cancellationToken) =>
            {
                var client = httpContext.GetResolvedClientKey();

                logger.LogHeartbeatRequest(client.ClientName);

                var heartbeatResult = await repository.HeartbeatAsync(
                    client,
                    cancellationToken);

                if (heartbeatResult.IsSuccess)
                {
                    logger.LogHeartbeatSucceeded(
                        client.ClientName,
                        heartbeatResult.Value.Id);
                }
                else
                {
                    logger.LogHeartbeatFailed(client.ClientName);
                }

                var responseResult = heartbeatResult.Map(session => new Response(
                    session.Id,
                    session.ClientKeyId,
                    session.ClientName,
                    session.CheckedInAt,
                    session.LastSeenAt
                ));

                return responseResult.ToProblemDetails();
            })
            .RequireClientKey()
            .WithTags(Tag)
            .WithName("Heartbeat")
            .WithSummary("Pushes back the overdue deadline for the calling client's open session.");
    }
}
