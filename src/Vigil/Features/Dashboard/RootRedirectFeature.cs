using Vigil.Endpoints;

namespace Vigil.Features.Dashboard;

internal class RootRedirectFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.Root, () => Results.Redirect(UiRoutes.Sessions))
            .ExcludeFromDescription();
    }
}
