using System.Text.Json.Serialization;

namespace Vigil.Domain.Events.EventActions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(WebhookTarget), WebhookTarget.Discriminator)]
[JsonDerivedType(typeof(CommandTarget), CommandTarget.Discriminator)]
internal abstract record EventActionTarget;

internal sealed record WebhookTarget(
    string Url,
    string? Secret = null,
    IReadOnlyDictionary<string, string>? Headers = null) : EventActionTarget
{
    internal const string Discriminator = "webhook";
}

internal sealed record CommandTarget(string Command, IReadOnlyList<string> Arguments) : EventActionTarget
{
    internal const string Discriminator = "command";
}
