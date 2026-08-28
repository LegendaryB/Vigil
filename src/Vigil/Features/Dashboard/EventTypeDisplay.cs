using Vigil.Domain.Events;

namespace Vigil.Features.Dashboard;

internal static class EventTypeDisplay
{
    extension(VigilEventType eventType)
    {
        internal string ToDisplayName() => eventType switch
        {
            VigilEventType.ClientCheckedIn => "Client checked in",
            VigilEventType.ClientCheckedOut => "Client checked out",
            VigilEventType.AllClientsCheckedOut => "All clients checked out",
            VigilEventType.ClientOverdue => "Client overdue",
            VigilEventType.ClientForceCheckedOut => "Client force-checked out",
            VigilEventType.GroupCheckedOut => "Group checked out",
            VigilEventType.GroupCompletionTimedOut => "Group completion timed out",
            VigilEventType.ClientMissedCheckIn => "Client missed check-in",
            _ => eventType.ToString()
        };

        internal string ToDescription() => eventType switch
        {
            VigilEventType.ClientCheckedIn => "A client checked in and opened a session.",
            VigilEventType.ClientCheckedOut => "A client checked out and closed its session normally.",
            VigilEventType.AllClientsCheckedOut => "The last open session was closed, leaving no clients checked in.",
            VigilEventType.ClientOverdue => "A client's session has been open longer than the configured retention window.",
            VigilEventType.ClientForceCheckedOut => "An open session was closed by an administrator rather than by the client itself.",
            VigilEventType.GroupCheckedOut => "Every client key tagged with this group has checked out.",
            VigilEventType.GroupCompletionTimedOut => "This group's completion timeout elapsed before every member checked out.",
            VigilEventType.ClientMissedCheckIn => "A client's expected check-in interval elapsed without a new check-in.",
            _ => string.Empty
        };
    }
}
