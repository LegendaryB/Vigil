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
    private readonly ILogger<ClientKeyRepository> _logger;
    
    private readonly ConcurrentDictionary<Guid, ClientKey> _keys = new();
    
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    
    public IReadOnlyList<ClientKey> Get() => _keys.Values.ToList();

    public ClientKeyRepository(
        ILogger<ClientKeyRepository> logger,
        IOptions<VigilOptions> options)
    {
        _logger = logger;
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
        {
            _logger.LogClientNameAlreadyExists(clientName);
            return ErrorCatalog.ClientKey.ClientNameMustBeUnique();
        }

        var clientKey = new ClientKey(
            Guid.NewGuid(),
            clientName,
            GenerateApiKey(),
            DateTime.UtcNow
        );

        _keys[clientKey.Id] = clientKey;
        
        _logger.LogClientKeyCreated(clientKey.ClientName, clientKey.Id);

        await SaveToJsonFileAsync(cancellationToken);

        return Result.Success(clientKey);
    }
    
    public async Task<Result> DeleteKeyAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty || !_keys.TryRemove(id, out var removedKey))
        {
            _logger.LogClientKeyNotFoundForDeletion(id);
            return ErrorCatalog.ClientKey.NotFound(id);
        }
        
        _logger.LogClientKeyDeleted(removedKey.ClientName, id);

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

            _logger.LogKeysPersisted(
                _keys.Count,
                _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogErrorSavingToFile(
                ex,
                _filePath);
            
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }
    
    private void LoadFromJsonFile()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogFileNotFoundStartingEmpty(_filePath);
            return;
        }

        try
        {
            var content = File.ReadAllText(_filePath);
            var loadedKeys = JsonSerializer.Deserialize<List<ClientKey>>(content);

            if (loadedKeys is null)
            {
                _logger.LogFileContentEmpty(_filePath);
                return;
            }

            foreach (var key in loadedKeys)
                _keys[key.Id] = key;

            _logger.LogKeysLoadedFromFile(_keys.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogErrorLoadingFromFile(ex, _filePath);
            
            throw new InvalidOperationException(
                $"Failed to initialize ClientKeyRepository from '{_filePath}'. " +
                "Application startup aborted to prevent data loss or invalid state.", ex);
        }
    }

    private static string GenerateApiKey() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}