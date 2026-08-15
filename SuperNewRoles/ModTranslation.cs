using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using AmongUs.Data;
using HarmonyLib;

namespace SuperNewRoles;

public static partial class ModTranslation
{
    private const int EstimatedTranslationCount = 1900;
    private const int JapaneseIndex = 0;
    private const int EnglishIndex = 1;
    private const int SChineseIndex = 2;
    private const int TChineseIndex = 3;

    private static Dictionary<string, string> CurrentTranslations;
    private static SupportedLangs? CurrentLang;
    private static readonly Dictionary<string, string>[] AllTranslations = new Dictionary<string, string>[4];
    private static readonly HashSet<string> MissingTranslations = new(StringComparer.Ordinal);

    // 単体テスト用の翻訳CSV差し替え
    private static Func<Stream> TestTranslationStreamProvider;
    // 単体テスト用の現在言語差し替え
    private static SupportedLangs? TestLanguageOverride;
    // 単体テスト用のAndroid判定差し替え
    private static bool? TestAndroidOverride;

    public static void Load()
    {
        ClearMissingTranslations();
        CurrentLang = null;

        if (!IsAndroid())
            LoadAllTranslations();

        UpdateCurrentTranslations();
    }

    private static void LoadAllTranslations()
    {
        var loadedTranslations = new Dictionary<string, string>[AllTranslations.Length];
        for (int i = 0; i < loadedTranslations.Length; i++)
            loadedTranslations[i] = CreateTranslationDictionary();

        string csv = ReadTranslationCsv();
        ReadOnlySpan<char> remaining = csv.AsSpan();
        while (TryReadLine(ref remaining, out ReadOnlySpan<char> line))
        {
            if (!TryParseLine(line, out ReadOnlySpan<char> keySpan, out ReadOnlySpan<char> japaneseSpan,
                out ReadOnlySpan<char> englishSpan, out ReadOnlySpan<char> sChineseSpan, out ReadOnlySpan<char> tChineseSpan))
                continue;

            string key = keySpan.ToString();
            string japanese = DecodeField(japaneseSpan);
            string english = DecodeField(englishSpan);
            string sChinese = sChineseSpan.SequenceEqual(englishSpan) ? english : DecodeField(sChineseSpan);
            string tChinese = tChineseSpan.SequenceEqual(englishSpan) ? english : DecodeField(tChineseSpan);

            // 旧実装と同様に、重複キーは最初の値を採用する。
            loadedTranslations[JapaneseIndex].TryAdd(key, japanese);
            loadedTranslations[EnglishIndex].TryAdd(key, english);
            loadedTranslations[SChineseIndex].TryAdd(key, sChinese);
            loadedTranslations[TChineseIndex].TryAdd(key, tChinese);
        }

        for (int i = 0; i < loadedTranslations.Length; i++)
            AllTranslations[i] = loadedTranslations[i];
    }

    private static Dictionary<string, string> LoadCurrentLanguageOnly(SupportedLangs lang)
    {
        var translations = CreateTranslationDictionary();
        int languageIndex = GetTableLanguageIndex(lang);

        string csv = ReadTranslationCsv();
        ReadOnlySpan<char> remaining = csv.AsSpan();
        while (TryReadLine(ref remaining, out ReadOnlySpan<char> line))
        {
            if (!TryParseLine(line, out ReadOnlySpan<char> keySpan, out ReadOnlySpan<char> japaneseSpan,
                out ReadOnlySpan<char> englishSpan, out ReadOnlySpan<char> sChineseSpan, out ReadOnlySpan<char> tChineseSpan))
                continue;

            ReadOnlySpan<char> valueSpan = languageIndex switch
            {
                JapaneseIndex => japaneseSpan,
                SChineseIndex => sChineseSpan,
                TChineseIndex => tChineseSpan,
                _ => englishSpan
            };
            translations.TryAdd(keySpan.ToString(), DecodeField(valueSpan));
        }

        return translations;
    }

