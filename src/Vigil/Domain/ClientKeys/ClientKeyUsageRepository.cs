using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Data;

namespace Vigil.Domain.ClientKeys;

internal sealed record ClientKeyUsage(Guid Id, DateTime LastUsedAt);

internal sealed class ClientKeyUsageRepository : JsonFileRepository<ClientKeyUsage>
{
    protected override Func<ClientKeyUsage, Guid> PrimaryKeySelector => usage => usage.Id;

    public ClientKeyUsageRepository(
        ILogger<ClientKeyUsageRepository> logger,
        IOptions<VigilOptions> options)
        : base(logger, options.Value.ClientKeyUsageFilePath)
    {
    }

    internal DateTime? GetLastUsedAt(Guid clientKeyId) =>
        Entities.TryGetValue(clientKeyId, out var usage) ? usage.LastUsedAt : null;

    internal async Task RecordAsync(Guid clientKeyId, CancellationToken cancellationToken)
    {
        await MutateAsync(
            () => Entities[clientKeyId] = new ClientKeyUsage(clientKeyId, DateTime.UtcNow),
            cancellationToken);

        await PersistAsync(cancellationToken);
    }
}
