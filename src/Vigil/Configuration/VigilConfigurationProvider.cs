using Microsoft.Extensions.Options;

namespace Vigil.Configuration;

internal sealed class VigilConfigurationProvider : IDisposable
{
    private static readonly TimeSpan ReloadDebounce = TimeSpan.FromMilliseconds(500);

    private readonly string _filePath;
    private readonly ILogger<VigilConfigurationProvider> _logger;
    private readonly object _loadLock = new();
    private readonly Timer _reloadTimer;
    private readonly FileSystemWatcher? _watcher;
    private readonly Dictionary<Guid, DateTime> _firstSeenAt = [];

    private volatile VigilConfiguration _current = VigilConfiguration.Empty;

    public VigilConfigurationProvider(
        IOptions<VigilOptions> options,
        ILogger<VigilConfigurationProvider> logger)
    {
        _filePath = options.Value.ExpandedConfigurationFile;
        _logger = logger;
        _reloadTimer = new Timer(_ => Reload(), null, Timeout.Infinite, Timeout.Infinite);

        Load(isStartup: true);

        _watcher = CreateWatcher();
    }

    public VigilConfiguration Current => _current;

    public void Dispose()
    {
        _watcher?.Dispose();
        _reloadTimer.Dispose();
    }

    private FileSystemWatcher? CreateWatcher()
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            _logger.LogConfigurationReloadUnavailable(_filePath);
            return null;
        }

        var watcher = new FileSystemWatcher(directory, Path.GetFileName(_filePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
        };

        FileSystemEventHandler onChange = (_, _) => ScheduleReload();

        watcher.Changed += onChange;
        watcher.Created += onChange;
        watcher.Deleted += onChange;
        watcher.Renamed += (_, _) => ScheduleReload();
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    private void ScheduleReload() => _reloadTimer.Change(ReloadDebounce, Timeout.InfiniteTimeSpan);

    private void Reload()
    {
        try
        {
            Load(isStartup: false);
        }
        catch (Exception ex)
        {
            _logger.LogConfigurationReloadFailed(ex, _filePath);
        }
    }

    private void Load(bool isStartup)
    {
        lock (_loadLock)
        {
            if (!File.Exists(_filePath))
            {
                if (isStartup)
                {
                    _logger.LogConfigurationFileMissing(_filePath);
                    return;
                }

                _logger.LogConfigurationFileRemoved(_filePath);
                return;
            }

            var yaml = ReadAllTextShared(_filePath);

            var result = VigilConfigurationLoader.Load(
                yaml,
                Environment.GetEnvironmentVariable,
                DateTime.UtcNow);

            foreach (var warning in result.Warnings)
                _logger.LogConfigurationWarning(warning);

            if (result.Configuration is null)
            {
                foreach (var error in result.Errors)
                    _logger.LogConfigurationError(error);

                if (isStartup)
                {
                    throw new InvalidOperationException(
                        $"Configuration file '{_filePath}' is invalid:{Environment.NewLine}" +
                        string.Join(Environment.NewLine, result.Errors.Select(e => $"  - {e}")));
                }

                _logger.LogConfigurationReloadRejected(_filePath);
                return;
            }

            _current = KeepFirstSeenTimestamps(result.Configuration);

            _logger.LogConfigurationLoaded(
                _current.ClientKeys.Count,
                _current.EventActions.Count,
                _filePath);
        }
    }

    private VigilConfiguration KeepFirstSeenTimestamps(VigilConfiguration loaded)
    {
        var clientKeys = loaded.ClientKeys
            .Select(k => k with { CreatedAt = FirstSeen(k.Id, k.CreatedAt) })
            .ToList();

        var eventActions = loaded.EventActions
            .Select(a => a with { CreatedAt = FirstSeen(a.Id, a.CreatedAt) })
            .ToList();

        return new VigilConfiguration(clientKeys, eventActions);
    }

    private DateTime FirstSeen(Guid id, DateTime loadedAt)
    {
        if (_firstSeenAt.TryGetValue(id, out var firstSeen))
            return firstSeen;

        _firstSeenAt[id] = loadedAt;
        return loadedAt;
    }

    private static string ReadAllTextShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
