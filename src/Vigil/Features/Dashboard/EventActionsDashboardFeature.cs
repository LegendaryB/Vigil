using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices;
using EventActionsIndex = Vigil.Slices.EventActions.Index;

namespace Vigil.Features.Dashboard;

internal class EventActionsDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.EventActions, (EventActionRepository repository) =>
            {
                var model = new EventActionsIndexModel(repository.Get().ToList(), Error: null);

                return Results.RazorSlice<EventActionsIndex, EventActionsIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
