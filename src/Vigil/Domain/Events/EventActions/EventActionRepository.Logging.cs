using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

internal static partial class EventActionRepositoryLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created event action '{EventActionId}' for event '{Event}'.")]
    internal static partial void LogEventActionCreated(
        this ILogger logger,
        Guid eventActionId,
        VigilEventType @event);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to create event action: priority '{Priority}' is invalid.")]
    internal static partial void LogEventActionInvalidPriority(
        this ILogger logger,
        int priority);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to delete event action: '{EventActionId}' not found.")]
    internal static partial void LogEventActionNotFoundForDeletion(
        this ILogger logger,
        Guid eventActionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Deleted event action '{EventActionId}'.")]
    internal static partial void LogEventActionDeleted(
        this ILogger logger,
        Guid eventActionId);
}
