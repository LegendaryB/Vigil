using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Errors.Security;

namespace Vigil.Endpoints.Security;

internal sealed class AdminKeyAuthFilter(
    IOptions<VigilOptions> options,
    ILogger<AdminKeyAuthFilter> logger) : IEndpointFilter
{
    private readonly string _configuredAdminKey = options.Value.AdminKey;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (ApiKeyHeaderAuth.TryGetSingleHeaderValue(context, AdminKeySecurityScheme.HeaderName, out var providedKey) &&
            ApiKeyHeaderAuth.KeysMatch(providedKey, _configuredAdminKey))
        {
            return await next(context);
        }

        logger.LogAdminKeyRejected(context.HttpContext.Request.Path);

        return SecurityErrorCatalog
            .AdminKeyInvalid()
            .ToProblemDetails();
    }
}
