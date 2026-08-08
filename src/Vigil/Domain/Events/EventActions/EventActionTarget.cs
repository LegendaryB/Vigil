using System.Text.Json.Serialization;

namespace Vigil.Domain.Events.EventActions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(WebhookTarget), WebhookTarget.Discriminator)]
[JsonDerivedType(typeof(CommandTarget), CommandTarget.Discriminator)]
public abstract record EventActionTarget;

public sealed record WebhookTarget(
    string Url,
    string? Secret = null,
    IReadOnlyDictionary<string, string>? Headers = null) : EventActionTarget
{
    internal const string Discriminator = "webhook";
}

public sealed record CommandTarget(
    string Command,
    IReadOnlyList<string> Arguments,
    IReadOnlyDictionary<string, string>? Environment = null) : EventActionTarget
{
    internal const string Discriminator = "command";
}
