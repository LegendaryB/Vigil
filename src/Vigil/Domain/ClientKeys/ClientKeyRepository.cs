using System.Collections.Concurrent;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.Extensions.Options;
using Vigil.Configuration;
using Vigil.Domain.Errors;
using Vigil.Domain.Errors.ClientKeys;

namespace Vigil.Domain.ClientKeys;

public class ClientKeyRepository
{
    private readonly ConcurrentDictionary<Guid, ClientKey> _keys = new();
    
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    
    public IReadOnlyList<ClientKey> Get() => _keys.Values.ToList();

    public ClientKeyRepository(
        ILogger<ClientKeyRepository> logger,
        IOptions<VigilOptions> options)
    {
        _filePath = options.Value.ClientKeysFilePath;
        
        LoadFromJsonFile();
    }
    
    public async Task<Result<ClientKey>> CreateKeyAsync(
        string clientName,
        CancellationToken cancellationToken)
    {
        var exists = _keys.Values.Any(
            k => k.ClientName.Equals(clientName, StringComparison.OrdinalIgnoreCase));
        
        if (exists)
            return ErrorCatalog.ClientKey.ClientNameMustBeUnique();

        var clientKey = new ClientKey(
            Guid.NewGuid(),
            clientName,
            GenerateApiKey(),
            DateTime.UtcNow
        );

        _keys[clientKey.Id] = clientKey;

        await SaveToJsonFileAsync(cancellationToken);

        return Result.Success(clientKey);
    }
    
    public async Task<Result> DeleteKeyAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty || !_keys.TryRemove(id, out _))
            return ErrorCatalog.ClientKey.NotFound(id);

        await SaveToJsonFileAsync(cancellationToken);

        return Result.Success();
    }

    private async Task SaveToJsonFileAsync(
        CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        
        try
        {
            var content = JsonSerializer.Serialize(
                _keys.Values,
                _serializerOptions);
            
            await File.WriteAllTextAsync(
                _filePath, 
                content,
                cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }
    
    private void LoadFromJsonFile()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            var content = File.ReadAllText(_filePath);
            var loadedKeys = JsonSerializer.Deserialize<List<ClientKey>>(content);

            if (loadedKeys is null)
                return;

            foreach (var key in loadedKeys)
                _keys[key.Id] = key;
        }
        catch (Exception)
        {
        }
    }

    private static string GenerateApiKey() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}