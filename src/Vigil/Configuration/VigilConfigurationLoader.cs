using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events;
using Vigil.Domain.Events.EventActions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Vigil.Configuration;

internal sealed record VigilConfiguration(
    IReadOnlyList<ClientKey> ClientKeys,
    IReadOnlyList<EventAction> EventActions)
{
    internal static VigilConfiguration Empty { get; } = new([], []);
}

internal sealed record VigilConfigurationLoadResult(
    VigilConfiguration? Configuration,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

internal static partial class VigilConfigurationLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    [GeneratedRegex(@"\$\{([A-Za-z_][A-Za-z0-9_]*)\}")]
    private static partial Regex PlaceholderPattern();

    internal static VigilConfigurationLoadResult Load(
        string yaml,
        Func<string, string?> getEnvironmentVariable,
        DateTime now)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        VigilConfigurationFile file;

        try
        {
            file = Deserializer.Deserialize<VigilConfigurationFile?>(yaml) ?? new VigilConfigurationFile();
        }
        catch (YamlException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            errors.Add($"Line {ex.Start.Line}, column {ex.Start.Column}: {innerMessage}");
            return new VigilConfigurationLoadResult(null, errors, warnings);
        }

        string? Expand(string? value, string location)
        {
            if (value is null)
                return null;

            return PlaceholderPattern().Replace(value, match =>
            {
                var variable = getEnvironmentVariable(match.Groups[1].Value);

                if (variable is not null)
                    return variable;

                errors.Add($"{location}: environment variable '{match.Groups[1].Value}' is not set.");
                return match.Value;
            });
        }

        var clientKeys = LoadClientKeys(file.ClientKeys ?? [], Expand, errors, now);
        var eventActions = LoadEventActions(file.EventActions ?? [], Expand, errors, now);

        var knownGroups = clientKeys
            .Select(k => k.Group)
            .Where(g => g is not null)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var eventAction in eventActions.Where(a => a.Group is not null && !knownGroups.Contains(a.Group)))
        {
            warnings.Add(
                $"Event action '{eventAction.Name}': group '{eventAction.Group}' matches no client key, so it can never fire.");
        }

        return errors.Count > 0
            ? new VigilConfigurationLoadResult(null, errors, warnings)
            : new VigilConfigurationLoadResult(new VigilConfiguration(clientKeys, eventActions), errors, warnings);
    }

    private static List<ClientKey> LoadClientKeys(
        List<ClientKeyConfiguration> entries,
        Func<string?, string, string?> expand,
        List<string> errors,
        DateTime now)
    {
        var result = new List<ClientKey>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var apiKeys = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            var name = entry.Name?.Trim();
            var location = string.IsNullOrEmpty(name) ? $"clientKeys[{index}]" : $"Client key '{name}'";

            if (string.IsNullOrEmpty(name))
            {
                errors.Add($"{location}: 'name' is required.");
                continue;
            }

            if (!names.Add(name))
                errors.Add($"{location}: name is used more than once (names are compared case-insensitively).");

            var apiKey = expand(entry.ApiKey, location);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                errors.Add($"{location}: 'apiKey' is required.");
                continue;
            }

            if (!apiKeys.Add(apiKey))
                errors.Add($"{location}: 'apiKey' is identical to another client key's apiKey.");

            result.Add(new ClientKey(
                DeterministicId("client-key", name),
                name,
                apiKey,
                now,
                LastUsedAt: null,
                Group: NullIfBlank(entry.Group)));
        }

        return result;
    }

    private static List<EventAction> LoadEventActions(
        List<EventActionConfiguration> entries,
        Func<string?, string, string?> expand,
        List<string> errors,
        DateTime now)
    {
        var result = new List<EventAction>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];
            var name = entry.Name?.Trim();
            var location = string.IsNullOrEmpty(name) ? $"eventActions[{index}]" : $"Event action '{name}'";

            if (string.IsNullOrEmpty(name))
            {
                errors.Add($"{location}: 'name' is required.");
                continue;
            }

            if (!names.Add(name))
                errors.Add($"{location}: name is used more than once (names are compared case-insensitively).");

            var errorCountBefore = errors.Count;

            if (!Enum.TryParse<VigilEventType>(entry.Event, ignoreCase: true, out var eventType)
                || !Enum.IsDefined(eventType))
            {
                errors.Add(
                    $"{location}: 'event' must be one of {string.Join(", ", Enum.GetNames<VigilEventType>())}.");
                continue;
            }

            if (entry.Priority < 1)
                errors.Add($"{location}: 'priority' must be at least 1.");

            var group = NullIfBlank(entry.Group);

            if (eventType.IsGroupScoped() && group is null)
                errors.Add($"{location}: 'group' is required for event {eventType}.");

            if (!eventType.IsGroupScoped() && group is not null)
                errors.Add($"{location}: 'group' must not be set for event {eventType}.");

            if ((entry.Webhook is null) == (entry.Command is null))
                errors.Add($"{location}: exactly one of 'webhook' or 'command' is required.");

            EventActionTarget? target = entry.Webhook is not null
                ? BuildWebhookTarget(entry.Webhook, location, expand, errors)
                : entry.Command is not null
                    ? BuildCommandTarget(entry.Command, location, expand, errors)
                    : null;

            if (target is null || errors.Count > errorCountBefore)
                continue;

            result.Add(new EventAction(
                DeterministicId("event-action", name),
                name,
                eventType,
                target,
                entry.Priority,
                now,
                group));
        }

        return result;
    }

    private static WebhookTarget? BuildWebhookTarget(
        WebhookConfiguration webhook,
        string location,
        Func<string?, string, string?> expand,
        List<string> errors)
    {
        var url = expand(webhook.Url, location);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            errors.Add($"{location}: 'webhook.url' must be an absolute http or https URL.");
            return null;
        }

        return new WebhookTarget(
            url!,
            NullIfBlank(expand(webhook.Secret, location)),
            ExpandValues(webhook.Headers, location, expand));
    }

    private static CommandTarget? BuildCommandTarget(
        CommandConfiguration command,
        string location,
        Func<string?, string, string?> expand,
        List<string> errors)
    {
        var executable = expand(command.Command, location);

        if (string.IsNullOrWhiteSpace(executable))
        {
            errors.Add($"{location}: 'command.command' is required.");
            return null;
        }

        var arguments = (command.Arguments ?? [])
            .Select(argument => expand(argument, location) ?? string.Empty)
            .ToList();

        return new CommandTarget(
            executable,
            arguments,
            ExpandValues(command.Environment, location, expand));
    }

    private static Dictionary<string, string>? ExpandValues(
        Dictionary<string, string>? values,
        string location,
        Func<string?, string, string?> expand) =>
        values?.ToDictionary(pair => pair.Key, pair => expand(pair.Value, location) ?? string.Empty);

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid DeterministicId(string kind, string name)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"vigil:{kind}:{name.ToLowerInvariant()}"));
        return new Guid(hash.AsSpan(0, 16));
    }
}
