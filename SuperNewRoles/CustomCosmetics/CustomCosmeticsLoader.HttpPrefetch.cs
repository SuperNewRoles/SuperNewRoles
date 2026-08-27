using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SuperNewRoles;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics;

public partial class CustomCosmeticsLoader
{
    private static readonly ConcurrentDictionary<string, string> prefetchedJsonByUrl = new(StringComparer.Ordinal);
    private static int httpPrefetchGate;

    public static Task HttpPrefetchTask { get; private set; }

    public readonly struct CosmeticsDownloadJob
    {
        public CosmeticsDownloadJob(string url, string targetPath, bool isAssetBundle, string expectedHash)
        {
            Url = url;
            TargetPath = targetPath;
            IsAssetBundle = isAssetBundle;
            ExpectedHash = expectedHash ?? "";
        }

        public string Url { get; }
        public string TargetPath { get; }
        public bool IsAssetBundle { get; }
        public string ExpectedHash { get; }
    }

    /// <summary>
    /// BepInEx Load 中に HTTP→ディスクだけを開始する。AssetBundle.LoadFromFile は呼ばない。
    /// </summary>
    public static void TryStartHttpPrefetch()
    {
        if (ConfigRoles.IsModCosmeticsAreNotLoaded == null)
            return;

        NetworkReachability reachability = NetworkReachability.ReachableViaLocalAreaNetwork;
        try
        {
            reachability = Application.internetReachability;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Cosmetics HTTP prefetch: internetReachability unavailable, try anyway. {ex.Message}");
        }

        bool canUseData = ConfigRoles.CanUseDataConnection == null || ConfigRoles.CanUseDataConnection.Value;
        if (!ShouldStartHttpPrefetch(ConfigRoles.IsModCosmeticsAreNotLoaded.Value, reachability, canUseData))
        {
            Logger.Info("カスタムコスメティックの HTTP プリフェッチをスキップします。");
            return;
        }

        _ = CosmeticsHttp.IsPooledClientAvailable;

        if (Interlocked.CompareExchange(ref httpPrefetchGate, 1, 0) != 0)
            return;

        DeleteCacheIfRequested();
        bool isAndroid = ModHelpers.IsAndroid();
        HttpPrefetchTask = Task.Run(() => PrefetchHttpDownloadsAsync(isAndroid));
        Logger.Info("カスタムコスメティックの HTTP プリフェッチを開始しました。");
    }

    public static bool ShouldStartHttpPrefetch(bool cosmeticsNotLoaded, NetworkReachability reachability, bool canUseDataConnection)
    {
        if (cosmeticsNotLoaded)
            return false;
        if (reachability == NetworkReachability.ReachableViaCarrierDataNetwork && !canUseDataConnection)
            return false;
        return true;
    }

    public static bool TryAdvanceEnumerator(IEnumerator enumerator, out object current)
    {
        current = null;
        try
        {
            if (!enumerator.MoveNext())
                return false;
            current = enumerator.Current;
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Enumerator pump failed: {ex}");
            return false;
        }
    }

    public static IEnumerator CoPumpEnumerator(IEnumerator enumerator)
    {
        while (TryAdvanceEnumerator(enumerator, out object current))
            yield return current;
    }

    internal static bool TryTakePrefetchedJson(string url, out string json)
        => prefetchedJsonByUrl.TryRemove(url, out json) && !string.IsNullOrEmpty(json);

    internal static void StorePrefetchedJson(string url, string json)
    {
        if (!string.IsNullOrEmpty(url) && json != null)
            prefetchedJsonByUrl[url] = json;
    }

    public static List<CosmeticsDownloadJob> CollectDownloadJobs(string sourceUrl, string jsonContent, bool isAndroid)
    {
        if (string.IsNullOrEmpty(jsonContent))
            return new List<CosmeticsDownloadJob>();

        return CollectDownloadJobs(sourceUrl, CustomCosmeticsJsonNode.Parse(jsonContent), isAndroid);
    }

