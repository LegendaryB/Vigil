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
                var expectedCheckInIntervalRaw = form["expectedCheckInInterval"].ToString();

                string? error = null;
                Guid? errorEntityId = null;

                if (string.IsNullOrWhiteSpace(clientName))
                {
                    error = "Client name is required.";
                    errorEntityId = id;
                }
                else if (!TryParseExpectedCheckInInterval(expectedCheckInIntervalRaw, out var expectedCheckInInterval))
                {
                    error = "Expected check-in interval must be a valid, positive time span (e.g. 01:00:00).";
                    errorEntityId = id;
                }
                else
                {
                    var updateResult = await repository.UpdateKeyAsync(id, clientName, group, expectedCheckInInterval, cancellationToken);

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

    private static bool TryParseExpectedCheckInInterval(string? raw, out TimeSpan? interval)
    {
        interval = null;

        if (string.IsNullOrWhiteSpace(raw))
            return true;

        if (!TimeSpan.TryParse(raw, out var parsed) || parsed <= TimeSpan.Zero)
            return false;

        interval = parsed;

        return true;
    }
}
