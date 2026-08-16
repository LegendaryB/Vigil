using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Slices;
using Vigil.Slices.ClientKeys;

namespace Vigil.Features.Dashboard;

internal class ClientKeysTableDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.ClientKeysTable, (ClientKeyRepository repository, string[]? group = null) =>
            {
                var clientKeys = ClientKeysFilter.Apply(repository.Get(), group).ToList();

                return Results.RazorSlice<_TableBody, ClientKeyTableBodyModel>(
                    new ClientKeyTableBodyModel(clientKeys, Error: null, ErrorEntityId: null));
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
