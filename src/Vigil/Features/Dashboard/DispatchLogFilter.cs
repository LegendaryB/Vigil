using Vigil.Domain.Events.EventActions;

namespace Vigil.Features.Dashboard;

internal static class DispatchLogFilter
{
    internal const string WebhookType = "webhook";
    internal const string CommandType = "command";
    internal const string SucceededValue = "succeeded";
    internal const string FailedValue = "failed";

    internal static IEnumerable<DispatchLogEntry> Apply(
        IEnumerable<DispatchLogEntry> entries,
        IReadOnlyCollection<string>? types,
        IReadOnlyCollection<string>? events,
        IReadOnlyCollection<string>? outcomes)
    {
        if (types is { Count: > 0 })
            entries = entries.Where(e => types.Contains(e.TargetType));

        if (events is { Count: > 0 })
            entries = entries.Where(e => events.Contains(e.Event.ToString()));

        if (outcomes is { Count: > 0 })
        {
            entries = entries.Where(e =>
                (outcomes.Contains(SucceededValue) && e.Succeeded) ||
                (outcomes.Contains(FailedValue) && !e.Succeeded));
        }

        return entries;
    }

    internal static bool IsChecked(IReadOnlyCollection<string>? selected, string value) =>
        selected is null || selected.Count == 0 || selected.Contains(value);
}
