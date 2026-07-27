namespace Vigil.Features.ClientKeys;

internal static partial class GetClientKeysFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to retrieve all client keys.")]
    internal static partial void LogGetClientKeysRequest(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} client key(s).")]
    internal static partial void LogGetClientKeysSuccess(
        this ILogger logger,
        int count);
}