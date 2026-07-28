namespace Vigil.Domain.Sessions;

public record Session(
    Guid Id,
    Guid ClientKeyId,
    string ClientName,
    DateTime CheckedInAt,
    DateTime? CheckedOutAt);
