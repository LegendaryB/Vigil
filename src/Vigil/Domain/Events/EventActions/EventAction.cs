using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

internal record EventAction(
    Guid Id,
    VigilEventType Event,
    EventActionTarget Target,
    string? Name,
    string? Description,
    int Priority,
    DateTime CreatedAt);
