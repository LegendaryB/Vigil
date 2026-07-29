using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;

namespace Vigil.Domain.Events;

internal sealed class SessionOverdueMonitor(
    SessionRepository sessionRepository,
    EventActionQueue queue,
    IOptionsMonitor<EventActionsOptions> options,
    ILogger<SessionOverdueMonitor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    private readonly HashSet<Guid> _notifiedSessionIds = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            CheckForOverdueSessions();
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private void CheckForOverdueSessions()
    {
        var timeout = options.CurrentValue.CheckInTimeout;

        if (timeout is null)
            return;

        var openSessions = sessionRepository.Get()
            .Where(s => s.CheckedOutAt is null)
            .ToList();

        _notifiedSessionIds.IntersectWith(openSessions.Select(s => s.Id));

        var now = DateTime.UtcNow;

        foreach (var session in openSessions)
        {
            if (now - session.CheckedInAt < timeout)
                continue;

            if (!_notifiedSessionIds.Add(session.Id))
                continue;

            logger.LogSessionOverdue(session.ClientName, session.Id);

            queue.Enqueue(new EventPayload(
                VigilEventType.ClientOverdue,
                session.ClientName,
                session.ClientKeyId,
                session.Id,
                now));
        }
    }
}
