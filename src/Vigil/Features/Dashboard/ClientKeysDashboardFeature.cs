using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Slices;
using ClientKeysIndex = Vigil.Slices.ClientKeys.Index;

namespace Vigil.Features.Dashboard;

internal class ClientKeysDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.ClientKeys, (ClientKeyRepository repository, string[]? group = null) =>
            {
                var allClientKeys = repository.Get();
                var clientKeys = ClientKeysFilter.Apply(allClientKeys, group).ToList();

                var groupFilter = GroupColumnFilterBuilder.Build(
                    allClientKeys.Select(k => k.Group),
                    "group-filter-popover",
                    "group-filter-list",
                    "group",
                    UiRoutes.ClientKeysTable,
                    $"#{DashboardStyles.ClientKeysTableBodyId}",
                    group,
                    ClientKeysFilter.Ungrouped);

                var model = new ClientKeysIndexModel(clientKeys, Error: null, groupFilter);

                return Results.RazorSlice<ClientKeysIndex, ClientKeysIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
