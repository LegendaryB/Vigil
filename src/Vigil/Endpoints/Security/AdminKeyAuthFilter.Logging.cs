namespace Vigil.Endpoints.Security;

internal static partial class AdminKeyAuthFilterLogging
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Rejected request to '{Path}': missing or invalid admin key.")]
    internal static partial void LogAdminKeyRejected(
        this ILogger logger,
        string path);
}
