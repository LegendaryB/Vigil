namespace Vigil.Configuration;

internal sealed class VigilConfigurationFile
{
    public List<ClientKeyConfiguration>? ClientKeys { get; set; }

    public List<EventActionConfiguration>? EventActions { get; set; }
}

internal sealed class ClientKeyConfiguration
{
    public string? Name { get; set; }

    public string? ApiKey { get; set; }

    public string? Group { get; set; }
}

internal sealed class EventActionConfiguration
{
    public string? Name { get; set; }

    public string? Event { get; set; }

    public string? Group { get; set; }

    public int Priority { get; set; } = 1;

    public WebhookConfiguration? Webhook { get; set; }

    public CommandConfiguration? Command { get; set; }
}

internal sealed class WebhookConfiguration
{
    public string? Url { get; set; }

    public string? Secret { get; set; }

    public Dictionary<string, string>? Headers { get; set; }
}

internal sealed class CommandConfiguration
{
    public string? Command { get; set; }

    public List<string>? Arguments { get; set; }

    public Dictionary<string, string>? Environment { get; set; }
}
