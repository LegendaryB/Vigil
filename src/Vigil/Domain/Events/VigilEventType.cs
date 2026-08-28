namespace Vigil.Domain.Events;

public enum VigilEventType
{
    /// <summary>A client checked in and opened a session.</summary>
    ClientCheckedIn,

    /// <summary>A client checked out and closed its session normally.</summary>
    ClientCheckedOut,

    /// <summary>The last open session was closed, leaving no clients checked in.</summary>
    AllClientsCheckedOut,

    /// <summary>A client's session has been open longer than the configured retention window.</summary>
    ClientOverdue,

    /// <summary>An open session was closed by an administrator rather than by the client itself.</summary>
    ClientForceCheckedOut,

    /// <summary>Every member of a client group has checked out.</summary>
    GroupCheckedOut,

    /// <summary>A client group's completion timeout elapsed before every member checked out.</summary>
    GroupCompletionTimedOut,

    /// <summary>A client's expected check-in interval elapsed without a new check-in.</summary>
    ClientMissedCheckIn
}

internal static class VigilEventTypeExtensions
{
    internal static bool IsGroupScoped(this VigilEventType eventType) =>
        eventType is VigilEventType.GroupCheckedOut or VigilEventType.GroupCompletionTimedOut;
}
