using Vigil.Domain.Sessions;

namespace Vigil.Features.Dashboard;

internal static class SessionsFilter
{
    internal const string OpenStatus = "open";
    internal const string ClosedStatus = "closed";

    internal static IEnumerable<Session> Apply(IEnumerable<Session> sessions, IReadOnlyCollection<string>? statuses)
    {
        if (statuses is null or { Count: 0 })
            statuses = [OpenStatus];

        var includeOpen = statuses.Contains(OpenStatus);
        var includeClosed = statuses.Contains(ClosedStatus);

        return sessions.Where(s =>
            (includeOpen && s.CheckedOutAt is null) ||
            (includeClosed && s.CheckedOutAt is not null));
    }
}
