namespace Vigil.Domain.ClientKeys;

internal static partial class ClientKeyRepositoryLogging
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to create client key: Client key with name '{ClientName}' already exists and must be unique.")]
    internal static partial void LogClientNameAlreadyExists(
        this ILogger logger,
        string clientName);
    
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created new client key '{ClientName}' with ID '{ClientKeyId}'.")]
    internal static partial void LogClientKeyCreated(
        this ILogger logger,
        string clientName,
        Guid clientKeyId);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to delete client key: Key with ID '{ClientKeyId}' not found.")]
    internal static partial void LogClientKeyNotFoundForDeletion(
        this ILogger logger,
        Guid clientKeyId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Deleted client key '{ClientName}' with ID '{ClientKeyId}'.")]
    internal static partial void LogClientKeyDeleted(
        this ILogger logger,
        string clientName,
        Guid clientKeyId);
    
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "No existing client keys file found at '{FilePath}'. Starting with an empty repository.")]
    internal static partial void LogFileNotFoundStartingEmpty(
        this ILogger logger,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Loaded {Count} client key(s) from '{FilePath}'.")]
    internal static partial void LogKeysLoadedFromFile(
        this ILogger logger,
        int count,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Client keys file at '{FilePath}' was empty or returned null.")]
    internal static partial void LogFileContentEmpty(
        this ILogger logger,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Critical error loading client keys from file '{FilePath}'.")]
    internal static partial void LogErrorLoadingFromFile(
        this ILogger logger,
        Exception exception,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Successfully persisted {Count} client key(s) to '{FilePath}'.")]
    internal static partial void LogKeysPersisted(
        this ILogger logger,
        int count,
        string filePath);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to persist client keys to file '{FilePath}'.")]
    internal static partial void LogErrorSavingToFile(
        this ILogger logger,
        Exception exception,
        string filePath);
}