    private static bool TryParseLine(
        ReadOnlySpan<char> line,
        out ReadOnlySpan<char> key,
        out ReadOnlySpan<char> japanese,
        out ReadOnlySpan<char> english,
        out ReadOnlySpan<char> sChinese,
        out ReadOnlySpan<char> tChinese)
    {
        key = default;
        japanese = default;
        english = default;
        sChinese = default;
        tChinese = default;

        if (line.Trim().IsEmpty || line[0] == '#')
            return false;

        int firstComma = line.IndexOf(',');
        if (firstComma < 0)
            return false;

        ReadOnlySpan<char> afterFirstComma = line[(firstComma + 1)..];
        int secondCommaOffset = afterFirstComma.IndexOf(',');
        if (secondCommaOffset < 0)
            return false;

        key = line[..firstComma].Trim();
        if (key.IsEmpty)
            return false;

        japanese = afterFirstComma[..secondCommaOffset];
        ReadOnlySpan<char> afterSecondComma = afterFirstComma[(secondCommaOffset + 1)..];
        int thirdCommaOffset = afterSecondComma.IndexOf(',');
        if (thirdCommaOffset < 0)
        {
            english = afterSecondComma;
            sChinese = english;
            tChinese = english;
            return true;
        }

        english = afterSecondComma[..thirdCommaOffset];
        ReadOnlySpan<char> afterThirdComma = afterSecondComma[(thirdCommaOffset + 1)..];
        int fourthCommaOffset = afterThirdComma.IndexOf(',');
        if (fourthCommaOffset < 0)
        {
            sChinese = afterThirdComma;
            tChinese = english;
            return true;
        }

        sChinese = afterThirdComma[..fourthCommaOffset];
        // 5列目には残りのカンマも含め、従来のCSV解釈を維持する。
        tChinese = afterThirdComma[(fourthCommaOffset + 1)..];
        return true;
    }

    private static Dictionary<string, string> CreateTranslationDictionary()
    {
        return new Dictionary<string, string>(EstimatedTranslationCount, StringComparer.Ordinal);
    }

