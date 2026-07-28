namespace Vigil.Endpoints.Security;

internal static partial class ClientKeyAuthFilterLogging
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Rejected request to '{Path}': missing or invalid client key.")]
    internal static partial void LogClientKeyRejected(
        this ILogger logger,
        string path);
}
