using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices;
using EventActionsIndex = Vigil.Slices.EventActions.Index;

namespace Vigil.Features.Dashboard;

internal class EventActionsDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.EventActions, (EventActionRepository repository, ClientKeyRepository clientKeyRepository) =>
            {
                var knownGroups = clientKeyRepository.Get()
                    .Select(k => k.Group)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Select(g => g!)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                var model = new EventActionsIndexModel(repository.Get().ToList(), Error: null, knownGroups);

                return Results.RazorSlice<EventActionsIndex, EventActionsIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
