namespace Vigil.Features.EventActions;

internal static partial class DeleteEventActionFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to delete event action with ID '{EventActionId}'.")]
    internal static partial void LogDeleteEventActionRequest(
        this ILogger logger,
        Guid eventActionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully deleted event action with ID '{EventActionId}'.")]
    internal static partial void LogEventActionDeletedSuccessfully(
        this ILogger logger,
        Guid eventActionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to delete event action with ID '{EventActionId}'. Domain error occurred.")]
    internal static partial void LogEventActionDeletionFailed(
        this ILogger logger,
        Guid eventActionId);
}
