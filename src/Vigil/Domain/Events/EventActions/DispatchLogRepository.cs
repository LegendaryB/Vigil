using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Data;

namespace Vigil.Domain.Events.EventActions;

internal sealed class DispatchLogRepository : JsonFileRepository<DispatchLogEntry>
{
    private readonly IOptions<EventActionsOptions> _eventActionsOptions;

    protected override Func<DispatchLogEntry, Guid> PrimaryKeySelector => entry => entry.Id;

    public DispatchLogRepository(
        ILogger<DispatchLogRepository> logger,
        IOptions<VigilOptions> options,
        IOptions<EventActionsOptions> eventActionsOptions)
        : base(logger, options.Value.DispatchLogFilePath)
    {
        _eventActionsOptions = eventActionsOptions;
    }

    internal async Task RecordAsync(DispatchLogEntry entry, CancellationToken cancellationToken)
    {
        await MutateAsync(() =>
        {
            Entities[entry.Id] = entry;

            var capacity = _eventActionsOptions.Value.DispatchLogCapacity;
            var excess = Entities.Count - capacity;

            if (excess <= 0)
                return true;

            var oldest = Entities.Values
                .OrderBy(e => e.DispatchedAt)
                .Take(excess);

            foreach (var stale in oldest)
                Entities.TryRemove(stale.Id, out _);

            return true;
        }, cancellationToken);

        await PersistAsync(cancellationToken);
    }
}
