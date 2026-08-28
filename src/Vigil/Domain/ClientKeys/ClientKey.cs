namespace Vigil.Domain.ClientKeys;

public record ClientKey(
    Guid Id,
    string ClientName,
    string ApiKey,
    DateTime CreatedAt,
    DateTime? LastUsedAt = null,
    string? Group = null);