using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

public record DispatchLogEntry(
    Guid Id,
    Guid EventActionId,
    VigilEventType Event,
    string? Group,
    string TargetType,
    string Destination,
    DateTime DispatchedAt,
    bool Succeeded,
    int? StatusCode,
    int? ExitCode,
    string? ErrorMessage);
