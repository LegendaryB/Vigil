namespace Vigil.Features.ClientKeys;

internal static partial class DeleteClientKeyFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to delete client key with ID '{ClientKeyId}'.")]
    internal static partial void LogDeleteClientKeyRequest(
        this ILogger logger,
        Guid clientKeyId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully deleted client key with ID '{ClientKeyId}'.")]
    internal static partial void LogClientKeyDeletedSuccessfully(
        this ILogger logger,
        Guid clientKeyId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to delete client key with ID '{ClientKeyId}'. Domain error occurred.")]
    internal static partial void LogClientKeyDeletionFailed(
        this ILogger logger,
        Guid clientKeyId);
}