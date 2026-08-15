using Vigil.Domain.Events.EventActions;

namespace Vigil.Domain.Errors.EventActions;

internal sealed class EventActionErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "event_action_";

    internal string InvalidPriority => Prefix + "invalid_priority";

    internal static string EventActionNotFoundMessage(Guid id) =>
        EntityNotFoundMessage(nameof(EventAction), id);

    internal const string InvalidPriorityMessage = "Priority must be at least 1.";
}
