namespace Vigil.Features.ClientKeys;

internal static partial class CreateClientKeyFeatureLogging
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Handling request to create client key for client '{ClientName}'.")]
    internal static partial void LogCreateClientKeyRequest(
        this ILogger logger,
        string clientName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Client key creation failed validation for client '{ClientName}'. Validation errors: {ValidationErrors}")]
    internal static partial void LogValidationFailed(
        this ILogger logger,
        string clientName,
        string validationErrors);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Successfully created client key for '{ClientName}' (ID: {ClientKeyId}).")]
    internal static partial void LogClientKeyCreatedSuccessfully(
        this ILogger logger,
        string clientName,
        Guid clientKeyId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to create client key for '{ClientName}'. Domain error occurred.")]
    internal static partial void LogClientKeyCreationFailed(
        this ILogger logger,
        string clientName);
}