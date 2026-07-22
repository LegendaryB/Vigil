using Vigil.Domain.ClientKeys;

namespace Vigil.Domain.Errors.ClientKeys;

internal sealed class ClientKeyErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "client_key_";
    
    internal static string ClientNameMustBeUniqueMessage() =>
        PropertyValueMustBeUniqueMessage(nameof(ClientKey.ClientName));

    internal static string ClientKeyNotFoundMessage(Guid id) =>
        EntityNotFoundMessage(
            nameof(ClientKey),
            id);
}