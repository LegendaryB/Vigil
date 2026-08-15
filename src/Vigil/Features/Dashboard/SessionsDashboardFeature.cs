using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Slices;

namespace Vigil.Features.Dashboard;

internal class SessionsDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.Sessions, (SessionRepository repository, string[]? status = null) =>
            {
                var sessions = SessionsFilter.Apply(repository.Get(), status).ToList();

                var statuses = status is { Length: > 0 } ? status : [SessionsFilter.OpenStatus];

                var statusFilter = new ColumnFilterModel(
                    "status-filter-popover",
                    "status-filter-list",
                    "status",
                    UiRoutes.SessionsTable,
                    $"#{DashboardStyles.SessionsTableBodyId}",
                    [
                        new ColumnFilterOption(SessionsFilter.OpenStatus, "Open", statuses.Contains(SessionsFilter.OpenStatus)),
                        new ColumnFilterOption(SessionsFilter.ClosedStatus, "Closed", statuses.Contains(SessionsFilter.ClosedStatus))
                    ]);

                return Results.RazorSlice<Slices.Sessions.Index, SessionsIndexModel>(
                    new SessionsIndexModel(sessions, statusFilter));
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
