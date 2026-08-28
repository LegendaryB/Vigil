using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Slices;
using Vigil.Slices.ClientKeys;

namespace Vigil.Features.Dashboard;

internal class CreateClientKeyDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.ClientKeys, async (
                HttpContext httpContext,
                ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                var form = await httpContext.Request.ReadFormAsync(cancellationToken);
                var clientName = form["clientName"].ToString();
                var group = form["group"].ToString();
                group = string.IsNullOrWhiteSpace(group) ? null : group.Trim();

                string? error = null;

                if (string.IsNullOrWhiteSpace(clientName))
                {
                    error = "Client name is required.";
                }
                else
                {
                    var createResult = await repository.CreateKeyAsync(clientName, group, cancellationToken);

                    if (!createResult.IsSuccess)
                        error = createResult.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Could not create client key.";
                }

                var allClientKeys = repository.Get();

                var groupFilter = GroupColumnFilterBuilder.Build(
                    allClientKeys.Select(k => k.Group),
                    "group-filter-popover",
                    "group-filter-list",
                    "group",
                    UiRoutes.ClientKeysTable,
                    $"#{DashboardStyles.ClientKeysTableBodyId}",
                    selected: null,
                    ClientKeysFilter.Ungrouped);

                var model = new ClientKeysIndexModel(allClientKeys.ToList(), error, groupFilter);

                return Results.RazorSlice<_Content, ClientKeysIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
