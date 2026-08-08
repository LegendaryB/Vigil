using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;

namespace Vigil.Features.Dashboard;

internal class DeleteClientKeyDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.ClientKeyDeleteTemplate, async (
                [FromRoute] Guid id,
                ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                await repository.DeleteKeyAsync(id, cancellationToken);

                return Results.Content(string.Empty, "text/html");
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
