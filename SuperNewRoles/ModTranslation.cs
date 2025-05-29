using System.Collections.Generic;
using System.Reflection;
using System.IO;
using AmongUs.Data;

namespace SuperNewRoles;

public static partial class ModTranslation
{
    private static Dictionary<string, string> CurrentTranslations = new();
    private static Dictionary<SupportedLangs, Dictionary<string, string>> AllTranslations = new();
    public static void Load()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.TranslationData.csv");
        var reader = new StreamReader(stream);

        Dictionary<string, string> japaneseTranslation = new();
        Dictionary<string, string> englishTranslation = new();

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            // 空白かコメントはスキップ
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;
            string[] parts = line.Split(',');
            if (parts.Length >= 3)
            {
                string key = parts[0].Trim();
                string japanese = parts[1].Replace("\\n", "\n");
                string english = parts[2].Replace("\\n", "\n");

                japaneseTranslation.Add(key, japanese);
                englishTranslation.Add(key, english);
            }
        }
        AllTranslations[SupportedLangs.Japanese] = japaneseTranslation;
        AllTranslations[SupportedLangs.English] = englishTranslation;
        switch (DataManager.Settings.Language.CurrentLanguage)
        {
            case SupportedLangs.Japanese:
                CurrentTranslations = AllTranslations[SupportedLangs.Japanese];
                break;
            case SupportedLangs.English:
                CurrentTranslations = AllTranslations[SupportedLangs.English];
                break;
            default:
                CurrentTranslations = AllTranslations[SupportedLangs.Japanese];
                break;
        }
    }
    public static string GetString(string key)
    {
        return CurrentTranslations.TryGetValue(key, out var value) ? value : key;
    }
    public static string GetString(string key, params object[] format)
    {
        return string.Format(GetString(key), format);
    }
}
