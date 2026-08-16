namespace Vigil.Configuration;

internal sealed class EventActionsOptions
{
    internal const string ConfigurationKey = "EventActions";
    
    public TimeSpan? CheckInTimeout { get; set; }

    public TimeSpan? GroupCompletionTimeout { get; set; }

    public TimeSpan? CommandTimeout { get; set; }

    public int DispatchLogCapacity { get; set; } = 1000;
}
