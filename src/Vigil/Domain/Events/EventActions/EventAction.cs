using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

public record EventAction(
    Guid Id,
    string Name,
    VigilEventType Event,
    EventActionTarget Target,
    int Priority,
    DateTime CreatedAt,
    string? Group = null);
