namespace Vigil.Domain.Events;

internal static partial class SessionOverdueMonitorLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Client '{ClientName}' is overdue for check-out (Session ID: {SessionId}).")]
    internal static partial void LogSessionOverdue(
        this ILogger logger,
        string clientName,
        Guid sessionId);
}
