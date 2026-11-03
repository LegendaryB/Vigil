using Vigil.Configuration;

namespace Vigil.Domain.ClientKeys;

internal sealed class ClientKeyRepository(
    VigilConfigurationProvider configurationProvider,
    ClientKeyUsageRepository usageRepository)
{
    public IReadOnlyList<ClientKey> Get() =>
        configurationProvider.Current.ClientKeys
            .Select(key => key with { LastUsedAt = usageRepository.GetLastUsedAt(key.Id) })
            .ToList();

    internal async Task RecordUsageAsync(Guid id, CancellationToken cancellationToken)
    {
        if (configurationProvider.Current.ClientKeys.All(key => key.Id != id))
            return;

        await usageRepository.RecordAsync(id, cancellationToken);
    }
}
