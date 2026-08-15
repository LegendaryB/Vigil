using Ardalis.Result;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.ClientKeys;

internal class CreateClientKeyFeature : IEndpoint
{
    public static string RoutePrefix => Routes.ClientKeys;

    public static string Tag => Tags.ClientKeys;

    internal record Request(string ClientName, string? Group = null);

    private record Response(
        Guid Id,
        string ClientName,
        string ApiKey,
        DateTime CreatedAt,
        DateTime? LastUsedAt,
        string? Group
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/", async (
                [FromBody] Request req,
                [FromServices] IValidator<Request> validator,
                [FromServices] ILogger<CreateClientKeyFeature> logger,
                ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                logger.LogCreateClientKeyRequest(req.ClientName);

                var validationResult = await validator.ValidateAsync(
                    req,
                    cancellationToken);
                
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    logger.LogValidationFailed(req.ClientName, errors);

                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
                
                var createKeyResult = await repository.CreateKeyAsync(
                    req.ClientName,
                    req.Group,
                    cancellationToken);

                if (createKeyResult.IsSuccess)
                {
                    logger.LogClientKeyCreatedSuccessfully(
                        req.ClientName, 
                        createKeyResult.Value.Id);
                }
                else
                {
                    logger.LogClientKeyCreationFailed(req.ClientName);
                }

                var responseResult = createKeyResult.Map(key => new Response(
                    key.Id,
                    key.ClientName,
                    key.ApiKey,
                    key.CreatedAt,
                    key.LastUsedAt,
                    key.Group
                ));

                return responseResult.ToProblemDetails();
            })
            .RequireAdminKey()
            .WithTags(Tag)
            .WithName("CreateClientKey")
            .WithSummary("Creates a new API-Key for a client.");
    }
}