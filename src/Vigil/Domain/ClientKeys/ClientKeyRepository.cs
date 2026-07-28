using System.Security.Cryptography;
using Ardalis.Result;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Data;
using Vigil.Domain.Errors;
using Vigil.Domain.Errors.ClientKeys;

namespace Vigil.Domain.ClientKeys;

internal sealed class ClientKeyRepository : JsonFileRepository<ClientKey>
{
    protected override Func<ClientKey, Guid> PrimaryKeySelector => key => key.Id;

    public ClientKeyRepository(
        ILogger<ClientKeyRepository> logger,
        IOptions<VigilOptions> options)
        : base(logger, options.Value.ClientKeysFilePath)
    {
    }

    public async Task<Result<ClientKey>> CreateKeyAsync(
        string clientName,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            var exists = Entities.Values.Any(
                k => k.ClientName.Equals(clientName, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                Logger.LogClientNameAlreadyExists(clientName);
                return ErrorCatalog.ClientKey.ClientNameMustBeUnique();
            }

            var clientKey = new ClientKey(
                Guid.NewGuid(),
                clientName,
                GenerateApiKey(),
                DateTime.UtcNow
            );

            Entities[clientKey.Id] = clientKey;

            return Result.Success(clientKey);
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        Logger.LogClientKeyCreated(result.Value.ClientName, result.Value.Id);

        await PersistAsync(cancellationToken);

        return result;
    }

    public async Task<Result> DeleteKeyAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await MutateAsync(() =>
        {
            if (id == Guid.Empty || !Entities.TryRemove(id, out var removedKey))
            {
                Logger.LogClientKeyNotFoundForDeletion(id);
                return ErrorCatalog.ClientKey.NotFound(id);
            }

            Logger.LogClientKeyDeleted(removedKey.ClientName, id);

            return Result.Success();
        }, cancellationToken);

        if (!result.IsSuccess)
            return result;

        await PersistAsync(cancellationToken);

        return result;
    }

    private static string GenerateApiKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
