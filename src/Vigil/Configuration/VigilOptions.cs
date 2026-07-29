using System.Diagnostics.CodeAnalysis;

namespace Vigil.Configuration;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal class VigilOptions
{
    public string DataDirectory { get; set; } = string.Empty;

    public string AdminKey { get; set; } = string.Empty;

    private string ExpandedDataDirectory => Path.GetFullPath(Environment.ExpandEnvironmentVariables(DataDirectory));

    internal string ClientKeysFilePath => Path.Combine(ExpandedDataDirectory, "client-keys.json");

    internal string SessionsFilePath => Path.Combine(ExpandedDataDirectory, "sessions.json");

    internal string EventActionsFilePath => Path.Combine(ExpandedDataDirectory, "event-actions.json");
}