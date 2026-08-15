using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.EventActions;

internal class GetEventActionsFeature : IEndpoint
{
    public static string RoutePrefix => Routes.EventActions;

    public static string Tag => Tags.EventActions;

    private record GetEventActionResponse
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
        app.MapGet(RoutePrefix + "/", (
                EventActionRepository repository,
                [FromServices] ILogger<GetEventActionsFeature> logger) =>
            {
                logger.LogGetEventActionsRequest();

                var response = repository.Get().Select(a => new GetEventActionResponse
                {
                    Id = a.Id,
                    Event = a.Event,
                    Target = a.Target,
                    Priority = a.Priority,
                    CreatedAt = a.CreatedAt,
                    Group = a.Group
                }).ToList();

                logger.LogGetEventActionsSuccess(response.Count);

                return Results.Ok(response);
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("GetEventActions")
            .WithSummary("Gets all event actions.");
    }
}
