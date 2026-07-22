namespace Vigil.Configuration;

public class VigilOptions
{
    public string DataDirectory { get; set; } = "./data";
    
    public string AdminKey { get; set; } = string.Empty;

    private string ExpandedDataDirectory => Path.GetFullPath(Environment.ExpandEnvironmentVariables(DataDirectory));

    public string ClientKeysFilePath => Path.Combine(ExpandedDataDirectory, "client-keys.json");
    public string TicketsFilePath => Path.Combine(ExpandedDataDirectory, "tickets.json");
}