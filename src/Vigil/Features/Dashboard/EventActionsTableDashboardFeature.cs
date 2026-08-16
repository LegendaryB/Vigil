using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices;
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

                return Results.RazorSlice<_TableBody, EventActionTableBodyModel>(
                    new EventActionTableBodyModel(eventActions, Error: null, ErrorEntityId: null));
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
