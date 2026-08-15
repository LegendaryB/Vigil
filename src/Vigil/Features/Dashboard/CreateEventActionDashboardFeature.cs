using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices;
using Vigil.Slices.EventActions;

namespace Vigil.Features.Dashboard;

internal class CreateEventActionDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(UiRoutes.EventActions, async (
                HttpContext httpContext,
                EventActionRepository repository,
                ClientKeyRepository clientKeyRepository,
                CancellationToken cancellationToken) =>
            {
                var form = await httpContext.Request.ReadFormAsync(cancellationToken);

                var error = TryBuildEventAction(form, out var eventType, out var target, out var priority, out var group);

                if (error is null)
                {
                    var createResult = await repository.CreateAsync(
                        eventType,
                        target!,
                        priority,
                        group,
                        cancellationToken);

                    if (!createResult.IsSuccess)
                        error = createResult.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Could not create event action.";
                }

                var knownGroups = clientKeyRepository.Get()
                    .Select(k => k.Group)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Select(g => g!)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                var typeFilter = new ColumnFilterModel(
                    "type-filter-popover",
                    "type-filter-list",
                    "type",
                    UiRoutes.EventActionsTable,
                    $"#{DashboardStyles.EventActionsTableBodyId}",
                    [
                        new ColumnFilterOption(EventActionsFilter.WebhookType, "Webhook", Checked: true),
                        new ColumnFilterOption(EventActionsFilter.CommandType, "Command", Checked: true)
                    ]);

                var eventFilter = new ColumnFilterModel(
                    "event-filter-popover",
                    "event-filter-list",
                    "event",
                    UiRoutes.EventActionsTable,
                    $"#{DashboardStyles.EventActionsTableBodyId}",
                    Enum.GetValues<VigilEventType>()
                        .Select(e => new ColumnFilterOption(e.ToString(), e.ToDisplayName(), Checked: true))
                        .ToList());

                var model = new EventActionsIndexModel(repository.Get().ToList(), error, knownGroups, typeFilter, eventFilter);

                return Results.RazorSlice<_Content, EventActionsIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }

    private static string? TryBuildEventAction(
        IFormCollection form,
        out VigilEventType eventType,
        out EventActionTarget? target,
        out int priority,
        out string? group)
    {
        target = null;

        if (!int.TryParse(form["priority"].ToString(), out priority))
            priority = 1;

        if (!Enum.TryParse(form["event"].ToString(), out eventType))
        {
            group = null;
            return "Invalid event type.";
        }

        if (eventType == VigilEventType.GroupCheckedOut)
        {
            group = NullIfEmpty(form["group"].ToString());

            if (group is null)
                return "Group is required for this event type.";
        }
        else
        {
            group = null;
        }

        var targetType = form["targetType"].ToString();

        if (targetType == "webhook")
        {
            var url = form["url"].ToString();

            if (string.IsNullOrWhiteSpace(url))
                return "Webhook URL is required.";

            target = new WebhookTarget(
                url,
                NullIfEmpty(form["secret"].ToString()),
                ParseKeyValueLines(form["headers"].ToString()));

            return null;
        }

        if (targetType == "command")
        {
            var command = form["command"].ToString();

            if (string.IsNullOrWhiteSpace(command))
                return "Command is required.";

            target = new CommandTarget(
                command,
                ParseLines(form["arguments"].ToString()),
                ParseKeyValueLines(form["environment"].ToString()));

            return null;
        }

        return "Invalid target type.";
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static IReadOnlyList<string> ParseLines(string? value) =>
        ParseLines(value, static line => line).ToList();

    private static IReadOnlyDictionary<string, string>? ParseKeyValueLines(string? value)
    {
        var pairs = ParseLines(value, line =>
        {
            var index = line.IndexOf('=');

            return index < 0
                ? default
                : (Key: line[..index].Trim(), Value: line[(index + 1)..].Trim());
        }).Where(pair => pair.Key is { Length: > 0 }).ToList();

        return pairs.Count == 0 ? null : pairs.ToDictionary(p => p.Key, p => p.Value);
    }

    private static IEnumerable<T> ParseLines<T>(string? value, Func<string, T> select)
    {
        if (string.IsNullOrWhiteSpace(value))
            yield break;

        foreach (var rawLine in value.Split('\n'))
        {
            var line = rawLine.Trim('\r', ' ');

            if (line.Length == 0)
                continue;

            yield return select(line);
        }
    }
}
