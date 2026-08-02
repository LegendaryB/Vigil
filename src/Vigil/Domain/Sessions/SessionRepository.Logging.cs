namespace Vigil.Domain.Sessions;

internal static partial class SessionRepositoryLogging
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to check in client '{ClientName}': an open session already exists.")]
    internal static partial void LogClientAlreadyCheckedIn(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Checked in client '{ClientName}' with session ID '{SessionId}'.")]
    internal static partial void LogClientCheckedIn(
        this ILogger logger,
        string clientName,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to check out client '{ClientName}': no open session found.")]
    internal static partial void LogNoOpenSessionForCheckOut(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Checked out client '{ClientName}' with session ID '{SessionId}'.")]
    internal static partial void LogClientCheckedOut(
        this ILogger logger,
        string clientName,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to record heartbeat for client '{ClientName}': no open session found.")]
    internal static partial void LogNoOpenSessionForHeartbeat(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Recorded heartbeat for client '{ClientName}' with session ID '{SessionId}'.")]
    internal static partial void LogHeartbeatReceived(
        this ILogger logger,
        string clientName,
        Guid sessionId);
}
