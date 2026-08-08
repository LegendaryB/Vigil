using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Vigil.Endpoints;

namespace Vigil.Features.Dashboard;

internal class LogoutFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.Logout, async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Results.Redirect(UiRoutes.Login);
            })
            .ExcludeFromDescription();
    }
}
