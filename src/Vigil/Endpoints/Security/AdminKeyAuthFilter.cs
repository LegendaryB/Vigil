using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Errors.Security;

namespace Vigil.Endpoints.Security;

internal sealed class AdminKeyAuthFilter(IOptions<VigilOptions> options) : IEndpointFilter
{
    private const string HeaderName = "X-Admin-Key";

    private readonly string _configuredAdminKey = options.Value.AdminKey;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (!TryGetSingleHeaderValue(context, out var providedKey) ||
            !IsAdminKeyValid(providedKey, _configuredAdminKey))
        {
            return SecurityErrorCatalog.AdminKeyInvalid().ToProblemDetails();
        }

        return await next(context);
    }

    private static bool TryGetSingleHeaderValue(
        EndpointFilterInvocationContext context,
        out string value)
    {
        value = string.Empty;
        
        var headerValues = context.HttpContext.Request.Headers[HeaderName];

        if (headerValues.Count != 1 || string.IsNullOrEmpty(headerValues[0]))
            return false;

        value = headerValues[0]!;
        
        return true;
    }

    private static bool IsAdminKeyValid(string providedKey, string configuredKey)
    {
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedKey));
        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));

        return CryptographicOperations.FixedTimeEquals(providedHash, configuredHash);
    }
}
