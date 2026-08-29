using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SuperNewRoles.Modules;

namespace SuperNewRoles.RequestInGame;

public record RequestInGameDraft(string Title, string Description, string Map, string Role, string Timing)
{
    public static RequestInGameDraft Empty => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public bool IsEmpty()
    {
        return
        string.IsNullOrEmpty(Title) &&
        string.IsNullOrEmpty(Description) &&
        string.IsNullOrEmpty(Map) &&
        string.IsNullOrEmpty(Role) &&
        string.IsNullOrEmpty(Timing);
    }
}

// 報告画面の下書き。入力中はメモリキャッシュを更新し、ディスクは遅延して書く。
// 誤って閉じても再オープン時に Restore できるよう、最終的にはファイルへ残す。
public static class RequestInGameDraftStore
{
    private const string SaveFileName = "RequestInGameDrafts.json";
    // 連打入力で毎回 I/O しないよう、最後の変更から 1.5 秒待ってからディスクへ書く。
    internal const int DiskWriteDebounceMilliseconds = 1500;
    private static string testSaveFilePath;
    // Sync はキャッシュと世代、DiskLock は実ファイル書き込みの直列化。
    private static readonly object Sync = new();
    private static readonly object DiskLock = new();
    // Load/Save はディスクを待たず、このキャッシュを読む。
    private static Dictionary<string, RequestInGameDraft> cachedDrafts;
    private static bool cacheLoaded;
    private static CancellationTokenSource debounceCts;
    private static Task pendingWrite = Task.CompletedTask;
    // Flush 後に、以前から飛んでいた非同期書き込みが古い内容で上書きしないための世代。
    private static int writeGeneration;
    private static int testDiskWriteHoldMilliseconds;
    private static ManualResetEventSlim testDiskWriteHoldStarted;

    // 誤って閉じたあとの再オープン時は、ここから下書きを復元する。
    public static RequestInGameDraft Load(RequestInGameType requestInGameType)
    {
        lock (Sync)
        {
            EnsureCacheLoadedNoLock();
            return cachedDrafts.TryGetValue(GetDraftKey(requestInGameType), out RequestInGameDraft draft)
                ? Normalize(draft)
                : RequestInGameDraft.Empty;
        }
    }

    public static void Save(RequestInGameType requestInGameType, RequestInGameDraft draft)
    {
        draft = Normalize(draft);
        // 空下書きはファイルに残さない（Clear 経由でディスクからも消す）。
        if (draft.IsEmpty())
        {
            Clear(requestInGameType);
            return;
        }

        lock (Sync)
        {
            EnsureCacheLoadedNoLock();
            cachedDrafts[GetDraftKey(requestInGameType)] = draft;
        }
        ScheduleDebouncedWrite();
    }

    public static void Clear(RequestInGameType requestInGameType)
    {
        bool removed;
        lock (Sync)
        {
            EnsureCacheLoadedNoLock();
            removed = cachedDrafts.Remove(GetDraftKey(requestInGameType));
        }
        if (!removed)
            return;

        ScheduleDebouncedWrite();
    }

    // debounce を打ち切り、同期的にディスクへ書く。
    // AutoSaver が OnDestroy / 一時停止 / フォーカス喪失時に呼ぶ。
    public static void Flush()
    {
        CancelDebounce();
        WaitPendingWrite();
        WriteSnapshotToDisk();
    }

    public static void SetTestSaveFilePath(string saveFilePath)
    {
        CancelDebounce();
        WaitPendingWrite();
        lock (Sync)
        {
            testSaveFilePath = saveFilePath;
            testDiskWriteHoldMilliseconds = 0;
            testDiskWriteHoldStarted?.Reset();
            ResetCacheNoLock();
        }
    }

    public static void ClearTestSaveFilePath()
    {
        CancelDebounce();
        WaitPendingWrite();
        lock (Sync)
        {
            testSaveFilePath = null;
            testDiskWriteHoldMilliseconds = 0;
            testDiskWriteHoldStarted?.Reset();
            ResetCacheNoLock();
        }
    }

    public static void SetTestDiskWriteHoldMilliseconds(int milliseconds)
    {
        lock (Sync)
        {
            testDiskWriteHoldStarted ??= new ManualResetEventSlim(false);
            testDiskWriteHoldStarted.Reset();
            testDiskWriteHoldMilliseconds = milliseconds;
        }
    }

    public static bool WaitForTestDiskWriteHold(int timeoutMilliseconds)
    {
        ManualResetEventSlim holdStarted = testDiskWriteHoldStarted;
        return holdStarted != null && holdStarted.Wait(timeoutMilliseconds);
    }

    private static string SaveFilePath =>
        testSaveFilePath ?? Path.Combine(SuperNewRolesPlugin.BaseDirectory, "SaveData", SaveFileName);

    private static string GetDraftKey(RequestInGameType requestInGameType)
    {
        return requestInGameType.ToString();
    }

    private static void EnsureCacheLoadedNoLock()
    {
        if (cacheLoaded)
            return;

        cachedDrafts = LoadAllFromDisk();
        cacheLoaded = true;
    }

    private static void ResetCacheNoLock()
    {
        cachedDrafts = null;
        cacheLoaded = false;
    }

