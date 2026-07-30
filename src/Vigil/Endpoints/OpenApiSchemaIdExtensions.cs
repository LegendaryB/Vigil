using Microsoft.AspNetCore.OpenApi;

namespace Vigil.Endpoints;

internal static class OpenApiSchemaIdExtensions
{
    internal static OpenApiOptions AddUniqueNestedTypeSchemaIds(this OpenApiOptions options)
    {
        options.CreateSchemaReferenceId = jsonTypeInfo =>
        {
            var type = jsonTypeInfo.Type;

            return type.DeclaringType is not null
                ? $"{type.DeclaringType.Name}{type.Name}"
                : OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        };

        return options;
    }
}
