using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Endpoints;
using Vigil.Slices;
using Vigil.Slices.ClientKeys;

namespace Vigil.Features.Dashboard;

internal class UpdateClientKeyDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.ClientKeyUpdateTemplate, async (
                [FromRoute] Guid id,
                HttpContext httpContext,
                ClientKeyRepository repository,
                CancellationToken cancellationToken) =>
            {
                var form = await httpContext.Request.ReadFormAsync(cancellationToken);
                var clientName = form["clientName"].ToString();
                var group = form["group"].ToString();
                group = string.IsNullOrWhiteSpace(group) ? null : group.Trim();

                string? error = null;
                Guid? errorEntityId = null;

                if (string.IsNullOrWhiteSpace(clientName))
                {
                    error = "Client name is required.";
                    errorEntityId = id;
                }
                else
                {
                    var updateResult = await repository.UpdateKeyAsync(id, clientName, group, cancellationToken);

                    if (!updateResult.IsSuccess)
                    {
                        error = updateResult.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Could not update client key.";
                        errorEntityId = id;
                    }
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

                var model = new ClientKeysIndexModel(allClientKeys.ToList(), error, groupFilter, errorEntityId);

                return Results.RazorSlice<_Content, ClientKeysIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
