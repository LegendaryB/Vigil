namespace Vigil.Features.DispatchLog;

internal static partial class GetDispatchLogFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to retrieve the dispatch log.")]
    internal static partial void LogGetDispatchLogRequest(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} dispatch log entries.")]
    internal static partial void LogGetDispatchLogSuccess(
        this ILogger logger,
        int count);
}
