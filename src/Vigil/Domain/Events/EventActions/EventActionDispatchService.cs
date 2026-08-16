using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Vigil.Configuration;

namespace Vigil.Domain.Events.EventActions;

internal sealed class EventActionDispatchService(
    EventActionQueue queue,
    EventActionRepository eventActionRepository,
    DispatchLogRepository dispatchLogRepository,
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<EventActionsOptions> options,
    ILogger<EventActionDispatchService> logger) : BackgroundService
{
    private const int ErrorMessageMaxLength = 500;

    private readonly ResiliencePipeline<CommandAttemptResult> commandResiliencePipeline =
        BuildCommandResiliencePipeline(options);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var payload in queue.ReadAllAsync(stoppingToken))
        {
            var orderedActions = eventActionRepository.Get()
                .Where(a => a.Event == payload.Event)
                .Where(a => !a.Event.IsGroupScoped() || a.Group == payload.GroupName)
                .OrderBy(a => a.Priority);

            foreach (var action in orderedActions)
            {
                switch (action.Target)
                {
                    case WebhookTarget webhook:
                        await DispatchWebhookAsync(action.Id, webhook, payload, stoppingToken);
                        break;
                    case CommandTarget command:
                        await DispatchCommandAsync(action.Id, command, payload, stoppingToken);
                        break;
                }
            }
        }
    }

    private async Task DispatchWebhookAsync(
        Guid eventActionId,
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
            {
                logger.LogWebhookDispatched(payload.Event, webhook.Url);

                await RecordDispatchAsync(
                    eventActionId, payload, "webhook", webhook.Url,
                    succeeded: true, statusCode: (int)response.StatusCode, exitCode: null, errorMessage: null,
                    cancellationToken);
            }
            else
            {
                logger.LogWebhookDispatchFailed(payload.Event, webhook.Url, (int)response.StatusCode);

                await RecordDispatchAsync(
                    eventActionId, payload, "webhook", webhook.Url,
                    succeeded: false, statusCode: (int)response.StatusCode, exitCode: null,
                    errorMessage: Truncate(response.ReasonPhrase), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWebhookDispatchError(ex, payload.Event, webhook.Url);

            await RecordDispatchAsync(
                eventActionId, payload, "webhook", webhook.Url,
                succeeded: false, statusCode: null, exitCode: null,
                errorMessage: Truncate(ex.Message), cancellationToken);
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
        Guid eventActionId,
        CommandTarget command,
        EventPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await commandResiliencePipeline.ExecuteAsync(
                ct => new ValueTask<CommandAttemptResult>(ExecuteCommandAttemptAsync(command, payload, ct)),
                cancellationToken);

            if (result.Succeeded)
            {
                logger.LogCommandDispatched(payload.Event, command.Command);

                await RecordDispatchAsync(
                    eventActionId, payload, "command", command.Command,
                    succeeded: true, statusCode: null, exitCode: result.ExitCode, errorMessage: null,
                    cancellationToken);
            }
            else
            {
                logger.LogCommandDispatchFailed(payload.Event, command.Command, result.ExitCode);

                if (!string.IsNullOrWhiteSpace(result.StandardError))
                    logger.LogCommandStandardError(payload.Event, command.Command, result.StandardError);

                await RecordDispatchAsync(
                    eventActionId, payload, "command", command.Command,
                    succeeded: false, statusCode: null, exitCode: result.ExitCode,
                    errorMessage: Truncate(result.StandardError), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogCommandDispatchError(ex, payload.Event, command.Command);

            await RecordDispatchAsync(
                eventActionId, payload, "command", command.Command,
                succeeded: false, statusCode: null, exitCode: null,
                errorMessage: Truncate(ex.Message), cancellationToken);
        }
    }

    private static async Task<CommandAttemptResult> ExecuteCommandAttemptAsync(
        CommandTarget command,
        EventPayload payload,
        CancellationToken cancellationToken)
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
            return CommandAttemptResult.ProcessStartFailed;

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); }
                catch { /* best effort */ }
            }

            throw;
        }

        if (process.ExitCode == 0)
            return new CommandAttemptResult(true, 0, null);

        string? stderr = null;

        try
        {
            stderr = await process.StandardError.ReadToEndAsync(CancellationToken.None);
        }
        catch { /* best effort; don't mask the real exit-code failure */ }

        return new CommandAttemptResult(false, process.ExitCode, string.IsNullOrWhiteSpace(stderr) ? null : stderr.Trim());
    }

    private static ResiliencePipeline<CommandAttemptResult> BuildCommandResiliencePipeline(
        IOptionsMonitor<EventActionsOptions> options) =>
        new ResiliencePipelineBuilder<CommandAttemptResult>()
            .AddRetry(new RetryStrategyOptions<CommandAttemptResult>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Result is { Succeeded: false } ||
                    args.Outcome.Exception is not null and not OperationCanceledException)
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                TimeoutGenerator = _ => ValueTask.FromResult(
                    options.CurrentValue.CommandTimeout is { } timeout ? timeout : Timeout.InfiniteTimeSpan)
            })
            .Build();

    private sealed record CommandAttemptResult(bool Succeeded, int ExitCode, string? StandardError)
    {
        internal static readonly CommandAttemptResult ProcessStartFailed = new(false, -1, null);
    }

    private static string SanitizeEnvironmentVariableKey(string key)
    {
        var sanitized = key.ToUpperInvariant()
            .Select(c => char.IsAsciiLetterOrDigit(c) || c == '_' ? c : '_')
            .ToArray();

        return new string(sanitized);
    }

    private Task RecordDispatchAsync(
        Guid eventActionId,
        EventPayload payload,
        string targetType,
        string destination,
        bool succeeded,
        int? statusCode,
        int? exitCode,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var entry = new DispatchLogEntry(
            Guid.NewGuid(),
            eventActionId,
            payload.Event,
            payload.GroupName,
            targetType,
            destination,
            DateTime.UtcNow,
            succeeded,
            statusCode,
            exitCode,
            errorMessage);

        return dispatchLogRepository.RecordAsync(entry, cancellationToken);
    }

    private static string? Truncate(string? value) =>
        value is null or { Length: <= ErrorMessageMaxLength } ? value : value[..ErrorMessageMaxLength];
}
