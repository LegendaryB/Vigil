namespace Vigil.Features.Sessions;

internal static partial class HeartbeatFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling heartbeat request for client '{ClientName}'.")]
    internal static partial void LogHeartbeatRequest(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully recorded heartbeat for client '{ClientName}' (Session ID: {SessionId}).")]
    internal static partial void LogHeartbeatSucceeded(
        this ILogger logger,
        string clientName,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to record heartbeat for client '{ClientName}'. Domain error occurred.")]
    internal static partial void LogHeartbeatFailed(
        this ILogger logger,
        string clientName);
}
