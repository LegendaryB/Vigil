using Vigil.Domain.ClientKeys;

namespace Vigil.Domain.Events.Groups;

internal sealed class GroupCompletionTracker(ClientKeyRepository clientKeyRepository)
{
    private readonly object _lock = new();
    private readonly Dictionary<string, GroupCycleState> _cycles = new(StringComparer.Ordinal);

    internal bool RecordCheckOut(string group, Guid clientKeyId)
    {
        lock (_lock)
        {
            if (!_cycles.TryGetValue(group, out var state))
            {
                state = new GroupCycleState(DateTime.UtcNow);
                _cycles[group] = state;
            }

            state.CompletedClientKeyIds.Add(clientKeyId);

            var memberIds = clientKeyRepository.Get()
                .Where(k => k.Group == group)
                .Select(k => k.Id)
                .ToHashSet();

            if (memberIds.Count == 0 || !state.CompletedClientKeyIds.IsSupersetOf(memberIds))
                return false;

            _cycles.Remove(group);
            return true;
        }
    }

    internal IReadOnlyList<(string Group, DateTime StartedAt)> GetActiveCycles()
    {
        lock (_lock)
        {
            return _cycles.Select(kv => (kv.Key, kv.Value.StartedAt)).ToList();
        }
    }

    internal bool TryCompleteForTimeout(string group)
    {
        lock (_lock)
        {
            return _cycles.Remove(group);
        }
    }

    private sealed class GroupCycleState(DateTime startedAt)
    {
        internal DateTime StartedAt { get; } = startedAt;
        internal HashSet<Guid> CompletedClientKeyIds { get; } = [];
    }
}
