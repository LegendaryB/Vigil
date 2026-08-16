namespace Vigil.Features.ClientKeys;

internal static partial class UpdateClientKeyFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to update client key '{ClientKeyId}' to name '{ClientName}'.")]
    internal static partial void LogUpdateClientKeyRequest(
        this ILogger logger,
        Guid clientKeyId,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Client key update failed validation for '{ClientKeyId}'. Validation errors: {ValidationErrors}")]
    internal static partial void LogUpdateValidationFailed(
        this ILogger logger,
        Guid clientKeyId,
        string validationErrors);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully updated client key '{ClientKeyId}' to name '{ClientName}'.")]
    internal static partial void LogClientKeyUpdatedSuccessfully(
        this ILogger logger,
        Guid clientKeyId,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to update client key '{ClientKeyId}'. Domain error occurred.")]
    internal static partial void LogClientKeyUpdateFailed(
        this ILogger logger,
        Guid clientKeyId);
}