    private static string ReadTranslationCsv()
    {
        using Stream stream = TestTranslationStreamProvider != null
            ? TestTranslationStreamProvider()
            : SuperNewRolesPlugin.Assembly.GetManifestResourceStream("SuperNewRoles.Resources.TranslationData.csv");

        if (stream == null)
            throw new InvalidOperationException("TranslationData.csv could not be opened.");

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static bool TryReadLine(ref ReadOnlySpan<char> remaining, out ReadOnlySpan<char> line)
    {
        if (remaining.IsEmpty)
        {
            line = default;
            return false;
        }

        int lineEnd = remaining.IndexOfAny('\r', '\n');
        if (lineEnd < 0)
        {
            line = remaining;
            remaining = default;
            return true;
        }

        line = remaining[..lineEnd];
        int nextLine = lineEnd;
        while (nextLine < remaining.Length && (remaining[nextLine] == '\r' || remaining[nextLine] == '\n'))
            nextLine++;
        remaining = remaining[nextLine..];
        return true;
    }

    private static string DecodeField(ReadOnlySpan<char> field)
    {
        string text = field.ToString();
        return text.Contains("\\n", StringComparison.Ordinal) ? text.Replace("\\n", "\n") : text;
    }

    private static SupportedLangs GetCurrentSupportedLang()
    {
        // 単体テストではIL2CPPのDataManagerへ触れず、明示した言語を優先する。
        if (TestLanguageOverride.HasValue)
            return TestLanguageOverride.Value;

        try
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
        catch
        {
            return SupportedLangs.English;
        }
    }

    public static void UpdateCurrentTranslations()
    {
        SupportedLangs newLang = GetCurrentSupportedLang();
        if (CurrentLang == newLang)
            return;

        Dictionary<string, string> newTranslations;
        if (IsAndroid())
        {
            newTranslations = LoadCurrentLanguageOnly(newLang);
        }
        else
        {
            newTranslations = AllTranslations[GetTableLanguageIndex(newLang)];
            if (newTranslations == null)
                return;
        }

        // 読み込み完了後にまとめて公開し、失敗時は旧言語の状態を維持する。
        CurrentTranslations = newTranslations;
        CurrentLang = newLang;
        ClearMissingTranslations();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(string key)
    {
        Dictionary<string, string> translations = CurrentTranslations;
        if (key == null || translations == null)
            return key;

        if (translations.TryGetValue(key, out string value))
            return value;

        return HandleMissingTranslation(key);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string HandleMissingTranslation(string key)
    {
        if (!key.Contains("<color=", StringComparison.Ordinal)
            && !key.Contains("</color>", StringComparison.Ordinal)
            && key != "CrewmateIntro1"
            && MarkTranslationMissing(key))
        {
            Logger.Warning($"Missing translation for key: {key}");
        }

        return key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetString(string key, out string value)
    {
        Dictionary<string, string> translations = CurrentTranslations;
        if (key == null || translations == null)
        {
            value = null;
            return false;
        }

        return translations.TryGetValue(key, out value);
    }

    public static string GetString(string key, object arg0)
    {
        return string.Format(GetString(key), arg0);
    }

    public static string GetString(string key, object arg0, object arg1)
    {
        return string.Format(GetString(key), arg0, arg1);
    }

    public static string GetString(string key, object arg0, object arg1, object arg2)
    {
        return string.Format(GetString(key), arg0, arg1, arg2);
    }

    public static string GetString(string key, params object[] format)
    {
        return string.Format(GetString(key), format);
    }

    public static void Cleanup()
    {
        CurrentTranslations = null;
        CurrentLang = null;

        for (int i = 0; i < AllTranslations.Length; i++)
            AllTranslations[i] = null;

        ClearMissingTranslations();
    }

    private static int GetTableLanguageIndex(SupportedLangs lang)
    {
        return lang switch
        {
            SupportedLangs.Japanese => JapaneseIndex,
            SupportedLangs.SChinese => SChineseIndex,
            SupportedLangs.TChinese => TChineseIndex,
            _ => EnglishIndex
        };
    }

    private static bool MarkTranslationMissing(string key)
    {
        lock (MissingTranslations)
            return MissingTranslations.Add(key);
    }

    private static void ClearMissingTranslations()
    {
        lock (MissingTranslations)
            MissingTranslations.Clear();
    }

    private static bool IsAndroid()
    {
        return TestAndroidOverride ?? ModHelpers.IsAndroid();
    }

    // --- テスト補助 ---
    // 埋め込みリソースの代わりに、単体テスト用の翻訳CSVを設定する。
    public static void SetTestTranslationCsv(string csv)
    {
        if (csv == null)
            throw new ArgumentNullException(nameof(csv));

        byte[] bytes = Encoding.UTF8.GetBytes(csv);
        TestTranslationStreamProvider = () => new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: true);
    }

    public static void SetTestTranslationStreamProvider(Func<Stream> provider)
    {
        TestTranslationStreamProvider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    // Among UsのDataManagerへ触れずに、単体テスト用の言語を設定する。
    public static void SetTestLanguage(SupportedLangs lang)
    {
        TestLanguageOverride = lang;
    }

    public static void ClearTestLanguage()
    {
        TestLanguageOverride = null;
    }

    public static void SetTestAndroid(bool isAndroid)
    {
        TestAndroidOverride = isAndroid;
    }

    public static void ClearTestAndroid()
    {
        TestAndroidOverride = null;
    }

    // テスト用CSVを解除し、埋め込みリソースへ戻す。
    public static void ClearTestTranslationCsv()
    {
        TestTranslationStreamProvider = null;
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.SetLanguage))]
    private sealed class TranslationControllerSetLanguagePatch
    {
        public static void Postfix()
        {
            UpdateCurrentTranslations();
        }
    }
}
