using Vigil.Domain.Sessions;
using Vigil.Endpoints;
using Vigil.Slices.Sessions;

namespace Vigil.Features.Dashboard;

internal class SessionsTableDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.SessionsTable, (SessionRepository repository, string[]? status = null) =>
            {
                var sessions = SessionsFilter.Apply(repository.Get(), status).ToList();

                return Results.RazorSlice<_TableBody, IReadOnlyList<Session>>(sessions);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
