using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices.DispatchLog;

namespace Vigil.Features.Dashboard;

internal class DispatchLogTableDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.DispatchLogTable, (
                DispatchLogRepository repository,
                string[]? type = null,
                [FromQuery(Name = "event")] string[]? events = null,
                string[]? outcome = null) =>
            {
                var entries = DispatchLogFilter.Apply(repository.Get(), type, events, outcome)
                    .OrderByDescending(e => e.DispatchedAt)
                    .ToList();

                return Results.RazorSlice<_TableBody, IReadOnlyList<DispatchLogEntry>>(entries);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
