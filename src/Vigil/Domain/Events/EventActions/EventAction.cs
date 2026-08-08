using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

public record EventAction(
    Guid Id,
    VigilEventType Event,
    EventActionTarget Target,
    string? Name,
    string? Description,
    int Priority,
    DateTime CreatedAt);
