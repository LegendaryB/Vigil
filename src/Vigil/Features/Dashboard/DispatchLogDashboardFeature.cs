using Microsoft.AspNetCore.Mvc;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using Vigil.Endpoints;
using Vigil.Slices;
using DispatchLogIndex = Vigil.Slices.DispatchLog.Index;

namespace Vigil.Features.Dashboard;

internal class DispatchLogDashboardFeature : IUiEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(UiRoutes.DispatchLog, (
                DispatchLogRepository repository,
                string[]? type = null,
                [FromQuery(Name = "event")] string[]? events = null,
                string[]? outcome = null) =>
            {
                var entries = DispatchLogFilter.Apply(repository.Get(), type, events, outcome)
                    .OrderByDescending(e => e.DispatchedAt)
                    .ToList();

                var eventFilter = new ColumnFilterModel(
                    "event-filter-popover",
                    "event-filter-list",
                    "event",
                    UiRoutes.DispatchLogTable,
                    $"#{DashboardStyles.DispatchLogTableBodyId}",
                    Enum.GetValues<VigilEventType>()
                        .Select(e => new ColumnFilterOption(e.ToString(), e.ToDisplayName(), DispatchLogFilter.IsChecked(events, e.ToString())))
                        .ToList());

                var typeFilter = new ColumnFilterModel(
                    "type-filter-popover",
                    "type-filter-list",
                    "type",
                    UiRoutes.DispatchLogTable,
                    $"#{DashboardStyles.DispatchLogTableBodyId}",
                    [
                        new ColumnFilterOption(DispatchLogFilter.WebhookType, "Webhook", DispatchLogFilter.IsChecked(type, DispatchLogFilter.WebhookType)),
                        new ColumnFilterOption(DispatchLogFilter.CommandType, "Command", DispatchLogFilter.IsChecked(type, DispatchLogFilter.CommandType))
                    ]);

                var outcomeFilter = new ColumnFilterModel(
                    "outcome-filter-popover",
                    "outcome-filter-list",
                    "outcome",
                    UiRoutes.DispatchLogTable,
                    $"#{DashboardStyles.DispatchLogTableBodyId}",
                    [
                        new ColumnFilterOption(DispatchLogFilter.SucceededValue, "Succeeded", DispatchLogFilter.IsChecked(outcome, DispatchLogFilter.SucceededValue)),
                        new ColumnFilterOption(DispatchLogFilter.FailedValue, "Failed", DispatchLogFilter.IsChecked(outcome, DispatchLogFilter.FailedValue))
                    ]);

                var model = new DispatchLogIndexModel(entries, eventFilter, typeFilter, outcomeFilter);

                return Results.RazorSlice<DispatchLogIndex, DispatchLogIndexModel>(model);
            })
            .RequireAuthorization()
            .ExcludeFromDescription();
    }
}
