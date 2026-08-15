using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.EventActions;

internal class CreateEventActionFeature : IEndpoint
{
    public static string RoutePrefix => Routes.EventActions;

    public static string Tag => Tags.EventActions;

    private record Request
    {
        public required VigilEventType Event { get; init; }
        public required EventActionTarget Target { get; init; }
        public int Priority { get; init; }
    }

    private record Response
    {
        public required Guid Id { get; init; }
        public required VigilEventType Event { get; init; }
        public required EventActionTarget Target { get; init; }
        public required int Priority { get; init; }
        public required DateTime CreatedAt { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/", async (
                [FromBody] Request req,
                [FromServices] ILogger<CreateEventActionFeature> logger,
                EventActionRepository repository,
                CancellationToken cancellationToken) =>
            {
                logger.LogCreateEventActionRequest(req.Event);

                var createResult = await repository.CreateAsync(
                    req.Event,
                    req.Target,
                    req.Priority,
                    cancellationToken);

                if (createResult.IsSuccess)
                {
                    logger.LogEventActionCreatedSuccessfully(
                        req.Event,
                        createResult.Value.Id);
                }
                else
                {
                    logger.LogEventActionCreationFailed(req.Event);
                }

                var responseResult = createResult.Map(eventAction => new Response
                {
                    Id = eventAction.Id,
                    Event = eventAction.Event,
                    Target = eventAction.Target,
                    Priority = eventAction.Priority,
                    CreatedAt = eventAction.CreatedAt
                });

                return responseResult.ToProblemDetails();
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("CreateEventAction")
            .WithSummary("Creates a new event action.");
    }
}
