using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Vigil.Endpoints.Security;

internal sealed class AdminKeyProtectedAttribute : Attribute;

internal static class AdminKeySecurityScheme
{
    internal const string SchemeId = "AdminKey";
    internal const string HeaderName = "X-Admin-Key";

    internal static OpenApiOptions AddAdminKeySecurityScheme(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = HeaderName,
                Description = "Admin key required for admin-protected endpoints."
            };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, _) =>
        {
            var requiresAdminKey = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<AdminKeyProtectedAttribute>()
                .Any();

            if (!requiresAdminKey)
                return Task.CompletedTask;
            
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeId, context.Document)] = []
            });

            return Task.CompletedTask;
        });

        return options;
    }

    internal static RouteHandlerBuilder RequireAdminKey(this RouteHandlerBuilder builder)
    {
        return builder
            .AddEndpointFilter<AdminKeyAuthFilter>()
            .WithMetadata(new AdminKeyProtectedAttribute());
    }
}
