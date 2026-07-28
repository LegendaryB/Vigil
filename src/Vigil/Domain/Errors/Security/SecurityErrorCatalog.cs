using System.Text;

namespace Vigil.Domain.Errors.Security;

internal sealed class SecurityErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "security_";

    internal static string AdminKeyInvalidMessage() =>
        UseMessageTemplate(AdminKeyInvalidMessageTemplate);

    internal static string ClientKeyInvalidMessage() =>
        UseMessageTemplate(ClientKeyInvalidMessageTemplate);

    private static readonly CompositeFormat AdminKeyInvalidMessageTemplate =
        CompositeFormat.Parse("A valid admin key is required to perform this action.");

    private static readonly CompositeFormat ClientKeyInvalidMessageTemplate =
        CompositeFormat.Parse("A valid client key is required to perform this action.");
}
