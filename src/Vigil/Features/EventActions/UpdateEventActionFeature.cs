using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.EventActions;

internal class UpdateEventActionFeature : IEndpoint
{
    public static string RoutePrefix => Routes.EventActions;

    public static string Tag => Tags.EventActions;

    private record Request
    {
        public required EventActionTarget Target { get; init; }
        public int Priority { get; init; }
        public string? Group { get; init; }
    }

    private record Response
    {
        public required Guid Id { get; init; }
        public required VigilEventType Event { get; init; }
        public required EventActionTarget Target { get; init; }
        public required int Priority { get; init; }
        public required DateTime CreatedAt { get; init; }
        public string? Group { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(RoutePrefix + "/{id:guid}", async (
                [FromRoute] Guid id,
                [FromBody] Request req,
                [FromServices] ILogger<UpdateEventActionFeature> logger,
                EventActionRepository repository,
                CancellationToken cancellationToken) =>
            {
                logger.LogUpdateEventActionRequest(id);

                var updateResult = await repository.UpdateAsync(
                    id,
                    req.Target,
                    req.Priority,
                    req.Group,
                    cancellationToken);

                if (updateResult.IsSuccess)
                {
                    logger.LogEventActionUpdatedSuccessfully(id);
                }
                else
                {
                    logger.LogEventActionUpdateFailed(id);
                }

                var responseResult = updateResult.Map(eventAction => new Response
                {
                    Id = eventAction.Id,
                    Event = eventAction.Event,
                    Target = eventAction.Target,
                    Priority = eventAction.Priority,
                    CreatedAt = eventAction.CreatedAt,
                    Group = eventAction.Group
                });

                return responseResult.ToProblemDetails();
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("UpdateEventAction")
            .WithSummary("Updates an existing event action's target, priority, and group. The event type and target type cannot change.");
    }
}
