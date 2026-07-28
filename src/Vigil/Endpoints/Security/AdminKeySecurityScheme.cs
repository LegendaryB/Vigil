using Microsoft.AspNetCore.OpenApi;

namespace Vigil.Endpoints.Security;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class AdminKeyProtectedAttribute : Attribute;

internal static class AdminKeySecurityScheme
{
    internal const string SchemeId = "AdminKey";
    internal const string HeaderName = "Admin-Key";

    internal static OpenApiOptions AddAdminKeySecurityScheme(this OpenApiOptions options) =>
        options.AddApiKeySecurityScheme<AdminKeyProtectedAttribute>(
            SchemeId,
            HeaderName,
            "Admin key required for admin-protected endpoints.");

    internal static RouteHandlerBuilder RequireAdminKey(this RouteHandlerBuilder builder)
    {
        return builder
            .AddEndpointFilter<AdminKeyAuthFilter>()
            .WithMetadata(new AdminKeyProtectedAttribute());
    }
}
