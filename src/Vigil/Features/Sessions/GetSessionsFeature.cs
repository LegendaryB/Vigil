using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.Sessions;

internal class GetSessionsFeature : IEndpoint
{
    public static string RoutePrefix => Routes.Sessions;

    public static string Tag => Tags.Sessions;

    private record GetSessionResponse(
        Guid Id,
        Guid ClientKeyId,
        string ClientName,
        DateTime CheckedInAt,
        DateTime? CheckedOutAt,
        IReadOnlyDictionary<string, string>? Metadata
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(RoutePrefix + "/", (
                SessionRepository repository,
                [FromServices] ILogger<GetSessionsFeature> logger) =>
            {
                logger.LogGetSessionsRequest();

                var sessions = repository.Get();

                var response = sessions.Select(s => new GetSessionResponse(
                    s.Id,
                    s.ClientKeyId,
                    s.ClientName,
                    s.CheckedInAt,
                    s.CheckedOutAt,
                    s.Metadata
                )).ToList();

                logger.LogGetSessionsSuccess(response.Count);

                return Results.Ok(response);
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("GetSessions")
            .WithSummary("Gets all sessions.");
    }
}
