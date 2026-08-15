using Ardalis.Result;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Data;
using Vigil.Domain.Errors;
using Vigil.Domain.Errors.EventActions;
using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

internal sealed class EventActionRepository : JsonFileRepository<EventAction>
{
    protected override Func<EventAction, Guid> PrimaryKeySelector => eventAction => eventAction.Id;

    public EventActionRepository(
        ILogger<EventActionRepository> logger,
        IOptions<VigilOptions> options)
        : base(logger, options.Value.EventActionsFilePath)
    {
    }

    public async Task<Result<EventAction>> CreateAsync(
        VigilEventType @event,
        EventActionTarget target,
        int priority,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            if (priority < 1)
            {
                Logger.LogEventActionInvalidPriority(priority);
                return ErrorCatalog.EventAction.InvalidPriority();
            }

            var eventAction = new EventAction(
                Guid.NewGuid(),
                @event,
                target,
                priority,
                DateTime.UtcNow
            );

            Entities[eventAction.Id] = eventAction;

            return Result.Success(eventAction);
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        Logger.LogEventActionCreated(result.Value.Id, result.Value.Event);

        await PersistAsync(cancellationToken);

        return result;
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            if (id == Guid.Empty || !Entities.TryRemove(id, out _))
            {
                Logger.LogEventActionNotFoundForDeletion(id);
                return ErrorCatalog.EventAction.EventActionNotFound(id);
            }

            Logger.LogEventActionDeleted(id);

            return Result.Success();
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        await PersistAsync(cancellationToken);

        return result;
    }
}
