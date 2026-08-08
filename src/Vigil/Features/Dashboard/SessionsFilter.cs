using Vigil.Domain.Sessions;

namespace Vigil.Features.Dashboard;

internal static class SessionsFilter
{
    internal static readonly TimeSpan ClosedSessionRetention = TimeSpan.FromDays(7);

    internal static IEnumerable<Session> Apply(IEnumerable<Session> sessions, bool showClosed)
    {
        if (!showClosed)
            return sessions.Where(s => s.CheckedOutAt is null);

        var cutoff = DateTime.UtcNow - ClosedSessionRetention;

        return sessions.Where(s => s.CheckedOutAt is null || s.CheckedOutAt >= cutoff);
    }
}
