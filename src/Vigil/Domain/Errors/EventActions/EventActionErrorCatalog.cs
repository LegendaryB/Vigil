using Vigil.Domain.Events.EventActions;

namespace Vigil.Domain.Errors.EventActions;

internal sealed class EventActionErrorCatalog : DomainErrorCatalog
{
    protected override string Prefix => "event_action_";

    internal static string EventActionNotFoundMessage(Guid id) =>
        EntityNotFoundMessage(nameof(EventAction), id);
}
