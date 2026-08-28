using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events.EventActions;

namespace Vigil.Domain.Events;

internal sealed class ClientScheduleMonitor(
    ClientKeyRepository clientKeyRepository,
    EventActionQueue queue,
    ILogger<ClientScheduleMonitor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    // Keyed by client key ID, valued by the reference time (LastUsedAt ?? CreatedAt) that was active
    // when we last notified for that key. Tracking the reference time itself - not just "notified: yes/no" -
    // means a re-notification correctly becomes eligible again as soon as the client checks in (moving the
    // reference time forward), regardless of whether any poll tick happens to land while the key looks
    // "fresh". A plain notified-or-not flag that only resets on an observed below-threshold poll would
    // silently stop firing forever once the poll interval is coarser than the configured check-in interval.
    private readonly Dictionary<Guid, DateTime> _notifiedForReferenceTime = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            CheckForMissedCheckIns();
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private void CheckForMissedCheckIns()
    {
        var candidates = clientKeyRepository.Get()
            .Where(k => k.ExpectedCheckInInterval is not null)
            .ToList();

        var candidateIds = candidates.Select(k => k.Id).ToHashSet();

        foreach (var staleId in _notifiedForReferenceTime.Keys.Where(id => !candidateIds.Contains(id)).ToList())
            _notifiedForReferenceTime.Remove(staleId);

        var now = DateTime.UtcNow;

        foreach (var key in candidates)
        {
            var referenceTime = key.LastUsedAt ?? key.CreatedAt;

            if (_notifiedForReferenceTime.TryGetValue(key.Id, out var notifiedForReferenceTime) &&
                notifiedForReferenceTime != referenceTime)
            {
                _notifiedForReferenceTime.Remove(key.Id);
            }

            if (now - referenceTime < key.ExpectedCheckInInterval!.Value)
                continue;

            if (!_notifiedForReferenceTime.TryAdd(key.Id, referenceTime))
                continue;

            logger.LogClientMissedCheckIn(key.ClientName, key.Id);

            queue.Enqueue(new EventPayload(
                VigilEventType.ClientMissedCheckIn,
                key.ClientName,
                key.Id,
                null,
                now));
        }
    }
}
