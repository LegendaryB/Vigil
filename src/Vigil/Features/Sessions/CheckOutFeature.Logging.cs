namespace Vigil.Features.Sessions;

internal static partial class CheckOutFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling check-out request for client '{ClientName}'.")]
    internal static partial void LogCheckOutRequest(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully checked out client '{ClientName}' (Session ID: {SessionId}).")]
    internal static partial void LogCheckOutSucceeded(
        this ILogger logger,
        string clientName,
        Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to check out client '{ClientName}'. Domain error occurred.")]
    internal static partial void LogCheckOutFailed(
        this ILogger logger,
        string clientName);
}
