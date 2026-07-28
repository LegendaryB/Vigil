namespace Vigil.Features.Sessions;

internal static partial class CheckInFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling check-in request for client '{ClientName}'.")]
    internal static partial void LogCheckInRequest(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully checked in client '{ClientName}' (Session ID: {SessionId}).")]
    internal static partial void LogCheckInSucceeded(
        this ILogger logger,
        string clientName,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to check in client '{ClientName}'. Domain error occurred.")]
    internal static partial void LogCheckInFailed(
        this ILogger logger,
        string clientName);
}
