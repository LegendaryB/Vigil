using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Slices;

namespace Vigil.Features.Dashboard;

internal class SessionsDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.Sessions, (
                SessionRepository repository,
                IOptionsMonitor<EventActionsOptions> options,
                string[]? status = null) =>
            {
                var allSessions = repository.Get();

                var sessions = SessionsFilter.Apply(allSessions, status).ToList();

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

                var today = DateTime.UtcNow.Date;
                var openSessions = allSessions.Where(s => s.CheckedOutAt is null).ToList();
                var timeout = options.CurrentValue.CheckInTimeout;

                var stats = new SessionStats(
                    openSessions.Count,
                    allSessions.Count(s => s.CheckedOutAt is { } checkedOutAt && checkedOutAt.Date == today),
                    timeout is null ? 0 : openSessions.Count(s => SessionsFilter.IsOverdue(s, timeout.Value)));

                return Results.RazorSlice<Slices.Sessions.Index, SessionsIndexModel>(
                    new SessionsIndexModel(sessions, statusFilter, stats));
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
