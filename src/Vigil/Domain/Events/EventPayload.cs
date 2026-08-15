namespace Vigil.Domain.Events;

internal record EventPayload(
    VigilEventType Event,
    string? ClientName,
    Guid? ClientKeyId,
    Guid? SessionId,
    DateTime OccurredAt,
    IReadOnlyDictionary<string, string>? Metadata = null,
    string? GroupName = null);
