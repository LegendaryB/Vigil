using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Events.EventActions;

namespace Vigil.Domain.Events.Groups;

internal sealed class GroupCompletionTimeoutMonitor(
    GroupCompletionTracker tracker,
    EventActionQueue queue,
    IOptionsMonitor<EventActionsOptions> options,
    ILogger<GroupCompletionTimeoutMonitor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            CheckForTimedOutGroups();
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private void CheckForTimedOutGroups()
    {
        var timeout = options.CurrentValue.GroupCompletionTimeout;

        if (timeout is null)
            return;

        var now = DateTime.UtcNow;

        foreach (var (group, startedAt) in tracker.GetActiveCycles())
        {
            if (now - startedAt < timeout)
                continue;

            if (!tracker.TryCompleteForTimeout(group))
                continue;

            logger.LogGroupCompletionTimedOut(group);

            queue.Enqueue(new EventPayload(
                VigilEventType.GroupCheckedOut,
                null,
                null,
                null,
                now,
                Metadata: null,
                GroupName: group));
        }
    }
}
