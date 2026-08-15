using Vigil.Domain.Events.EventActions;

namespace Vigil.Features.Dashboard;

internal static class EventActionsFilter
{
    internal const string WebhookType = "webhook";
    internal const string CommandType = "command";

    internal static IEnumerable<EventAction> Apply(
        IEnumerable<EventAction> eventActions,
        IReadOnlyCollection<string>? types,
        IReadOnlyCollection<string>? events)
    {
        if (types is { Count: > 0 })
        {
            eventActions = eventActions.Where(a => a.Target switch
            {
                WebhookTarget => types.Contains(WebhookType),
                CommandTarget => types.Contains(CommandType),
                _ => true
            });
        }

        if (events is { Count: > 0 })
            eventActions = eventActions.Where(a => events.Contains(a.Event.ToString()));

        return eventActions;
    }

    internal static bool IsChecked(IReadOnlyCollection<string>? selected, string value) =>
        selected is null || selected.Count == 0 || selected.Contains(value);
}
