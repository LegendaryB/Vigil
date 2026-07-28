using System.Text;

namespace Vigil.Domain.Errors.Sessions;

internal sealed class SessionErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "session_";

    internal string AlreadyCheckedInCode => Prefix + "already_checked_in";
    internal string NoOpenSessionCode => Prefix + "no_open_session";

    internal static string AlreadyCheckedInMessage(string clientName) =>
        UseMessageTemplate(AlreadyCheckedInMessageTemplate, clientName);

    internal static string NoOpenSessionMessage(string clientName) =>
        UseMessageTemplate(NoOpenSessionMessageTemplate, clientName);

    private static readonly CompositeFormat AlreadyCheckedInMessageTemplate =
        CompositeFormat.Parse("Client '{0}' already has an open session. Check out before checking in again.");

    private static readonly CompositeFormat NoOpenSessionMessageTemplate =
        CompositeFormat.Parse("Client '{0}' has no open session to check out.");
}
