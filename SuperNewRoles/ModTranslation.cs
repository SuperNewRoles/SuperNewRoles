using System.Collections.Generic;
using System.Reflection;
using System.IO;
using AmongUs.Data;
using HarmonyLib;
using System;

namespace SuperNewRoles;

public static partial class ModTranslation
{
    private static Dictionary<string, string> CurrentTranslations = new();
    private static SupportedLangs? CurrentLang = null;
    private static Dictionary<SupportedLangs, Dictionary<string, string>> AllTranslations = new();
    public static void Load()
    {
        if (!ModHelpers.IsAndroid())
            AllTranslations = LoadTranslation();
        UpdateCurrentTranslations();
    }
    private static Dictionary<SupportedLangs, Dictionary<string, string>> LoadTranslation(SupportedLangs? lang = null)
    {
        using var stream = SuperNewRolesPlugin.Assembly.GetManifestResourceStream("SuperNewRoles.Resources.TranslationData.csv");
        using var reader = new StreamReader(stream);
        Dictionary<string, string> japaneseTranslation = new();
        Dictionary<string, string> englishTranslation = new();
        Dictionary<string, string> sChineseTranslation = new();
        Dictionary<string, string> tChineseTranslation = new();
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
                string sChinese = parts.Length >= 4 ? parts[3].Replace("\\n", "\n") : english;
                string tChinese = parts.Length >= 5 ? parts[4].Replace("\\n", "\n") : english;

                japaneseTranslation.Add(key, japanese);
                englishTranslation.Add(key, english);
                sChineseTranslation.Add(key, sChinese);
                tChineseTranslation.Add(key, tChinese);
            }
        }
        return new Dictionary<SupportedLangs, Dictionary<string, string>>
        {
            { SupportedLangs.Japanese, japaneseTranslation },
            { SupportedLangs.English, englishTranslation },
            { SupportedLangs.SChinese, sChineseTranslation },
            { SupportedLangs.TChinese, tChineseTranslation }
        };
    }
    private static SupportedLangs GetCurrentSupportedLang()
    {
        switch (DataManager.Settings.Language.CurrentLanguage)
        {
            case SupportedLangs.Japanese:
            case SupportedLangs.SChinese:
            case SupportedLangs.TChinese:
                return DataManager.Settings.Language.CurrentLanguage;
            default:
                return SupportedLangs.English;
        }
    }
    public static void UpdateCurrentTranslations()
    {
        if (CurrentLang == GetCurrentSupportedLang())
            return;
        CurrentLang = GetCurrentSupportedLang();
        // Androidはメモリ節約のため現在の翻訳しか保持しない
        if (ModHelpers.IsAndroid())
            AllTranslations = LoadTranslation();
        switch (CurrentLang)
        {
            case SupportedLangs.Japanese:
                CurrentTranslations = AllTranslations[SupportedLangs.Japanese];
                break;
            case SupportedLangs.English:
                CurrentTranslations = AllTranslations[SupportedLangs.English];
                break;
            case SupportedLangs.SChinese:
                CurrentTranslations = AllTranslations[SupportedLangs.SChinese];
                break;
            case SupportedLangs.TChinese:
                CurrentTranslations = AllTranslations[SupportedLangs.TChinese];
                break;
            default:
                throw new Exception($"Invalid language: {CurrentLang}");
        }
        if (ModHelpers.IsAndroid())
            AllTranslations = null;
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
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.SetLanguage))]
    class TranslationControllerGetStringPatch
    {
        public static void Postfix()
        {
            UpdateCurrentTranslations();
        }
    }
}
