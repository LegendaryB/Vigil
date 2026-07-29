namespace Vigil.Features.EventActions;

internal static partial class GetEventActionsFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to retrieve all event actions.")]
    internal static partial void LogGetEventActionsRequest(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} event action(s).")]
    internal static partial void LogGetEventActionsSuccess(
        this ILogger logger,
        int count);
}
