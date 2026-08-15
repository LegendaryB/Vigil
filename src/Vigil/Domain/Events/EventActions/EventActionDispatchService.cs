using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Vigil.Domain.Events.EventActions;

internal sealed class EventActionDispatchService(
    EventActionQueue queue,
    EventActionRepository eventActionRepository,
    IHttpClientFactory httpClientFactory,
    ILogger<EventActionDispatchService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var payload in queue.ReadAllAsync(stoppingToken))
        {
            var orderedActions = eventActionRepository.Get()
                .Where(a => a.Event == payload.Event)
                .Where(a => a.Event != VigilEventType.GroupCheckedOut || a.Group == payload.GroupName)
                .OrderBy(a => a.Priority);

            foreach (var action in orderedActions)
            {
                switch (action.Target)
                {
                    case WebhookTarget webhook:
                        await DispatchWebhookAsync(webhook, payload, stoppingToken);
                        break;
                    case CommandTarget command:
                        await DispatchCommandAsync(command, payload, stoppingToken);
                        break;
                }
            }
        }
    }

    private async Task DispatchWebhookAsync(
        WebhookTarget webhook,
        EventPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = JsonSerializer.Serialize(new
            {
                @event = payload.Event.ToString(),
                clientName = payload.ClientName,
                clientKeyId = payload.ClientKeyId,
                sessionId = payload.SessionId,
                occurredAt = payload.OccurredAt,
                metadata = payload.Metadata,
                group = payload.GroupName
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            if (webhook.Headers is not null)
            {
                foreach (var (key, value) in webhook.Headers)
                    request.Headers.TryAddWithoutValidation(key, value);
            }

            if (!string.IsNullOrWhiteSpace(webhook.Secret))
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var signature = ComputeSignature(webhook.Secret, timestamp, body);

                request.Headers.TryAddWithoutValidation("X-Vigil-Timestamp", timestamp.ToString());
                request.Headers.TryAddWithoutValidation("X-Vigil-Signature", $"sha256={signature}");
            }

            var client = httpClientFactory.CreateClient(nameof(EventActionDispatchService));

            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
                logger.LogWebhookDispatched(payload.Event, webhook.Url);
            else
                logger.LogWebhookDispatchFailed(payload.Event, webhook.Url, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWebhookDispatchError(ex, payload.Event, webhook.Url);
        }
    }

    private static string ComputeSignature(string secret, long timestamp, string body)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var message = Encoding.UTF8.GetBytes($"{timestamp}.{body}");
        var hash = HMACSHA256.HashData(key, message);

        return Convert.ToHexStringLower(hash);
    }

    private async Task DispatchCommandAsync(
        CommandTarget command,
        EventPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command.Command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            foreach (var argument in command.Arguments)
                startInfo.ArgumentList.Add(argument);

            if (command.Environment is not null)
            {
                foreach (var (key, value) in command.Environment)
                    startInfo.Environment[key] = value;
            }

            startInfo.Environment["VIGIL_EVENT"] = payload.Event.ToString();
            startInfo.Environment["VIGIL_CLIENT_NAME"] = payload.ClientName ?? string.Empty;
            startInfo.Environment["VIGIL_CLIENT_KEY_ID"] = payload.ClientKeyId?.ToString() ?? string.Empty;
            startInfo.Environment["VIGIL_SESSION_ID"] = payload.SessionId?.ToString() ?? string.Empty;
            startInfo.Environment["VIGIL_OCCURRED_AT"] = payload.OccurredAt.ToString("O");
            startInfo.Environment["VIGIL_GROUP"] = payload.GroupName ?? string.Empty;

            if (payload.Metadata is not null)
            {
                foreach (var (key, value) in payload.Metadata)
                    startInfo.Environment[$"VIGIL_METADATA_{SanitizeEnvironmentVariableKey(key)}"] = value;
            }

            using var process = Process.Start(startInfo);

            if (process is null)
            {
                logger.LogCommandDispatchFailed(payload.Event, command.Command, -1);
                return;
            }

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                logger.LogCommandDispatched(payload.Event, command.Command);
            }
            else
            {
                logger.LogCommandDispatchFailed(payload.Event, command.Command, process.ExitCode);

                var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(stderr))
                    logger.LogCommandStandardError(payload.Event, command.Command, stderr.Trim());
            }
        }
        catch (Exception ex)
        {
            logger.LogCommandDispatchError(ex, payload.Event, command.Command);
        }
    }

    private static string SanitizeEnvironmentVariableKey(string key)
    {
        var sanitized = key.ToUpperInvariant()
            .Select(c => char.IsAsciiLetterOrDigit(c) || c == '_' ? c : '_')
            .ToArray();

        return new string(sanitized);
    }
}
