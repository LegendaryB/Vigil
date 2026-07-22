using Ardalis.Result;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;

namespace Vigil.Features.ClientKeys;

internal class CreateClientKeyFeature : IEndpoint
{
    public static string RoutePrefix => Routes.ClientKeys;
    
    internal record Request(string ClientName);

    private record Response(
        Guid Id,
        string ClientName,
        string ApiKey,
        DateTime CreatedAt
    );

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePrefix + "/", async (
                [FromBody] Request req,
                [FromServices] IValidator<Request> validator,
                ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                var validationResult = await validator.ValidateAsync(
                    req,
                    cancellationToken);
                
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());
                
                var createKeyResult = await repository.CreateKeyAsync(
                    req.ClientName,
                    cancellationToken);

                var responseResult = createKeyResult.Map(key => new Response(
                    key.Id,
                    key.ClientName,
                    key.ApiKey,
                    key.CreatedAt
                ));

                return responseResult.ToProblemDetails();
            })
            .WithName("CreateClientKey")
            .WithSummary("Creates a new API-Key for a client.");
    }
}