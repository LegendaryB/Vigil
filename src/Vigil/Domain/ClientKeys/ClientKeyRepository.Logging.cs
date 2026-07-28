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
}
