using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Slices;

namespace Vigil.Features.Dashboard;

internal class SessionsDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.Sessions, (SessionRepository repository, bool showClosed = false) =>
            {
                var sessions = SessionsFilter.Apply(repository.Get(), showClosed).ToList();

                return Results.RazorSlice<Slices.Sessions.Index, SessionsIndexModel>(
                    new SessionsIndexModel(sessions, showClosed));
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
