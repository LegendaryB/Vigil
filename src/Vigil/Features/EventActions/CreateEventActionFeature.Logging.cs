using Vigil.Domain.Events;

namespace Vigil.Features.EventActions;

internal static partial class CreateEventActionFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to create an event action for event '{Event}'.")]
    internal static partial void LogCreateEventActionRequest(
        this ILogger logger,
        VigilEventType @event);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully created event action for event '{Event}' (ID: {EventActionId}).")]
    internal static partial void LogEventActionCreatedSuccessfully(
        this ILogger logger,
        VigilEventType @event,
        Guid eventActionId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to create event action for event '{Event}'. Domain error occurred.")]
    internal static partial void LogEventActionCreationFailed(
        this ILogger logger,
        VigilEventType @event);
}
