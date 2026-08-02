using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Errors;
using Vigil.Domain.Errors.Sessions;
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

    private const int MaxMetadataEntries = 20;
    private const int MaxMetadataKeyLength = 100;
    private const int MaxMetadataValueLength = 500;

    private record Request(IReadOnlyDictionary<string, string>? Metadata);

    private record Response(
        Guid Id,
        Guid ClientKeyId,
        string ClientName,
        DateTime CheckedInAt,
        DateTime? LastSeenAt,
        IReadOnlyDictionary<string, string>? Metadata
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/check-in", async (
                HttpContext httpContext,
                [FromBody] Request? req,
                [FromServices] SessionRepository repository,
                [FromServices] EventActionQueue eventQueue,
                [FromServices] ILogger<CheckInFeature> logger,
                CancellationToken cancellationToken) =>
            {
                var client = httpContext.GetResolvedClientKey();

                logger.LogCheckInRequest(client.ClientName);

                if (!TryValidateMetadata(req?.Metadata, out var validationReason))
                {
                    logger.LogCheckInFailed(client.ClientName);

                    return ErrorCatalog.Session
                        .InvalidMetadata(validationReason!)
                        .ToProblemDetails();
                }

                var checkInResult = await repository.CheckInAsync(
                    client,
                    req?.Metadata,
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
                        checkInResult.Value.CheckedInAt,
                        checkInResult.Value.Metadata));
                }
                else
                {
                    logger.LogCheckInFailed(client.ClientName);
                }

                var responseResult = checkInResult.Map(session => new Response(
                    session.Id,
                    session.ClientKeyId,
                    session.ClientName,
                    session.CheckedInAt,
                    session.LastSeenAt,
                    session.Metadata
                ));

                return responseResult.ToProblemDetails();
            })
            .RequireClientKey()
            .WithTags(Tag)
            .WithName("CheckIn")
            .WithSummary("Checks in a client.");
    }

    private static bool TryValidateMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        out string? reason)
    {
        reason = null;

        if (metadata is null)
            return true;

        if (metadata.Count > MaxMetadataEntries)
        {
            reason = $"at most {MaxMetadataEntries} entries are allowed.";
            
            return false;
        }

        foreach (var (key, value) in metadata)
        {
            if (key.Length > MaxMetadataKeyLength)
            {
                reason = $"key '{key}' exceeds {MaxMetadataKeyLength} characters.";
                
                return false;
            }

            if (value.Length <= MaxMetadataValueLength)
                continue;
            
            reason = $"value for key '{key}' exceeds {MaxMetadataValueLength} characters.";
            
            return false;
        }

        return true;
    }
}
