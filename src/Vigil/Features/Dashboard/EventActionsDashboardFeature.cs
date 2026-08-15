using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices;
using EventActionsIndex = Vigil.Slices.EventActions.Index;

namespace Vigil.Features.Dashboard;

internal class EventActionsDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.EventActions, (
                EventActionRepository repository,
                ClientKeyRepository clientKeyRepository,
                string[]? type = null,
                [FromQuery(Name = "event")] string[]? events = null,
                string[]? group = null) =>
            {
                var knownGroups = clientKeyRepository.Get()
                    .Select(k => k.Group)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Select(g => g!)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                var allEventActions = repository.Get();
                var eventActions = EventActionsFilter.Apply(allEventActions, type, events, group).ToList();

                var typeFilter = new ColumnFilterModel(
                    "type-filter-popover",
                    "type-filter-list",
                    "type",
                    UiRoutes.EventActionsTable,
                    $"#{DashboardStyles.EventActionsTableBodyId}",
                    [
                        new ColumnFilterOption(EventActionsFilter.WebhookType, "Webhook", EventActionsFilter.IsChecked(type, EventActionsFilter.WebhookType)),
                        new ColumnFilterOption(EventActionsFilter.CommandType, "Command", EventActionsFilter.IsChecked(type, EventActionsFilter.CommandType))
                    ]);

                var eventFilter = new ColumnFilterModel(
                    "event-filter-popover",
                    "event-filter-list",
                    "event",
                    UiRoutes.EventActionsTable,
                    $"#{DashboardStyles.EventActionsTableBodyId}",
                    Enum.GetValues<VigilEventType>()
                        .Select(e => new ColumnFilterOption(e.ToString(), e.ToDisplayName(), EventActionsFilter.IsChecked(events, e.ToString())))
                        .ToList());

                var groupFilter = GroupColumnFilterBuilder.Build(
                    allEventActions.Select(a => a.Group),
                    "group-filter-popover",
                    "group-filter-list",
                    "group",
                    UiRoutes.EventActionsTable,
                    $"#{DashboardStyles.EventActionsTableBodyId}",
                    group,
                    EventActionsFilter.Ungrouped);

                var model = new EventActionsIndexModel(eventActions, Error: null, knownGroups, typeFilter, eventFilter, groupFilter);

                return Results.RazorSlice<EventActionsIndex, EventActionsIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
