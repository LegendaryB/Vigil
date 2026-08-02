using System.Text;
using Vigil.Domain.Sessions;

namespace Vigil.Domain.Errors.Sessions;

internal sealed class SessionErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "session_";

    internal string AlreadyCheckedInCode => Prefix + "already_checked_in";
    internal string NoOpenSessionCode => Prefix + "no_open_session";
    internal string InvalidMetadataCode => Prefix + "invalid_metadata";
    internal string AlreadyClosedCode => Prefix + "already_closed";

    internal static string AlreadyCheckedInMessage(string clientName) =>
        UseMessageTemplate(AlreadyCheckedInMessageTemplate, clientName);

    internal static string NoOpenSessionMessage(string clientName) =>
        UseMessageTemplate(NoOpenSessionMessageTemplate, clientName);

    internal static string InvalidMetadataMessage(string reason) =>
        UseMessageTemplate(InvalidMetadataMessageTemplate, reason);

    internal static string SessionNotFoundMessage(Guid id) =>
        EntityNotFoundMessage(nameof(Session), id);

    internal static string AlreadyClosedMessage(Guid id) =>
        UseMessageTemplate(AlreadyClosedMessageTemplate, id);

    private static readonly CompositeFormat AlreadyCheckedInMessageTemplate =
        CompositeFormat.Parse("Client '{0}' already has an open session. Check out before checking in again.");

    private static readonly CompositeFormat NoOpenSessionMessageTemplate =
        CompositeFormat.Parse("Client '{0}' has no open session to check out.");

    private static readonly CompositeFormat InvalidMetadataMessageTemplate =
        CompositeFormat.Parse("Invalid session metadata: {0}");

    private static readonly CompositeFormat AlreadyClosedMessageTemplate =
        CompositeFormat.Parse("Session '{0}' is already closed.");
}
