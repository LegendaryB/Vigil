using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.DispatchLog;

internal class GetDispatchLogFeature : IEndpoint
{
    public static string RoutePrefix => Routes.DispatchLog;

    public static string Tag => Tags.DispatchLog;

    private record GetDispatchLogResponse
    {
        public required Guid Id { get; init; }
        public required Guid EventActionId { get; init; }
        public required VigilEventType Event { get; init; }
        public string? Group { get; init; }
        public required string TargetType { get; init; }
        public required string Destination { get; init; }
        public required DateTime DispatchedAt { get; init; }
        public required bool Succeeded { get; init; }
        public int? StatusCode { get; init; }
        public int? ExitCode { get; init; }
        public string? ErrorMessage { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(RoutePrefix + "/", (
                DispatchLogRepository repository,
                [FromServices] ILogger<GetDispatchLogFeature> logger) =>
            {
                logger.LogGetDispatchLogRequest();

                var response = repository.Get()
                    .OrderByDescending(e => e.DispatchedAt)
                    .Select(e => new GetDispatchLogResponse
                    {
                        Id = e.Id,
                        EventActionId = e.EventActionId,
                        Event = e.Event,
                        Group = e.Group,
                        TargetType = e.TargetType,
                        Destination = e.Destination,
                        DispatchedAt = e.DispatchedAt,
                        Succeeded = e.Succeeded,
                        StatusCode = e.StatusCode,
                        ExitCode = e.ExitCode,
                        ErrorMessage = e.ErrorMessage
                    }).ToList();

                logger.LogGetDispatchLogSuccess(response.Count);

                return Results.Ok(response);
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("GetDispatchLog")
            .WithSummary("Gets recent event-action dispatch attempts, newest first.");
    }
}
