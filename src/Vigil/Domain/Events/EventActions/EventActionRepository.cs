using Vigil.Configuration;

namespace Vigil.Domain.Events.EventActions;

internal sealed class EventActionRepository(VigilConfigurationProvider configurationProvider)
{
    public IReadOnlyList<EventAction> Get() => configurationProvider.Current.EventActions;
}
