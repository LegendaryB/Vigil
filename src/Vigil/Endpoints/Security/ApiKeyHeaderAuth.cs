using System.Security.Cryptography;
using System.Text;

namespace Vigil.Endpoints.Security;

internal static class ApiKeyHeaderAuth
{
    internal static bool TryGetSingleHeaderValue(
        EndpointFilterInvocationContext context,
        string headerName,
        out string value)
    {
        value = string.Empty;

        var headerValues = context.HttpContext.Request.Headers[headerName];

        if (headerValues.Count != 1 || string.IsNullOrEmpty(headerValues[0]))
            return false;

        value = headerValues[0]!;

        return true;
    }

    internal static bool KeysMatch(string providedKey, string expectedKey)
    {
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedKey));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expectedKey));

        return CryptographicOperations.FixedTimeEquals(providedHash, expectedHash);
    }
}
