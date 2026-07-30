using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Vigil.Domain.Events.EventActions;

internal static class WebhookResilienceExtensions
{
    internal static IHttpClientBuilder AddWebhookRetryHandler(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("webhook-retry", static pipeline => pipeline
            .AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                DelayGenerator = static args =>
                {
                    var retryAfter = args.Outcome.Result?.Headers.RetryAfter;

                    if (retryAfter?.Delta is { } delta)
                        return ValueTask.FromResult<TimeSpan?>(delta);

                    if (retryAfter?.Date is { } date)
                        return ValueTask.FromResult<TimeSpan?>(date - DateTimeOffset.UtcNow);

                    return ValueTask.FromResult<TimeSpan?>(null);
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(10)));

        return builder;
    }
}
