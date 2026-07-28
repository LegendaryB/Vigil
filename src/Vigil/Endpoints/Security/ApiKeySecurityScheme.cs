using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Vigil.Endpoints.Security;

internal static class ApiKeySecurityScheme
{
    internal static OpenApiOptions AddApiKeySecurityScheme<TProtectedAttribute>(
        this OpenApiOptions options,
        string schemeId,
        string headerName,
        string description)
        where TProtectedAttribute : Attribute
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes[schemeId] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = headerName,
                Description = description
            };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, _) =>
        {
            var requiresKey = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<TProtectedAttribute>()
                .Any();

            if (!requiresKey)
                return Task.CompletedTask;

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeId, context.Document)] = []
            });

            return Task.CompletedTask;
        });

        return options;
    }
}
