using System.Collections.Generic;
using System.IO;
using SuperNewRoles.Patches; // For JsonParser

namespace SuperNewRoles.Modules;
public class VersionConfigData
{
    public string updateType { get; set; } = "all";
    public string version { get; set; } = null;
    public string channel { get; set; } = "all";
}

public static class VersionConfigManager
{
    private static string FilePath = Path.Combine(BepInEx.Paths.PatcherPluginPath, "snrupdate.json");
    private static VersionConfigData _configData;

    private static VersionConfigData LoadConfig()
    {
        if (_configData != null)
        {
            return _configData;
        }

        if (!File.Exists(FilePath))
        {
            _configData = new VersionConfigData();
            SaveConfig(); // Save a default config if it doesn't exist
            return _configData;
        }

        string json = File.ReadAllText(FilePath);
        var parser = JsonParser.Parse(json) as Dictionary<string, object>;
        _configData = new VersionConfigData
        {
            updateType = parser.TryGetValue("updateType", out var type) ? type as string : "all",
            version = parser.TryGetValue("version", out var ver) ? ver as string : "",
            channel = parser.TryGetValue("channel", out var ch) ? (ch as string) : "all"
        };
        return _configData;
    }

    private static void SaveConfig()
    {
        if (_configData == null) return;

        var dictionary = new Dictionary<string, object>
            {
                { "updateType", _configData.updateType },
                { "version", _configData.version },
                { "channel", string.IsNullOrEmpty(_configData.channel) ? "all" : _configData.channel }
            };
        string json = JsonParser.Serialize(dictionary);
        File.WriteAllText(FilePath, json);
    }

    public static string GetVersionType()
    {
        return LoadConfig().updateType;
    }

    public static void SetVersionType(string updateType)
    {
        LoadConfig().updateType = updateType;
        SaveConfig();
    }

    public static string GetVersion()
    {
        return LoadConfig().version;
    }

    public static void SaveVersion(string version)
    {
        LoadConfig().version = version;
        SaveConfig();
    }

    public static string GetChannel()
    {
        return LoadConfig().channel ?? "all";
    }

    public static void SetChannel(string channel)
    {
        LoadConfig().channel = string.IsNullOrEmpty(channel) ? "all" : channel;
        SaveConfig();
    }
}