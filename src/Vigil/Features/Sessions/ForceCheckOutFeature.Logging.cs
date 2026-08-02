namespace Vigil.Features.Sessions;

internal static partial class ForceCheckOutFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling force-close request for session '{SessionId}'.")]
    internal static partial void LogForceCheckOutRequest(
        this ILogger logger,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully force-closed session for client '{ClientName}' (Session ID: {SessionId}).")]
    internal static partial void LogForceCheckOutSucceeded(
        this ILogger logger,
        string clientName,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to force-close session '{SessionId}'. Domain error occurred.")]
    internal static partial void LogForceCheckOutFailed(
        this ILogger logger,
        Guid sessionId);
}
