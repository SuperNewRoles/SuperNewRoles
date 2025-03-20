using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomCosmetics.UI;

namespace SuperNewRoles.CustomCosmetics;
public class CustomCosmeticsData
{
    public Dictionary<string, string> assetbundles { get; set; }
}

public class CustomCosmeticsLoader
{
    // 配信元URLの初期化（必要に応じて他のURLも追加可能）
    public static string[] CustomCosmeticsURLs = new string[]
    {
            // "https://example.com/custom_cosmetics.json",
            // "./SuperNewRolesNext/debug_assets.json",
    };
    public const string ModdedPrefix = "Modded_";
    private static readonly HttpClient client = new();
    private static readonly int maxRetryAttempts = 1;
    private static readonly TimeSpan retryDelay = TimeSpan.FromSeconds(5);
    private static readonly MD5 md5 = MD5.Create();
    private static readonly List<string> notLoadedAssetBundles = new();
    private static readonly List<CustomCosmeticsPackage> loadedPackages = new();
    public static List<CustomCosmeticsPackage> LoadedPackages => loadedPackages;
    private static readonly Dictionary<string, CustomCosmeticsHat> moddedHats = new();
    private static readonly Dictionary<string, CustomCosmeticsVisor> moddedVisors = new();
    public static async Task Load()
    {
        foreach (string url in CustomCosmeticsURLs)
        {
            try
            {
                string jsonContent = url.StartsWith("./") || url.StartsWith("../")
                    ? File.ReadAllText(url)
                    : await client.GetStringAsync(url);

                // JSONをパース
                JObject json = JObject.Parse(jsonContent);
                JToken assetBundlesToken = json["assetbundles"];
                if (assetBundlesToken == null)
                {
                    Logger.Error($"assetbundlesが見つかりません: {url}");
                    continue;
                }

                // 各assetbundle要素についてダウンロードを試行
                for (var assetBundle = assetBundlesToken.First; assetBundle != null; assetBundle = assetBundle.Next)
                {
                    string assetBundleUrl = assetBundle["url"]?.ToString() ?? "";
                    string expectedHash = assetBundle["hash"]?.ToString() ?? "";
                    string? savedPath = await DownloadAssetBundleWithRetry(assetBundleUrl, expectedHash);
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        notLoadedAssetBundles.Add(savedPath);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {url}\nHTTPリクエストエラー: {e.StatusCode}\n{e}");
            }
            catch (Exception e)
            {
                Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {url}\n{e}");
            }
        }

        // ダウンロードされたアセットバンドルを順次読み込みパッケージを取り出す
        try
        {
            while (notLoadedAssetBundles.Count > 0)
            {
                string bundlePath = notLoadedAssetBundles[0];
                AssetBundle assetBundle = LoadAssetBundle(bundlePath);
                string[] packagesHats = GetPackages(assetBundle, "Hats");
                string[] packagesVisors = GetPackages(assetBundle, "Visors");
                foreach (string package in packagesHats)
                {
                    Logger.Info($"パッケージ: {package}");
                }
                LoadPackages(assetBundle, packagesHats, packagesVisors);
                notLoadedAssetBundles.RemoveAt(0);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {e}");
        }
    }

    private static void LoadPackages(AssetBundle assetBundle, string[] packagesHats, string[] packagesVisors)
    {
        foreach (string package in packagesHats)
        {
            // package.jsonをロードするパスを組み立て、読み込み
            string packageJsonPath = $"assets/hats/{package}/package.json";
            TextAsset packageTextAsset = assetBundle.LoadAsset<TextAsset>(packageJsonPath);
            if (packageTextAsset == null)
            {
                Logger.Error($"パッケージ: {package} の package.json が読み込めません(Hats)");
            }
            string packageJson = packageTextAsset.text;
            JObject packageJsonObject = JObject.Parse(packageJson);
            JToken packageInfo = packageJsonObject["package"];
            if (packageInfo == null)
            {
                Logger.Error($"パッケージ: {package} に package が見つかりません");
                continue;
            }
            CustomCosmeticsPackage cosmeticsPackage = new(
                packageInfo["name"].ToString(),
                packageInfo["name_en"]?.ToString(),
                (int)packageInfo["parseversion"]
            );
            loadedPackages.Add(cosmeticsPackage);
            Logger.Info($"{cosmeticsPackage.name} {cosmeticsPackage.version}");

            JToken hatsToken = packageJsonObject["hats"];
            if (hatsToken != null)
            {
                List<CustomCosmeticsHat> customCosmeticsHats = new();
                for (var hat = hatsToken.First; hat != null; hat = hat.Next)
                {
                    CustomCosmeticsHat customCosmeticsHat = new(
                        name: hat["name"].ToString(),
                        name_en: hat["name_en"]?.ToString(),
                        hat_id: hat["hat_id"].ToString(),
                        path_base: $"Assets/Hats/{package}/{hat["hat_id"].ToString()}/",
                        author: hat["author"].ToString(),
                        package: cosmeticsPackage,
                        options: new(hat),
                        assetBundle: assetBundle
                    );
                    customCosmeticsHats.Add(customCosmeticsHat);
                    moddedHats.Add(customCosmeticsHat.ProdId, customCosmeticsHat);
                }
                cosmeticsPackage.hats = customCosmeticsHats.ToArray();
            }
            else
                Logger.Error($"パッケージ: {package} に hats が見つかりません");
        }
        foreach (string package in packagesVisors)
        {
            string visorsPath = $"assets/visors/{package}/package.json";
            TextAsset visorsTextAsset = assetBundle.LoadAsset<TextAsset>(visorsPath);
            if (visorsTextAsset == null)
            {
                Logger.Error($"パッケージ: {package} の package.json が読み込めません(Visors)");
                continue;
            }
            // バイザーの読み込み
            JObject packageJsonObject = JObject.Parse(visorsTextAsset.text);
            JToken packageInfo = packageJsonObject["package"];
            if (packageInfo == null)
            {
                Logger.Error($"パッケージ: {package} に package が見つかりません");
                continue;
            }
            CustomCosmeticsPackage cosmeticsPackage = loadedPackages.FirstOrDefault(p => p.name == packageInfo["name"].ToString() && p.name_en == packageInfo["name_en"]?.ToString() && p.version == (int)packageInfo["parseversion"]) ?? new(
                packageInfo["name"].ToString(),
                packageInfo["name_en"]?.ToString(),
                (int)packageInfo["parseversion"]
            );
            loadedPackages.Add(cosmeticsPackage);
            Logger.Info($"{cosmeticsPackage.name} {cosmeticsPackage.version}");

            JToken visorsToken = packageJsonObject["visors"];
            if (visorsToken != null)
            {
                List<CustomCosmeticsVisor> customCosmeticsVisors = new();
                for (var visor = visorsToken.First; visor != null; visor = visor.Next)
                {
                    CustomCosmeticsVisor customCosmeticsVisor = new(
                        name: visor["name"].ToString(),
                        name_en: visor["name_en"]?.ToString(),
                        visor_id: visor["visor_id"].ToString(),
                        path_base: $"Assets/Visors/{package}/{visor["visor_id"].ToString()}/",
                        author: visor["author"].ToString(),
                        package: cosmeticsPackage,
                        options: new(visor),
                        assetBundle: assetBundle
                    );
                    customCosmeticsVisors.Add(customCosmeticsVisor);
                    moddedVisors.Add(customCosmeticsVisor.ProdId, customCosmeticsVisor);
                }
                cosmeticsPackage.visors = customCosmeticsVisors.ToArray();
            }
            else
            {
                cosmeticsPackage.visors = [];
            }
        }
    }
    public static CustomCosmeticsHat? GetModdedHat(string hatId)
    {
        return moddedHats.TryGetValue(hatId, out var hat) ? hat : null;
    }
    public static ICosmeticData? GetModdedHatData(string hatId)
    {
        return moddedHats.TryGetValue(hatId, out var hat) ? new ModdedHatDataWrapper(hat) : null;
    }

    public static CustomCosmeticsVisor? GetModdedVisor(string visorId)
    {
        return moddedVisors.TryGetValue(visorId, out var visor) ? visor : null;
    }
    public static ICosmeticData? GetModdedVisorData(string visorId)
    {
        return moddedVisors.TryGetValue(visorId, out var visor) ? new ModdedVisorDataWrapper(visor) : null;
    }

    private static string[] GetPackages(AssetBundle assetBundle, string type)
    {
        return assetBundle.GetAllAssetNames()
            .Select(path => path.Split('/'))
            .Where(segments => segments.Length >= 4 &&
                               string.Equals(segments[0], "Assets", StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(segments[1], type, StringComparison.OrdinalIgnoreCase) &&
                               !string.IsNullOrEmpty(segments[2]))
            .Select(segments => segments[2])
            .Distinct()
            .ToArray();
    }

    private static AssetBundle LoadAssetBundle(string assetBundlePath)
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
        assetBundle.DontUnload();
        Logger.Info($"アセットバンドルをロードしました: {assetBundlePath} {assetBundle != null}");
        return assetBundle;
    }

    private static async Task<string?> DownloadAssetBundleWithRetry(string assetBundleUrl, string expectedHash)
    {
        string fileName = Path.GetFileName(assetBundleUrl);
        string targetPath = $"./SuperNewRolesNext/CustomCosmetics/{fileName}/{expectedHash}.bundle";
        if (File.Exists(targetPath))
        {
            Logger.Info($"アセットバンドルが既に存在します: {targetPath}");
            return targetPath;
        }

        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            try
            {
                byte[] assetBundleData = assetBundleUrl.StartsWith("./") || assetBundleUrl.StartsWith("../")
                    ? File.ReadAllBytes(assetBundleUrl)
                    : await client.GetByteArrayAsync(assetBundleUrl);

                // MD5ハッシュを計算
                string fileHash = BitConverter.ToString(md5.ComputeHash(assetBundleData))
                    .Replace("-", "")
                    .ToLower();
                string savePath = $"./SuperNewRolesNext/CustomCosmetics/{fileName}/{fileHash}.bundle";

                // ディレクトリ作成と古いファイルの削除
                string directory = Path.GetDirectoryName(savePath) ?? "";
                Directory.CreateDirectory(directory);
                foreach (string file in Directory.GetFiles(directory))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"アセットバンドルの削除に失敗しました: {file}\n{ex}");
                    }
                }

                // アセットバンドルの保存
                File.WriteAllBytes(savePath, assetBundleData);
                Logger.Info($"アセットバンドルをダウンロードしました: {assetBundleUrl} -> {savePath}");
                return savePath;
            }
            catch (HttpRequestException e)
            {
                Logger.Error($"アセットバンドルのダウンロードに失敗しました: {assetBundleUrl}\nHTTPリクエストエラー: {e.StatusCode}\n{e}");
            }
            catch (Exception e)
            {
                Logger.Error($"アセットバンドルのダウンロードに失敗しました: {assetBundleUrl}\n{e}");
            }

            Logger.Warning($"リトライします ({attempt + 1}/{maxRetryAttempts}) ... URL: {assetBundleUrl}");
            await Task.Delay(retryDelay);
        }

        Logger.Error($"アセットバンドルのダウンロードに最大リトライ回数({maxRetryAttempts})を超えました: {assetBundleUrl}");
        return null;
    }
}

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
public static class SplashManagerStartPatch
{
    public static void Postfix(SplashManager __instance)
    {
        // メインスレッドで非同期処理を開始
        _ = LoadCosmeticsAsync();
        Logger.Info("SplashManagerStartPatch");
    }

    // Unityのメインスレッドで安全に非同期処理を実行するためのメソッド
    private static async Task LoadCosmeticsAsync()
    {
        try
        {
            await CustomCosmeticsLoader.Load();
        }
        catch (Exception e)
        {
            Logger.Error($"カスタムコスメティックのロード中にエラーが発生しました: {e}");
        }
    }
}