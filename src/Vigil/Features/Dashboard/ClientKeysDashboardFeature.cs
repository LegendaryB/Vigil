using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Slices;
using ClientKeysIndex = Vigil.Slices.ClientKeys.Index;

namespace Vigil.Features.Dashboard;

internal class ClientKeysDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.ClientKeys, (ClientKeyRepository repository) =>
            {
                var model = new ClientKeysIndexModel(repository.Get().ToList(), Error: null);

                return Results.RazorSlice<ClientKeysIndex, ClientKeysIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
