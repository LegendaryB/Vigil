namespace Vigil.Features.EventActions;

internal static partial class UpdateEventActionFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to update event action '{EventActionId}'.")]
    internal static partial void LogUpdateEventActionRequest(
        this ILogger logger,
        Guid eventActionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully updated event action '{EventActionId}'.")]
    internal static partial void LogEventActionUpdatedSuccessfully(
        this ILogger logger,
        Guid eventActionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to update event action '{EventActionId}'. Domain error occurred.")]
    internal static partial void LogEventActionUpdateFailed(
        this ILogger logger,
        Guid eventActionId);
}
