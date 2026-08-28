namespace Vigil.Domain.Events;

internal static partial class ClientScheduleMonitorLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Client '{ClientName}' missed its expected check-in interval (Client key ID: {ClientKeyId}).")]
    internal static partial void LogClientMissedCheckIn(
        this ILogger logger,
        string clientName,
        Guid clientKeyId);
}
