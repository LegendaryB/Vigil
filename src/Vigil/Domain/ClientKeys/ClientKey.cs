namespace Vigil.Domain.ClientKeys;

public record ClientKey(
    Guid Id,
    string ClientName,
    string ApiKey,
    DateTime CreatedAt);