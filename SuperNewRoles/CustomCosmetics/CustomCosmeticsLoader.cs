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

namespace SuperNewRoles.CustomCosmetics;

public class CustomCosmeticsData
{
    public Dictionary<string, string> assetbundles { get; set; }
}

public class CustomCosmeticsLoader
{
    public static string[] CustomCosmeticsURLs = [
        // "https://example.com/custom_cosmetics.json",
        "./SuperNewRolesNext/debug_assets.json",
    ];

    private static readonly HttpClient client = new();
    private static readonly int maxRetryAttempts = 1;
    private static readonly TimeSpan retryDelay = TimeSpan.FromSeconds(5);
    private static readonly MD5 md5 = MD5.Create();
    private static readonly List<string> notLoadedAssetBundles = new();
    private static readonly List<CustomCosmeticsPackage> loadedPackages = new();
    public static async void Load()
    {
        foreach (string url in CustomCosmeticsURLs)
        {
            try
            {
                string jsonContent;
                if (url.StartsWith("./") || url.StartsWith("../"))
                {
                    jsonContent = File.ReadAllText(url);
                }
                else
                {
                    jsonContent = await client.GetStringAsync(url);
                }

                // JSONをパース
                JObject json = JObject.Parse(jsonContent);
                JToken assetbundles = json["assetbundles"];
                if (assetbundles == null)
                {
                    Logger.Error($"assetbundlesが見つかりません: {url}");
                    continue;
                }
                // アセットバンドルをダウンロード
                if (assetbundles != null)
                {
                    for (var assetBundle = assetbundles.First; assetBundle != null; assetBundle = assetBundle.Next)
                    {
                        string assetBundleUrl = assetBundle["url"].ToString();
                        string exceptedHash = assetBundle["hash"].ToString();
                        string? savedPath = await DownloadAssetBundleWithRetry(assetBundleUrl, exceptedHash);
                        if (savedPath != null)
                        {
                            notLoadedAssetBundles.Add(savedPath);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {url}\nHTTPリクエストエラー: {e.StatusCode}\n{e}");
            }
            catch (System.Exception e)
            {
                Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {url}\n{e}");
            }
        }
        try
        {
            while (notLoadedAssetBundles.Count > 0)
            {
                AssetBundle assetBundle = LoadAssetBundle(notLoadedAssetBundles[0]);
                string[] packages = GetPackages(assetBundle);
                foreach (string package in packages)
                {
                    Logger.Info($"パッケージ: {package}");
                }
                LoadPackages(assetBundle, packages);
                notLoadedAssetBundles.RemoveAt(0);
            }
        }
        catch (System.Exception e)
        {
            Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {e}");
        }
    }
    private static void LoadPackages(AssetBundle assetBundle, string[] packages)
    {
        foreach (string package in packages)
        {
            string packageJson = assetBundle.LoadAsset<TextAsset>($"assets/hats/{package}/package.json").text;
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
            JToken hats = packageJsonObject["hats"];
            if (hats == null)
            {
                Logger.Error($"パッケージ: {package} に hats が見つかりません");
                continue;
            }
            List<CustomCosmeticsHat> customCosmeticsHats = [];
            for (var hat = hats.First; hat != null; hat = hat.Next)
            {
                CustomCosmeticsHat customCosmeticsHat = new(
                    name: hat["name"].ToString(),
                    name_en: hat["name_en"]?.ToString(),
                    hat_id: hat["hat_id"].ToString(),
                    path_base: $"Assets/Hats/{package}/{hat["hat_id"].ToString()}/",
                    author: hat["author"].ToString(),
                    package: cosmeticsPackage,
                    options: new(hat)
                );
                customCosmeticsHats.Add(customCosmeticsHat);
            }
            cosmeticsPackage.hats = customCosmeticsHats.ToArray();
        }
    }
    private static string[] GetPackages(AssetBundle assetBundle)
    {
        Logger.Info($"---アセットバンドル: {assetBundle.name}---");
        foreach (string assetName in assetBundle.GetAllAssetNames())
        {
            Logger.Info($"アセット: {assetName}");
        }
        Logger.Info("--------------------------------");
        return assetBundle.GetAllAssetNames()
            .Select(path => path.Split('/'))
            .Where(segments => segments.Length >= 4 &&
                               string.Equals(segments[0], "Assets", StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(segments[1], "Hats", StringComparison.OrdinalIgnoreCase) &&
                               !string.IsNullOrEmpty(segments[2]))
            .Select(segments => segments[2])
            .Distinct()
            .ToArray();
    }
    private static AssetBundle LoadAssetBundle(string assetBundlePath)
    {
        AssetBundle assetBundle3 = AssetBundle.LoadFromFile(assetBundlePath);
        assetBundle3.DontUnload();
        Logger.Info($"アセットバンドルをロードしました: {assetBundlePath} {assetBundle3 != null}");
        return assetBundle3;
    }

    private static async Task<string?> DownloadAssetBundleWithRetry(string assetBundleUrl, string exceptedHash)
    {
        string fileName = Path.GetFileName(assetBundleUrl);
        string savedPath = $"./SuperNewRolesNext/CustomCosmetics/{fileName}/{exceptedHash}.bundle";
        if (File.Exists(savedPath))
        {
            Logger.Info($"アセットバンドルが既に存在します: {savedPath}");
            return savedPath;
        }
        for (int i = 0; i < maxRetryAttempts; ++i)
        {
            try
            {
                byte[] assetBundleData;
                if (assetBundleUrl.StartsWith("./") || assetBundleUrl.StartsWith("../"))
                {
                    assetBundleData = File.ReadAllBytes(assetBundleUrl);
                }
                else
                {
                    assetBundleData = await client.GetByteArrayAsync(assetBundleUrl);
                }

                // md5を計算
                string fileHash = BitConverter.ToString(md5.ComputeHash(assetBundleData)).Replace("-", "").ToLower();
                string savePath = $"./SuperNewRolesNext/CustomCosmetics/{fileName}/{fileHash}.bundle";

                // ディレクトリを作る
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                foreach (string file in Directory.GetFiles(Path.GetDirectoryName(savePath)))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error($"アセットバンドルの削除に失敗しました: {file}\n{e}");
                    }
                }

                // アセットバンドルを保存
                File.WriteAllBytes(savePath, assetBundleData);
                Logger.Info($"アセットバンドルをダウンロードしました: {assetBundleUrl} -> {savePath}");
                return savePath; // 成功したらリトライしない
            }
            catch (HttpRequestException e)
            {
                Logger.Error($"アセットバンドルのダウンロードに失敗しました: {assetBundleUrl}\nHTTPリクエストエラー: {e.StatusCode}\n{e}");
            }
            catch (System.Exception e)
            {
                Logger.Error($"アセットバンドルのダウンロードに失敗しました: {assetBundleUrl}\n{e}");
            }

            Logger.Warning($"リトライします ({i + 1}/{maxRetryAttempts}) ... URL: {assetBundleUrl}");
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
        CustomCosmeticsLoader.Load();
    }
}