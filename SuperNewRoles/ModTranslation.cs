using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using AmongUs.Data;
using HarmonyLib;

namespace SuperNewRoles;

// 高速ハッシュテーブル実装
internal unsafe struct FastHashTable
{
    private const int TableSize = 4096; // 2の累乗にすることでモジュロ演算を高速化
    private const int MaxKeyLength = 128;

    private fixed ulong buckets[TableSize * 16]; // 各バケットは最大16エントリ (keyHash, valueOffset, keyOffset, keyLength)
    private byte* stringData;
    private int stringDataSize;
    private int stringDataCapacity;

    public static FastHashTable* Create(int capacity)
    {
        var table = (FastHashTable*)Marshal.AllocHGlobal(sizeof(FastHashTable));
        Unsafe.InitBlock(table, 0, (uint)sizeof(FastHashTable));
        table->stringDataCapacity = capacity;
        table->stringData = (byte*)Marshal.AllocHGlobal(capacity);
        table->stringDataSize = 0;

        // バケットを初期化
        for (int i = 0; i < TableSize * 16; i++)
        {
            table->buckets[i] = 0;
        }

        return table;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong FastHash(byte* str, int len)
    {
        unchecked
        {
            // xxHash64のような高速ハッシュアルゴリズム
            const ulong PRIME64_1 = 11400714785074694791UL;
            const ulong PRIME64_2 = 14029467366897019727UL;
            const ulong PRIME64_3 = 1609587929392839161UL;
            const ulong PRIME64_4 = 9650029242287828579UL;
            const ulong PRIME64_5 = 2870177450012600261UL;

            ulong h64;

            if (len >= 32)
            {
                ulong v1 = PRIME64_1 + PRIME64_2;
                ulong v2 = PRIME64_2;
                ulong v3 = 0;
                ulong v4 = (ulong)(-(long)PRIME64_1);

                int pos = 0;
                do
                {
                    v1 = RotateLeft64(v1 + (*(ulong*)(str + pos) * PRIME64_2), 31) * PRIME64_1;
                    pos += 8;
                    v2 = RotateLeft64(v2 + (*(ulong*)(str + pos) * PRIME64_2), 31) * PRIME64_1;
                    pos += 8;
                    v3 = RotateLeft64(v3 + (*(ulong*)(str + pos) * PRIME64_2), 31) * PRIME64_1;
                    pos += 8;
                    v4 = RotateLeft64(v4 + (*(ulong*)(str + pos) * PRIME64_2), 31) * PRIME64_1;
                    pos += 8;
                } while (pos <= len - 32);

                h64 = RotateLeft64(v1, 1) + RotateLeft64(v2, 7) + RotateLeft64(v3, 12) + RotateLeft64(v4, 18);

                v1 *= PRIME64_2; v1 = RotateLeft64(v1, 31); v1 *= PRIME64_1; h64 ^= v1;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v2 *= PRIME64_2; v2 = RotateLeft64(v2, 31); v2 *= PRIME64_1; h64 ^= v2;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v3 *= PRIME64_2; v3 = RotateLeft64(v3, 31); v3 *= PRIME64_1; h64 ^= v3;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                v4 *= PRIME64_2; v4 = RotateLeft64(v4, 31); v4 *= PRIME64_1; h64 ^= v4;
                h64 = h64 * PRIME64_1 + PRIME64_4;

                str += pos;
                len -= pos;
            }
            else
            {
                h64 = PRIME64_5;
            }

            h64 += (ulong)len;

            while (len >= 8)
            {
                ulong k1 = *(ulong*)str;
                k1 *= PRIME64_2; k1 = RotateLeft64(k1, 31); k1 *= PRIME64_1;
                h64 ^= k1;
                h64 = RotateLeft64(h64, 27) * PRIME64_1 + PRIME64_4;
                str += 8;
                len -= 8;
            }

            if (len >= 4)
            {
                h64 ^= (*(uint*)str) * PRIME64_1;
                h64 = RotateLeft64(h64, 23) * PRIME64_2 + PRIME64_3;
                str += 4;
                len -= 4;
            }

            while (len > 0)
            {
                h64 ^= (*str) * PRIME64_5;
                h64 = RotateLeft64(h64, 11) * PRIME64_1;
                str++;
                len--;
            }

            h64 ^= h64 >> 33;
            h64 *= PRIME64_2;
            h64 ^= h64 >> 29;
            h64 *= PRIME64_3;
            h64 ^= h64 >> 32;

            return h64;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft64(ulong value, int count)
    {
        return (value << count) | (value >> (64 - count));
    }

    public void Add(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var valueBytes = Encoding.UTF8.GetBytes(value ?? "");

        fixed (byte* keyPtr = keyBytes)
        fixed (byte* valuePtr = valueBytes)
        {
            AddRaw(keyPtr, keyBytes.Length, valuePtr, valueBytes.Length);
        }
    }

    private void AddRaw(byte* key, int keyLen, byte* value, int valueLen)
    {
        if (stringDataSize + keyLen + valueLen + 2 > stringDataCapacity)
        {
            // 容量拡張
            int newCapacity = stringDataCapacity == 0 ? 1024 * 16 : stringDataCapacity * 2;
            int requiredSize = stringDataSize + keyLen + valueLen + 2;
            if (newCapacity < requiredSize)
            {
                newCapacity = requiredSize;
            }

            var newData = (byte*)Marshal.AllocHGlobal(newCapacity);
            try
            {
                if (stringData != null)
                {
                    Buffer.MemoryCopy(stringData, newData, newCapacity, stringDataSize);
                }
            }
            catch
            {
                Marshal.FreeHGlobal((IntPtr)newData);
                throw;
            }

            if (stringData != null)
            {
                Marshal.FreeHGlobal((IntPtr)stringData);
            }
            stringData = newData;
            stringDataCapacity = newCapacity;
        }

        ulong hash = FastHash(key, keyLen);
        int bucketIndex = (int)(hash & (TableSize - 1));

        // 文字列データを保存
        int keyOffset = stringDataSize;
        Buffer.MemoryCopy(key, stringData + keyOffset, keyLen, keyLen);
        stringDataSize += keyLen;

        int valueOffset = stringDataSize;
        Buffer.MemoryCopy(value, stringData + valueOffset, valueLen, valueLen);
        stringDataSize += valueLen;

        // ハッシュテーブルに追加
        for (int i = 0; i < 16; i++)
        {
            int idx = bucketIndex * 16 + i * 4;
            if (buckets[idx] == 0) // 空きスロット
            {
                buckets[idx] = hash;
                buckets[idx + 1] = (ulong)valueOffset;
                buckets[idx + 2] = (ulong)keyOffset;
                buckets[idx + 3] = (ulong)((keyLen << 16) | valueLen);
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Get(string key)
    {
        if (key == null) return key;

        var keyBytes = Encoding.UTF8.GetBytes(key);
        fixed (byte* keyPtr = keyBytes)
        {
            string value = GetRaw(keyPtr, keyBytes.Length);
            if (value == null) // keyがCSVにない場合にログを出力
            {
                if (!key.Contains("<color=") && !key.Contains("</color>") && value != "CrewmateIntro1")
                {  // colorタグが含まれている場合・CrewmateIntro1(本体純正)は出力しない
                    Logger.Warning($"Missing translation for key: {key}");
                }
            }
            return value ?? key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(string key, out string value)
    {
        if (key == null)
        {
            value = null;
            return false;
        }

        var keyBytes = Encoding.UTF8.GetBytes(key);
        fixed (byte* keyPtr = keyBytes)
        {
            value = GetRaw(keyPtr, keyBytes.Length);
            return value != null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetRaw(byte* key, int keyLen)
    {
        ulong hash = FastHash(key, keyLen);
        int bucketIndex = (int)(hash & (TableSize - 1));

        for (int i = 0; i < 16; i++)
        {
            int idx = bucketIndex * 16 + i * 4;
            if (buckets[idx] == 0) break;

            if (buckets[idx] == hash)
            {
                int storedKeyLen = (int)(buckets[idx + 3] >> 16);
                if (storedKeyLen == keyLen)
                {
                    byte* storedKey = stringData + (int)buckets[idx + 2];

                    // SIMD比較
                    if (FastCompare(key, storedKey, keyLen))
                    {
                        int valueOffset = (int)buckets[idx + 1];
                        int valueLen = (int)(buckets[idx + 3] & 0xFFFF);
                        return Encoding.UTF8.GetString(stringData + valueOffset, valueLen);
                    }
                }
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool FastCompare(byte* a, byte* b, int len)
    {
        if (Avx2.IsSupported && len >= 32)
        {
            int i = 0;
            for (; i + 32 <= len; i += 32)
            {
                var va = Avx.LoadVector256(a + i);
                var vb = Avx.LoadVector256(b + i);
                var cmp = Avx2.CompareEqual(va, vb);
                if (Avx2.MoveMask(cmp) != -1) return false;
            }

            // 残りを処理
            for (; i < len; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        else
        {
            for (int i = 0; i < len; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }

    public void Destroy()
    {
        if (stringData != null)
        {
            Marshal.FreeHGlobal((IntPtr)stringData);
            stringData = null;
        }
    }

    public static void DestroyTable(FastHashTable* table)
    {
        if (table != null)
        {
            table->Destroy();
            Marshal.FreeHGlobal((IntPtr)table);
        }
    }
}

public static unsafe partial class ModTranslation
{
    private static FastHashTable* CurrentTranslations = null;
    private static SupportedLangs? CurrentLang = null;
    private static FastHashTable*[] AllTranslations = new FastHashTable*[4];
    // Test override provider for translation CSV (used by unit tests only)
    private static Func<Stream> TestTranslationStreamProvider = null;
    // Test override for current language (used by unit tests only)
    private static SupportedLangs? TestLanguageOverride = null;

    // 事前計算されたハッシュ値のキャッシュ
    private static readonly Dictionary<string, ulong> HashCache = new();

    public static void Load()
    {
        if (!ModHelpers.IsAndroid())
            LoadAllTranslations();
        UpdateCurrentTranslations();
    }

    private static void LoadAllTranslations()
    {
        // 各言語用のハッシュテーブルを作成
        for (int i = 0; i < 4; i++)
        {
            AllTranslations[i] = FastHashTable.Create(1024 * 1024); // 1MB初期容量
        }

        LoadTranslationData();
    }

    private static void LoadTranslationData()
    {
        using var stream = (TestTranslationStreamProvider != null)
            ? TestTranslationStreamProvider()
            : SuperNewRolesPlugin.Assembly.GetManifestResourceStream("SuperNewRoles.Resources.TranslationData.csv");

        // 全データを一度にメモリに読み込む
        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);

        fixed (byte* bufferPtr = buffer)
        {
            ParseCSVFast(bufferPtr, buffer.Length);
        }
    }

    private static void ParseCSVFast(byte* data, int length)
    {
        int pos = 0;
        int lineStart = 0;

        // BOMスキップ
        if (length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
        {
            pos = 3;
            lineStart = 3;
        }

        while (pos < length)
        {
            // 改行を探す
            while (pos < length && data[pos] != '\n' && data[pos] != '\r')
            {
                pos++;
            }

            if (pos > lineStart)
            {
                ParseLineFast(data + lineStart, pos - lineStart);
            }

            // 改行をスキップ
            while (pos < length && (data[pos] == '\n' || data[pos] == '\r'))
            {
                pos++;
            }

            lineStart = pos;
        }
    }

    private static void ParseLineFast(byte* line, int length)
    {
        // コメントまたは空行をスキップ
        if (length == 0 || line[0] == '#') return;

        // カンマ位置を高速検索
        int[] commaPos = new int[5];
        int commaCount = 0;

        for (int i = 0; i < length && commaCount < 5; i++)
        {
            if (line[i] == ',')
            {
                commaPos[commaCount++] = i;
            }
        }

        if (commaCount < 2) return;

        // キーを抽出
        string key = Encoding.UTF8.GetString(line, commaPos[0]).Trim();

        // 各言語の翻訳を抽出
        int start1 = commaPos[0] + 1;
        int len1 = commaPos[1] - start1;
        string japanese = ReplaceNewlines(Encoding.UTF8.GetString(line + start1, len1));

        int start2 = commaPos[1] + 1;
        int len2 = (commaCount > 2 ? commaPos[2] : length) - start2;
        string english = ReplaceNewlines(Encoding.UTF8.GetString(line + start2, len2));

        string sChinese = english;
        string tChinese = english;

        if (commaCount > 2)
        {
            int start3 = commaPos[2] + 1;
            int len3 = (commaCount > 3 ? commaPos[3] : length) - start3;
            sChinese = ReplaceNewlines(Encoding.UTF8.GetString(line + start3, len3));

            if (commaCount > 3)
            {
                int start4 = commaPos[3] + 1;
                int len4 = length - start4;
                tChinese = ReplaceNewlines(Encoding.UTF8.GetString(line + start4, len4));
            }
        }

        // ハッシュテーブルに追加
        AllTranslations[0]->Add(key, japanese);
        AllTranslations[1]->Add(key, english);
        AllTranslations[2]->Add(key, sChinese);
        AllTranslations[3]->Add(key, tChinese);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ReplaceNewlines(string text)
    {
        return text.Replace("\\n", "\n");
    }

    private static SupportedLangs GetCurrentSupportedLang()
    {
        // Prefer explicit test override (unit tests should not touch IL2CPP DataManager)
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
        var newLang = GetCurrentSupportedLang();
        if (CurrentLang == newLang) return;

        CurrentLang = newLang;

        // Androidはメモリ節約のため現在の翻訳しか保持しない
        if (ModHelpers.IsAndroid())
        {
            if (CurrentTranslations != null)
            {
                FastHashTable.DestroyTable(CurrentTranslations);
            }
            CurrentTranslations = FastHashTable.Create(1024 * 1024);
            LoadCurrentLanguageOnly();
        }
        else
        {
            switch (CurrentLang)
            {
                case SupportedLangs.Japanese:
                    CurrentTranslations = AllTranslations[0];
                    break;
                case SupportedLangs.English:
                    CurrentTranslations = AllTranslations[1];
                    break;
                case SupportedLangs.SChinese:
                    CurrentTranslations = AllTranslations[2];
                    break;
                case SupportedLangs.TChinese:
                    CurrentTranslations = AllTranslations[3];
                    break;
            }
        }
    }

    private static void LoadCurrentLanguageOnly()
    {
        using var stream = (TestTranslationStreamProvider != null)
            ? TestTranslationStreamProvider()
            : SuperNewRolesPlugin.Assembly.GetManifestResourceStream("SuperNewRoles.Resources.TranslationData.csv");
        using var reader = new StreamReader(stream);

        int langIndex = CurrentLang switch
        {
            SupportedLangs.Japanese => 1,
            SupportedLangs.English => 2,
            SupportedLangs.SChinese => 3,
            SupportedLangs.TChinese => 4,
            _ => 2
        };

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(',');
            if (parts.Length >= 3)
            {
                string key = parts[0].Trim();
                string value = parts.Length > langIndex ? parts[langIndex].Replace("\\n", "\n") : parts[2].Replace("\\n", "\n");
                CurrentTranslations->Add(key, value);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(string key)
    {
        if (CurrentTranslations == null) return key;
        return CurrentTranslations->Get(key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetString(string key, out string value)
    {
        if (CurrentTranslations == null)
        {
            value = null;
            return false;
        }

        return CurrentTranslations->TryGet(key, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(string key, params object[] format)
    {
        return string.Format(GetString(key), format);
    }

    public static void Cleanup()
    {
        if (ModHelpers.IsAndroid())
        {
            if (CurrentTranslations != null)
            {
                FastHashTable.DestroyTable(CurrentTranslations);
                CurrentTranslations = null;
            }
        }
        else
        {
            for (int i = 0; i < AllTranslations.Length; i++)
            {
                if (AllTranslations[i] != null)
                {
                    FastHashTable.DestroyTable(AllTranslations[i]);
                    AllTranslations[i] = null;
                }
            }
            // Also clear current pointer to avoid dangling references
            CurrentTranslations = null;
        }
        CurrentLang = null;
    }

    // --- Test helpers ---
    // Provide test CSV text to be used by Load() instead of the embedded resource.
    // Intended for unit tests to supply stable translation data.
    public static void SetTestTranslationCsv(string csv)
    {
        if (csv == null) throw new ArgumentNullException(nameof(csv));
        var bytes = Encoding.UTF8.GetBytes(csv);
        TestTranslationStreamProvider = () => new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: true);
    }

    // Allow unit tests to set language without touching Among Us DataManager
    public static void SetTestLanguage(SupportedLangs lang)
    {
        TestLanguageOverride = lang;
    }

    // Clear the test CSV provider and return to embedded resource loading.
    public static void ClearTestTranslationCsv()
    {
        TestTranslationStreamProvider = null;
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
