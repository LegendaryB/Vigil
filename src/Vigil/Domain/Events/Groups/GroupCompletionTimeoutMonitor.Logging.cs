namespace Vigil.Domain.Events.Groups;

internal static partial class GroupCompletionTimeoutMonitorLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Group '{Group}' completion timed out; firing GroupCheckedOut anyway.")]
    internal static partial void LogGroupCompletionTimedOut(
        this ILogger logger,
        string group);
}
