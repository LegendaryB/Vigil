using Vigil.Domain.ClientKeys;
using Vigil.Domain.Errors.Security;

namespace Vigil.Endpoints.Security;

internal sealed class ClientKeyAuthFilter(
    ClientKeyRepository repository,
    ILogger<ClientKeyAuthFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (ApiKeyHeaderAuth.TryGetSingleHeaderValue(
            context,
            ClientKeySecurityScheme.HeaderName,
            out var providedKey))
        {
            var matchedClient = repository.Get()
                .FirstOrDefault(key => ApiKeyHeaderAuth.KeysMatch(providedKey, key.ApiKey));

            if (matchedClient is not null)
            {
                context.HttpContext.SetResolvedClientKey(matchedClient);

                return await next(context);
            }
        }

        logger.LogClientKeyRejected(context.HttpContext.Request.Path);

        return SecurityErrorCatalog
            .ClientKeyInvalid()
            .ToProblemDetails();
    }
}
