namespace Vigil.Features.Sessions;

internal static partial class GetSessionsFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to retrieve all sessions.")]
    internal static partial void LogGetSessionsRequest(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} session(s).")]
    internal static partial void LogGetSessionsSuccess(
        this ILogger logger,
        int count);
}
