using System.Diagnostics.CodeAnalysis;

namespace Vigil.Configuration;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal class VigilOptions
{
    public string DataDirectory { get; set; } = string.Empty;

    public string ConfigurationFile { get; set; } = string.Empty;

    public string AdminKey { get; set; } = string.Empty;

    private string ExpandedDataDirectory => Path.GetFullPath(Environment.ExpandEnvironmentVariables(DataDirectory));

    internal string ExpandedConfigurationFile => Path.GetFullPath(Environment.ExpandEnvironmentVariables(ConfigurationFile));

    internal string ClientKeyUsageFilePath => Path.Combine(ExpandedDataDirectory, "client-key-usage.json");

    internal string SessionsFilePath => Path.Combine(ExpandedDataDirectory, "sessions.json");

    internal string DispatchLogFilePath => Path.Combine(ExpandedDataDirectory, "dispatch-log.json");
}
