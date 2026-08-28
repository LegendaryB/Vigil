using Ardalis.Result;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;

namespace Vigil.Features.ClientKeys;

internal class UpdateClientKeyFeature : IEndpoint
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
        app.MapPut(RoutePrefix + "/{id:guid}", async (
                [FromRoute] Guid id,
                [FromBody] Request req,
                [FromServices] IValidator<Request> validator,
                [FromServices] ILogger<UpdateClientKeyFeature> logger,
                ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                logger.LogUpdateClientKeyRequest(id, req.ClientName);

                var validationResult = await validator.ValidateAsync(
                    req,
                    cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    logger.LogUpdateValidationFailed(id, errors);

                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var updateResult = await repository.UpdateKeyAsync(
                    id,
                    req.ClientName,
                    req.Group,
                    cancellationToken);

                if (updateResult.IsSuccess)
                {
                    logger.LogClientKeyUpdatedSuccessfully(id, req.ClientName);
                }
                else
                {
                    logger.LogClientKeyUpdateFailed(id);
                }

                var responseResult = updateResult.Map(key => new Response(
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
            .WithName("UpdateClientKey")
            .WithSummary("Updates an existing client key's name and group.");
    }
}
