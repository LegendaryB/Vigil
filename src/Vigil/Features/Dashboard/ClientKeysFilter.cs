using Vigil.Domain.ClientKeys;

namespace Vigil.Features.Dashboard;

internal static class ClientKeysFilter
{
    internal const string Ungrouped = "__ungrouped__";

    internal static IEnumerable<ClientKey> Apply(IEnumerable<ClientKey> clientKeys, IReadOnlyCollection<string>? groups)
    {
        if (groups is not { Count: > 0 })
            return clientKeys;

        return clientKeys.Where(k => groups.Contains(string.IsNullOrWhiteSpace(k.Group) ? Ungrouped : k.Group));
    }
}
