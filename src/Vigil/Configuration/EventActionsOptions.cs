namespace Vigil.Configuration;

internal sealed class EventActionsOptions
{
    internal const string ConfigurationKey = "EventActions";
    
    public TimeSpan? CheckInTimeout { get; set; }
}
