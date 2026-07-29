using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

internal static partial class EventActionDispatchServiceLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Dispatched webhook for event '{Event}' to '{WebhookUrl}'.")]
    internal static partial void LogWebhookDispatched(
        this ILogger logger,
        VigilEventType @event,
        string webhookUrl);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Webhook for event '{Event}' to '{WebhookUrl}' returned status {StatusCode}.")]
    internal static partial void LogWebhookDispatchFailed(
        this ILogger logger,
        VigilEventType @event,
        string webhookUrl,
        int statusCode);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to dispatch webhook for event '{Event}' to '{WebhookUrl}'.")]
    internal static partial void LogWebhookDispatchError(
        this ILogger logger,
        Exception exception,
        VigilEventType @event,
        string webhookUrl);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Dispatched command for event '{Event}': '{Command}'.")]
    internal static partial void LogCommandDispatched(
        this ILogger logger,
        VigilEventType @event,
        string command);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Command for event '{Event}' ('{Command}') exited with code {ExitCode}.")]
    internal static partial void LogCommandDispatchFailed(
        this ILogger logger,
        VigilEventType @event,
        string command,
        int exitCode);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to dispatch command for event '{Event}': '{Command}'.")]
    internal static partial void LogCommandDispatchError(
        this ILogger logger,
        Exception exception,
        VigilEventType @event,
        string command);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Standard error output from command for event '{Event}' ('{Command}'): {StandardError}")]
    internal static partial void LogCommandStandardError(
        this ILogger logger,
        VigilEventType @event,
        string command,
        string standardError);
}