    private static Dictionary<string, RequestInGameDraft> LoadAllFromDisk()
    {
        string saveFilePath = SaveFilePath;
        if (!File.Exists(saveFilePath))
            return new Dictionary<string, RequestInGameDraft>();

        try
        {
            string json = File.ReadAllText(saveFilePath);
            if (JsonParser.Parse(json) is not Dictionary<string, object> parsed)
                return new Dictionary<string, RequestInGameDraft>();

            Dictionary<string, RequestInGameDraft> drafts = new();
            foreach (var pair in parsed)
            {
                if (pair.Value is Dictionary<string, object> draftDict)
                    drafts[pair.Key] = ParseDraft(draftDict);
            }
            return drafts;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonParseException)
        {
            return new Dictionary<string, RequestInGameDraft>();
        }
    }

    // 直前の遅延書き込みをキャンセルし、1.5 秒後にスナップショットを非同期で書く。
    private static void ScheduleDebouncedWrite()
    {
        CancellationTokenSource cts = new();
        CancellationTokenSource previous = Interlocked.Exchange(ref debounceCts, cts);
        try
        {
            previous?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        previous?.Dispose();

        CancellationToken token = cts.Token;
        pendingWrite = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DiskWriteDebounceMilliseconds, token);
                WriteSnapshotToDisk();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                // 保存失敗しても報告 UI は動かし続ける（下書き永続化は best-effort）。
            }
        });
    }

    private static void CancelDebounce()
    {
        CancellationTokenSource cts = Interlocked.Exchange(ref debounceCts, null);
        try
        {
            cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        cts?.Dispose();
    }

    // 進行中の非同期書き込みが終わるまで最大 2 秒待つ。
    private static void WaitPendingWrite()
    {
        Task write = pendingWrite;
        if (write == null || write.IsCompleted)
            return;

        try
        {
            write.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
        }
    }

    private static void WriteSnapshotToDisk()
    {
        Dictionary<string, RequestInGameDraft> snapshot;
        string saveFilePath;
        int generation;
        lock (Sync)
        {
            EnsureCacheLoadedNoLock();
            snapshot = new Dictionary<string, RequestInGameDraft>(cachedDrafts);
            saveFilePath = SaveFilePath;
            // 開始時に世代を進める。実書き込み直前に再確認し、古い in-flight 書き込みは捨てる。
            generation = ++writeGeneration;
        }

        // テスト用: 古い書き込みが Flush より後に終わる競合を再現する。
        int holdMilliseconds = Interlocked.Exchange(ref testDiskWriteHoldMilliseconds, 0);
        if (holdMilliseconds > 0)
        {
            testDiskWriteHoldStarted?.Set();
            Thread.Sleep(holdMilliseconds);
        }

        lock (DiskLock)
        {
            bool stale;
            lock (Sync)
                stale = generation != writeGeneration;
            if (stale)
                return;

            SaveAllToPath(saveFilePath, snapshot);
        }
    }

    private static void SaveAllToPath(string saveFilePath, Dictionary<string, RequestInGameDraft> drafts)
    {
        try
        {
            string directory = Path.GetDirectoryName(saveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (drafts.Count == 0)
            {
                // 全下書きが空なら本体も .tmp も削除する。
                if (File.Exists(saveFilePath))
                    File.Delete(saveFilePath);
                string leftoverTempPath = saveFilePath + ".tmp";
                if (File.Exists(leftoverTempPath))
                    File.Delete(leftoverTempPath);
                return;
            }

            string json = JsonParser.Serialize(SerializeDrafts(drafts));
            WriteAllTextAtomic(saveFilePath, json);
        }
        catch (Exception)
        {
            // 保存失敗しても報告 UI は動かし続ける（下書き永続化は best-effort）。
        }
    }

    // .tmp に書き切ってから同一ディレクトリ内で置換する。既存ファイルを先に削除しない。
    private static void WriteAllTextAtomic(string saveFilePath, string json)
    {
        string tempPath = saveFilePath + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, saveFilePath, overwrite: true);
    }

    private static RequestInGameDraft Normalize(RequestInGameDraft draft)
    {
        if (draft == null)
            return RequestInGameDraft.Empty;

        return new RequestInGameDraft(
            draft.Title ?? string.Empty,
            draft.Description ?? string.Empty,
            draft.Map ?? string.Empty,
            draft.Role ?? string.Empty,
            draft.Timing ?? string.Empty);
    }

    private static Dictionary<string, object> SerializeDrafts(Dictionary<string, RequestInGameDraft> drafts)
    {
        Dictionary<string, object> serializedDrafts = new();
        foreach (var pair in drafts)
            serializedDrafts[pair.Key] = SerializeDraft(pair.Value);
        return serializedDrafts;
    }

    private static Dictionary<string, object> SerializeDraft(RequestInGameDraft draft)
    {
        draft = Normalize(draft);
        return new Dictionary<string, object>
        {
            ["Title"] = draft.Title,
            ["Description"] = draft.Description,
            ["Map"] = draft.Map,
            ["Role"] = draft.Role,
            ["Timing"] = draft.Timing
        };
    }

    private static RequestInGameDraft ParseDraft(Dictionary<string, object> draft)
    {
        return Normalize(new RequestInGameDraft(
            GetString(draft, "Title"),
            GetString(draft, "Description"),
            GetString(draft, "Map"),
            GetString(draft, "Role"),
            GetString(draft, "Timing")));
    }

    private static string GetString(Dictionary<string, object> dict, string key)
    {
        return dict.TryGetValue(key, out object value) && value is string stringValue
            ? stringValue
            : string.Empty;
    }
}
