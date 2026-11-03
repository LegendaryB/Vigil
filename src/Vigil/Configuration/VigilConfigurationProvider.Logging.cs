namespace Vigil.Configuration;

internal static partial class VigilConfigurationProviderLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Loaded {ClientKeyCount} client key(s) and {EventActionCount} event action(s) from '{FilePath}'.")]
    internal static partial void LogConfigurationLoaded(
        this ILogger logger,
        int clientKeyCount,
        int eventActionCount,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Configuration file '{FilePath}' not found. Starting with no client keys and no event actions.")]
    internal static partial void LogConfigurationFileMissing(
        this ILogger logger,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Configuration file '{FilePath}' was removed. Keeping the previously loaded configuration.")]
    internal static partial void LogConfigurationFileRemoved(
        this ILogger logger,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Configuration directory for '{FilePath}' does not exist. Changes to the file will not be picked up until restart.")]
    internal static partial void LogConfigurationReloadUnavailable(
        this ILogger logger,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Configuration error: {Error}")]
    internal static partial void LogConfigurationError(
        this ILogger logger,
        string error);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Configuration warning: {Warning}")]
    internal static partial void LogConfigurationWarning(
        this ILogger logger,
        string warning);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Configuration file '{FilePath}' is invalid. Keeping the previously loaded configuration.")]
    internal static partial void LogConfigurationReloadRejected(
        this ILogger logger,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to reload configuration file '{FilePath}'. Keeping the previously loaded configuration.")]
    internal static partial void LogConfigurationReloadFailed(
        this ILogger logger,
        Exception exception,
        string filePath);
}