    public static List<CosmeticsDownloadJob> CollectDownloadJobs(string sourceUrl, CustomCosmeticsJsonNode json, bool isAndroid)
    {
        var jobs = new List<CosmeticsDownloadJob>();
        if (json == null)
            return jobs;

        CollectAssetBundleJobs(sourceUrl, json, isAndroid, jobs);
        CollectHatSpriteJobs(sourceUrl, json, jobs);
        CollectVisorSpriteJobs(sourceUrl, json, jobs);
        CollectNamePlateSpriteJobs(sourceUrl, json, jobs);
        return jobs;
    }

    public static bool IsLocalCosmeticsPath(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return url.StartsWith("./", StringComparison.Ordinal) ||
               url.StartsWith("../", StringComparison.Ordinal) ||
               Path.IsPathRooted(url) ||
               File.Exists(url);
    }

    private static string SanitizeCachePathComponent(string value, string fallback)
    {
        string sanitized = SanitizeFileName(value ?? string.Empty)
            .Replace("/", string.Empty)
            .Replace("\\", string.Empty);
        return string.IsNullOrWhiteSpace(sanitized) || sanitized == "." || sanitized == ".."
            ? fallback
            : sanitized;
    }

    private static string GetPackageStorageDirectory(string packageName)
    {
        return Path.Combine(CustomCosmeticsCacheDirectory, SanitizeCachePathComponent(packageName, "NONE_PACKAGE"));
    }

    internal static string GetSpritePathBase(string packageName, string spriteName)
    {
        string sanitizedSpriteName = SanitizeCachePathComponent(spriteName, "item");
        return Path.Combine(GetPackageStorageDirectory(packageName), $"{sanitizedSpriteName}_").Replace("\\", "/");
    }

