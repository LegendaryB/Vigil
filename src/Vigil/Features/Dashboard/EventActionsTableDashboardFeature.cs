using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices.EventActions;

namespace Vigil.Features.Dashboard;

internal class EventActionsTableDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.EventActionsTable, (
                EventActionRepository repository,
                string[]? type = null,
                [FromQuery(Name = "event")] string[]? events = null,
                string[]? group = null) =>
            {
                var eventActions = EventActionsFilter.Apply(repository.Get(), type, events, group).ToList();

                return Results.RazorSlice<_TableBody, IReadOnlyList<EventAction>>(eventActions);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
