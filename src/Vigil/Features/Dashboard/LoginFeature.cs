using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Endpoints;
using Vigil.Endpoints.Security;
using Vigil.Slices;

namespace Vigil.Features.Dashboard;

internal class LoginFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.Login, () =>
                Results.RazorSlice<Login, LoginPageModel>(new LoginPageModel(false)))
            .ExcludeFromDescription();

        app.MapPost(UiRoutes.Login, async (
                HttpContext httpContext,
                IOptions<VigilOptions> options,
                CancellationToken cancellationToken) =>
            {
                var form = await httpContext.Request.ReadFormAsync(cancellationToken);
                var providedKey = form["adminKey"].ToString();

                if (string.IsNullOrEmpty(providedKey) ||
                    !ApiKeyHeaderAuth.KeysMatch(providedKey, options.Value.AdminKey))
                {
                    return Results.RazorSlice<Login, LoginPageModel>(new LoginPageModel(true));
                }

                var claimsIdentity = new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, "admin")],
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return Results.Redirect(UiRoutes.Sessions);
            })
            .ExcludeFromDescription();
    }
}