    internal static bool IsCachePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            string cacheRoot = Path.GetFullPath(CustomCosmeticsCacheDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(cacheRoot, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static string GetAssetBundleStorageDirectory(string bundleUrl)
    {
        string fileNameFromUrl;
        if (Uri.TryCreate(bundleUrl, UriKind.Absolute, out Uri bundleUri) &&
            (bundleUri.Scheme == Uri.UriSchemeHttp || bundleUri.Scheme == Uri.UriSchemeHttps))
        {
            fileNameFromUrl = Path.GetFileName(bundleUri.LocalPath);
        }
        else
        {
            fileNameFromUrl = Path.GetFileName(bundleUrl ?? string.Empty);
        }

        fileNameFromUrl = SanitizeCachePathComponent(fileNameFromUrl, "bundle");
        return Path.Combine(CustomCosmeticsCacheDirectory, fileNameFromUrl);
    }

    internal static string GetAssetBundleTargetPath(string bundleUrl, string expectedHash)
    {
        string safeExpectedHash = SanitizeCachePathComponent(expectedHash, "nohash");
        return Path.Combine(GetAssetBundleStorageDirectory(bundleUrl), $"{safeExpectedHash}.bundle");
    }

    internal static string GetSpriteTargetPath(string packageName, string spriteName)
    {
        string sanitizedSpriteName = SanitizeCachePathComponent(spriteName, "item");
        return Path.Combine(GetPackageStorageDirectory(packageName), $"{sanitizedSpriteName}.png").Replace("\\", "/");
    }

    internal static string ComputeMd5Hex(byte[] data)
    {
        using var md5 = MD5.Create();
        return BitConverter.ToString(md5.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
    }

    internal static void LogHashMismatchIfNeeded(string url, string expectedHash, byte[] data)
    {
        if (string.IsNullOrEmpty(expectedHash) || data == null)
            return;

        string actualHash = ComputeMd5Hex(data);
        if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
            Logger.Error($"ハッシュミスマッチ。URL: {url}, Expected: {expectedHash}, Actual: {actualHash}");
    }

    private static async Task PrefetchHttpDownloadsAsync(bool isAndroid)
    {
        try
        {
            int prefetchConcurrency = Math.Max(1, Math.Min(MAX_CONCURRENT_DOWNLOADS, CosmeticsHttp.BufferedResponseMaxConcurrentDownloads));
            using var throttle = new SemaphoreSlim(prefetchConcurrency);
            var jsonByUrl = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            var jsonTasks = new List<Task>();

            foreach (string url in CustomCosmeticsURLs)
            {
                string capturedUrl = url;
                jsonTasks.Add(Task.Run(async () =>
                {
                    await throttle.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        string content = await FetchMetadataAsync(capturedUrl).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(content))
                        {
                            jsonByUrl[capturedUrl] = content;
                            StorePrefetchedJson(capturedUrl, content);
                        }
                    }
                    finally
                    {
                        throttle.Release();
                    }
                }));
            }

            await Task.WhenAll(jsonTasks).ConfigureAwait(false);

            var jobs = new List<CosmeticsDownloadJob>();
            var seenSourceUrls = new HashSet<string>(StringComparer.Ordinal);
            foreach (string sourceUrl in CustomCosmeticsURLs)
            {
                if (!seenSourceUrls.Add(sourceUrl))
                    continue;
                if (jsonByUrl.TryGetValue(sourceUrl, out string content))
                    jobs.AddRange(CollectDownloadJobs(sourceUrl, content, isAndroid));
            }

            // 同じ target path のジョブを一つにまとめ、同一ファイルへの同時書き込みを防ぐ。
            var uniqueJobs = new List<CosmeticsDownloadJob>(jobs.Count);
            var seenTargetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (CosmeticsDownloadJob job in jobs)
            {
                if (string.IsNullOrEmpty(job.Url) || string.IsNullOrEmpty(job.TargetPath))
                    continue;
                if (!IsCachePath(job.TargetPath))
                {
                    Logger.Warning($"Prefetch target path is outside the cosmetics cache and was skipped: {job.TargetPath}");
                    continue;
                }
                if (File.Exists(job.TargetPath))
                    continue;

                string normalizedTargetPath;
                try
                {
                    normalizedTargetPath = Path.GetFullPath(job.TargetPath);
                }
                catch (Exception)
                {
                    continue;
                }

                if (seenTargetPaths.Add(normalizedTargetPath))
                    uniqueJobs.Add(job);
            }

            await DownloadJobsWithWorkersAsync(uniqueJobs, prefetchConcurrency).ConfigureAwait(false);
            CleanupStaleAssetBundleCache(jobs);

            Logger.Info($"Cosmetics HTTP prefetch finished. json={jsonByUrl.Count} jobs={jobs.Count} downloads={uniqueJobs.Count}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Cosmetics HTTP prefetch failed: {ex}");
        }
    }

    private static void CleanupStaleAssetBundleCache(IReadOnlyList<CosmeticsDownloadJob> jobs)
    {
        var expectedTargetsByDirectory = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (CosmeticsDownloadJob job in jobs)
        {
            if (!job.IsAssetBundle || string.IsNullOrEmpty(job.TargetPath) || !IsCachePath(job.TargetPath))
                continue;

            try
            {
                string targetPath = Path.GetFullPath(job.TargetPath);
                string directory = Path.GetDirectoryName(targetPath);
                if (string.IsNullOrEmpty(directory))
                    continue;

                if (!expectedTargetsByDirectory.TryGetValue(directory, out HashSet<string> expectedTargets))
                {
                    expectedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    expectedTargetsByDirectory[directory] = expectedTargets;
                }

                expectedTargets.Add(targetPath);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to resolve asset bundle cache path: {job.TargetPath}. {ex.Message}");
            }
        }

        foreach (var entry in expectedTargetsByDirectory)
        {
            if (!Directory.Exists(entry.Key))
                continue;

            // 新しい対象の取得に全て失敗した場合は、既存のキャッシュを残す。
            bool hasAvailableExpectedTarget = false;
            foreach (string expectedTarget in entry.Value)
            {
                if (File.Exists(expectedTarget))
                {
                    hasAvailableExpectedTarget = true;
                    break;
                }
            }
            if (!hasAvailableExpectedTarget)
                continue;

            string[] existingBundleFiles;
            try
            {
                existingBundleFiles = Directory.GetFiles(entry.Key, "*.bundle");
            }
            catch (Exception ex)
            {
                Logger.Warning($"アセットバンドル一覧の取得に失敗しました: {entry.Key}. {ex.Message}");
                continue;
            }

            foreach (string existingBundleFile in existingBundleFiles)
            {
                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(existingBundleFile);
                }
                catch (Exception)
                {
                    continue;
                }

                if (entry.Value.Contains(fullPath))
                    continue;

                try
                {
                    File.Delete(fullPath);
                    Logger.Info($"古い/不要なアセットバンドルを削除しました: {fullPath}");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"古いアセットバンドルの削除に失敗しました: {fullPath}. {ex.Message}");
                }
            }
        }
    }

    private static async Task DownloadJobsWithWorkersAsync(IReadOnlyList<CosmeticsDownloadJob> jobs, int workerCount)
    {
        if (jobs.Count == 0)
            return;

        int nextIndex = -1;
        int actualWorkerCount = Math.Min(Math.Max(1, workerCount), jobs.Count);
        var workers = new List<Task>(actualWorkerCount);
        for (int i = 0; i < actualWorkerCount; i++)
        {
            workers.Add(Task.Run(async () =>
            {
                while (true)
                {
                    int index = Interlocked.Increment(ref nextIndex);
                    if (index >= jobs.Count)
                        return;

                    await DownloadJobToDiskAsync(jobs[index]).ConfigureAwait(false);
                }
            }));
        }

        await Task.WhenAll(workers).ConfigureAwait(false);
    }

    private static async Task<string> FetchMetadataAsync(string url)
    {
        if (IsLocalCosmeticsPath(url))
            return File.Exists(url) ? File.ReadAllText(url) : null;

        CosmeticsHttpRequest request = CosmeticsHttpRequest.Get(url);
        request.timeout = MetadataRequestTimeoutSeconds;
        await request.SendAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(request.error) || request.downloadHandler?.text == null)
        {
            Logger.Error($"Prefetch metadata failed: {url} {request.error}");
            return null;
        }

        return request.downloadHandler.text;
    }

    private static async Task DownloadJobToDiskAsync(CosmeticsDownloadJob job)
    {
        try
        {
            if (string.IsNullOrEmpty(job.Url) || !IsCachePath(job.TargetPath))
                return;
            if (File.Exists(job.TargetPath))
                return;

            byte[] data;
            if (IsLocalCosmeticsPath(job.Url))
            {
                string localPath = Path.GetFullPath(job.Url);
                if (!File.Exists(localPath))
                    return;
                data = File.ReadAllBytes(localPath);
            }
            else
            {
                CosmeticsHttpRequest request = CosmeticsHttpRequest.Get(job.Url);
                request.timeout = job.IsAssetBundle ? AssetBundleRequestTimeoutSeconds : SpriteDownloadTimeoutSeconds;
                await request.SendAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(request.error) || request.downloadHandler?.data == null)
                {
                    Logger.Warning($"Prefetch download failed: {job.Url} {request.error}");
                    return;
                }

                data = request.downloadHandler.data;
            }

            if (data == null || data.Length == 0)
                return;

            if (job.IsAssetBundle)
                LogHashMismatchIfNeeded(job.Url, job.ExpectedHash, data);

            string directory = Path.GetDirectoryName(job.TargetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string tempPath = $"{job.TargetPath}.{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllBytes(tempPath, data);
                File.Move(tempPath, job.TargetPath, overwrite: true);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                }
            }

            Interlocked.Add(ref downloadReceivedBytes, data.Length);
        }
        catch (Exception ex)
        {
            Logger.Error($"Prefetch write failed: {job.Url}\n{ex}");
        }
    }

    private static void CollectAssetBundleJobs(string sourceUrl, CustomCosmeticsJsonNode json, bool isAndroid, List<CosmeticsDownloadJob> jobs)
    {
        CustomCosmeticsJsonNode assetBundlesToken = json["assetbundles"];
        if (assetBundlesToken == null)
            return;

        for (var assetBundle = assetBundlesToken.First; assetBundle != null; assetBundle = assetBundle.Next)
        {
            string assetBundleUrl = assetBundle["url"]?.ToString() ?? "";
            string assetBundleAndroidUrl = assetBundle["url_android"]?.ToString() ?? "";
            string expectedHash = assetBundle["hash"]?.ToString() ?? "";
            string expectedHashAndroid = assetBundle["hash_android"]?.ToString() ?? "";
            string currentUrl = isAndroid && !string.IsNullOrWhiteSpace(assetBundleAndroidUrl) ? assetBundleAndroidUrl : assetBundleUrl;
            string currentExpectedHash = isAndroid && !string.IsNullOrWhiteSpace(expectedHashAndroid) ? expectedHashAndroid : expectedHash;
            if (string.IsNullOrWhiteSpace(currentUrl))
            {
                Logger.Warning($"カスタムコスメティックのアセットバンドルURLが空のためスキップします: {sourceUrl}");
                continue;
            }

            string targetPath = GetAssetBundleTargetPath(currentUrl, currentExpectedHash);
            jobs.Add(new CosmeticsDownloadJob(currentUrl, targetPath, isAssetBundle: true, currentExpectedHash));
        }
    }

    private static void CollectHatSpriteJobs(string sourceUrl, CustomCosmeticsJsonNode json, List<CosmeticsDownloadJob> jobs)
    {
        CustomCosmeticsJsonNode hatsToken = json["hats"];
        if (hatsToken == null)
            return;

        for (var hat = hatsToken.First; hat != null; hat = hat.Next)
        {
            string packageName = hat["package"]?.ToString() ?? "NONE_PACKAGE";
            string hatName = hat["name"]?.ToString();
            if (string.IsNullOrEmpty(hatName))
                continue;

            AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_front", "hats/" + hat["resource"]?.ToString());
            if (hat["resourceleft"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_front_left", "hats/" + hat["resourceleft"]?.ToString());
            if (hat["backresource"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_back", "hats/" + hat["backresource"]?.ToString());
            if (hat["backresourceleft"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_back_left", "hats/" + hat["backresourceleft"]?.ToString());
            if (hat["backflipresource"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_backflip", "hats/" + hat["backflipresource"]?.ToString());
            if (hat["flipresource"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_flip", "hats/" + hat["flipresource"]?.ToString());
            if (hat["climbresource"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, hatName + "_climb", "hats/" + hat["climbresource"]?.ToString());
        }
    }

    private static void CollectVisorSpriteJobs(string sourceUrl, CustomCosmeticsJsonNode json, List<CosmeticsDownloadJob> jobs)
    {
        CustomCosmeticsJsonNode visorsToken = json["visors"] ?? json["Visors"];
        if (visorsToken == null)
            return;

        string folder = json["visors"] != null ? "visors/" : "Visors/";
        for (var visor = visorsToken.First; visor != null; visor = visor.Next)
        {
            string packageName = visor["package"]?.ToString() ?? "NONE_PACKAGE";
            string visorName = visor["name"]?.ToString();
            if (string.IsNullOrEmpty(visorName))
                continue;

            AddSpriteJob(jobs, sourceUrl, packageName, visorName + "_idle", folder + visor["resource"]?.ToString());
            if (visor["flipresource"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, visorName + "_flip", folder + visor["flipresource"]?.ToString());
            if (visor["climbresource"] != null)
                AddSpriteJob(jobs, sourceUrl, packageName, visorName + "_climb", folder + visor["climbresource"]?.ToString());
        }
    }

    private static void CollectNamePlateSpriteJobs(string sourceUrl, CustomCosmeticsJsonNode json, List<CosmeticsDownloadJob> jobs)
    {
        CustomCosmeticsJsonNode namePlatesToken = json["nameplates"];
        if (namePlatesToken == null)
            return;

        for (var namePlate = namePlatesToken.First; namePlate != null; namePlate = namePlate.Next)
        {
            string packageName = namePlate["package"]?.ToString() ?? "NONE_PACKAGE";
            string namePlateName = namePlate["name"]?.ToString();
            if (string.IsNullOrEmpty(namePlateName))
                continue;

            AddSpriteJob(jobs, sourceUrl, packageName, namePlateName + "_nameplate", "nameplates/" + namePlate["resource"]?.ToString());
        }
    }

    private static void AddSpriteJob(List<CosmeticsDownloadJob> jobs, string sourceUrl, string packageName, string spriteName, string relativePath)
    {
        string spriteUrl = getpath(sourceUrl, relativePath);
        if (string.IsNullOrEmpty(spriteUrl))
            return;

        string filePath = GetSpriteTargetPath(packageName, spriteName);
        jobs.Add(new CosmeticsDownloadJob(spriteUrl, filePath, isAssetBundle: false, expectedHash: ""));
    }
}
