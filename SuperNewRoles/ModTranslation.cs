using System.Collections.Generic;
using System.Reflection;
using System.IO;
using AmongUs.Data;

namespace SuperNewRoles;

public static partial class ModTranslation
{
    private static Dictionary<string, string> CurrentTranslations = new();
    public static void Load()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.TranslationData.csv");
        using var reader = new StreamReader(stream);
        var lang = DataManager.Settings.Language.CurrentLanguage;
        var translations = new Dictionary<string, string>();
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;
            var parts = line.Split(new[] { ',' }, 3);
            if (parts.Length < 3)
                continue;
            var key = parts[0].Trim();
            var text = (lang == SupportedLangs.Japanese ? parts[1] : parts[2]).Replace("\\n", "\n");
            translations[key] = text;
        }
        CurrentTranslations = translations;
    }
    public static string GetString(string key)
    {
        return CurrentTranslations.TryGetValue(key, out var value) ? value : key;
    }
    public static bool TryGetString(string key, out string value)
    {
        return CurrentTranslations.TryGetValue(key, out value);
    }
    public static string GetString(string key, params object[] format)
    {
        return string.Format(GetString(key), format);
    }
}
