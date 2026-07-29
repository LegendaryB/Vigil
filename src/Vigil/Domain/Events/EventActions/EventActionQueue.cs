using System.Threading.Channels;
using Vigil.Domain.Events;

namespace Vigil.Domain.Events.EventActions;

internal sealed class EventActionQueue
{
    private readonly Channel<EventPayload> _channel = Channel.CreateUnbounded<EventPayload>();

    internal void Enqueue(EventPayload payload) => _channel.Writer.TryWrite(payload);

    internal IAsyncEnumerable<EventPayload> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
