using Microsoft.AspNetCore.OpenApi;

namespace Vigil.Endpoints.Security;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class ClientKeyProtectedAttribute : Attribute;

internal static class ClientKeySecurityScheme
{
    internal const string SchemeId = "ClientKey";
    internal const string HeaderName = "Client-Key";

    internal static OpenApiOptions AddClientKeySecurityScheme(this OpenApiOptions options) =>
        options.AddApiKeySecurityScheme<ClientKeyProtectedAttribute>(
            SchemeId,
            HeaderName,
            "Client key required for client-protected endpoints.");

    internal static RouteHandlerBuilder RequireClientKey(this RouteHandlerBuilder builder)
    {
        return builder
            .AddEndpointFilter<ClientKeyAuthFilter>()
            .WithMetadata(new ClientKeyProtectedAttribute());
    }
}
