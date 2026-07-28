using Vigil.Domain.ClientKeys;

namespace Vigil.Endpoints.Security;

internal static class HttpContextClientKeyExtensions
{
    private const string ItemKey = "Vigil.ResolvedClientKey";

    extension(HttpContext context)
    {
        internal void SetResolvedClientKey(ClientKey clientKey) =>
            context.Items[ItemKey] = clientKey;

        internal ClientKey GetResolvedClientKey() =>
            (ClientKey)context.Items[ItemKey]!;
    }
}
