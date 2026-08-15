using Vigil.Domain.Events.EventActions;

namespace Vigil.Domain.Errors.EventActions;

internal sealed class EventActionErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "event_action_";

    internal string InvalidPriority => Prefix + "invalid_priority";

    internal string GroupRequired => Prefix + "group_required";

    internal string GroupNotAllowed => Prefix + "group_not_allowed";

    internal static string EventActionNotFoundMessage(Guid id) =>
        EntityNotFoundMessage(nameof(EventAction), id);

    internal const string InvalidPriorityMessage = "Priority must be at least 1.";

    internal const string GroupRequiredMessage = "Group is required when Event is GroupCheckedOut or GroupCompletionTimedOut.";

    internal const string GroupNotAllowedMessage = "Group must not be set unless Event is GroupCheckedOut or GroupCompletionTimedOut.";
}
