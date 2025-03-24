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
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Threading;
using UnityEngine.ProBuilder;

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
            "./SuperNewRolesNext/debug_assets.json",
            "https://raw.githubusercontent.com/hinakkyu/TheOtherHats/refs/heads/master/CustomHats.json",
            "https://raw.githubusercontent.com/catudon1276/CatudonCostume/refs/heads/main/CustomHats.json",
            "https://raw.githubusercontent.com/catudon1276/Mememura-Hats/refs/heads/main/CustomHats.json",
            // "https://raw.githubusercontent.com/Ujet222/TOPHats/refs/heads/main/CustomHats.json",
            // "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/refs/heads/main/CustomHats.json",
            // "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/refs/heads/main/CustomVisors.json",
            "https://raw.githubusercontent.com/Ujet222/TOPVisors/refs/heads/main/CustomVisors.json",
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
    private static readonly Dictionary<string, List<(string, string)>> willDownloads = new();
    private static readonly Dictionary<string, byte[]> downloadedSprites = new();
    public static void Load()
    {
        foreach (string url in CustomCosmeticsURLs)
        {
            try
            {
                string jsonContent = url.StartsWith("./") || url.StartsWith("../")
                    ? File.ReadAllText(url)
                    : client.GetStringAsync(url).Result;

                // JSONをパース
                JObject json = JObject.Parse(jsonContent);
                JToken assetBundlesToken = json["assetbundles"];
                if (assetBundlesToken != null)
                {

                    // 各assetbundle要素についてダウンロードを試行
                    for (var assetBundle = assetBundlesToken.First; assetBundle != null; assetBundle = assetBundle.Next)
                    {
                        string assetBundleUrl = assetBundle["url"]?.ToString() ?? "";
                        string expectedHash = assetBundle["hash"]?.ToString() ?? "";
                        string? savedPath = DownloadAssetBundleWithRetry(assetBundleUrl, expectedHash).Result;
                        if (!string.IsNullOrEmpty(savedPath))
                        {
                            notLoadedAssetBundles.Add(savedPath);
                        }
                    }
                }
                else
                {
                    Logger.Error($"assetbundlesが見つかりません: {url}");
                }

                JToken hatsToken = json["hats"];
                if (hatsToken != null)
                {
                    Dictionary<string, CustomCosmeticsPackage> customCosmeticsPackages = new();
                    for (var hat = hatsToken.First; hat != null; hat = hat.Next)
                    {
                        string packageName = hat["package"]?.ToString() ?? "NONE_PACKAGE";
                        if (!customCosmeticsPackages.ContainsKey(packageName))
                        {
                            customCosmeticsPackages[packageName] = new CustomCosmeticsPackage(
                                packageName,
                                packageName,
                                0
                            );
                            loadedPackages.Add(customCosmeticsPackages[packageName]);
                        }
                        bool adaptive = hat["adaptive"] != null ? (bool)hat["adaptive"] : false;
                        bool resource_bounce = hat["resource"]?.ToString().Contains("bounce") ?? false;
                        bool flip_bounce = hat["flipresource"]?.ToString().Contains("bounce") ?? false;
                        bool climb_bounce = hat["climbresource"]?.ToString().Contains("bounce") ?? false;
                        bool back_bounce = hat["backresource"]?.ToString().Contains("bounce") ?? false;
                        bool backflip_bounce = hat["backflipresource"]?.ToString().Contains("bounce") ?? false;

                        var front = adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive;
                        if (resource_bounce)
                            front |= HatOptionType.Bounce;
                        var front_left = HatOptionType.None;
                        var back = adaptive ? HatOptionType.Adaptive : hat["backresource"] != null ? HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            back |= HatOptionType.Bounce;
                        var back_left = HatOptionType.None;
                        var backflip = adaptive ? HatOptionType.Adaptive : hat["backflipresource"] != null ? HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            backflip |= HatOptionType.Bounce;
                        var flip = adaptive ? HatOptionType.Adaptive : hat["flipresource"] != null ? HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            flip |= HatOptionType.Bounce;
                        var climb = adaptive ? HatOptionType.Adaptive : hat["climbresource"] != null ? HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            climb |= HatOptionType.Bounce;


                        var hatOption = new CustomCosmeticsHatOptions(
                            front: front,
                            back: back,
                            flip: flip,
                            flip_back: backflip,
                            climb: climb,
                            hideBody: hat["hideBody"] != null ? (bool)hat["hideBody"] : false
                        );

                        CustomCosmeticsHat customCosmeticsHat = new(
                            hat["name"].ToString(),
                            hat["name"]?.ToString(),
                            hat["name"].ToString(),
                            $"./SuperNewRolesNext/CustomCosmetics/{customCosmeticsPackages[packageName].name}/{hat["name"].ToString()}_",
                            hat["author"].ToString(),
                            customCosmeticsPackages[packageName],
                            hatOption,
                            null
                        );
                        customCosmeticsPackages[packageName].hats.Add(customCosmeticsHat);
                        moddedHats[customCosmeticsHat.ProdId] = customCosmeticsHat;

                        string packagenamed = customCosmeticsPackages[packageName].name;
                        if (!willDownloads.ContainsKey(packagenamed))
                            willDownloads.Add(packagenamed, []);
                        willDownloads[packagenamed].Add((hat["name"].ToString() + "_front", getpath(url, "hats/" + hat["resource"]?.ToString())));
                        if (hat["resourceleft"] != null)
                            willDownloads[packagenamed].Add((hat["name"].ToString() + "_front_left", getpath(url, "hats/" + hat["resourceleft"]?.ToString())));
                        if (hat["backresource"] != null)
                            willDownloads[packagenamed].Add((hat["name"].ToString() + "_back", getpath(url, "hats/" + hat["backresource"]?.ToString())));
                        if (hat["backresourceleft"] != null)
                            willDownloads[packagenamed].Add((hat["name"].ToString() + "_back_left", getpath(url, "hats/" + hat["backresourceleft"]?.ToString())));
                        if (hat["backflipresource"] != null)
                            willDownloads[packagenamed].Add((hat["name"].ToString() + "_backflip", getpath(url, "hats/" + hat["backflipresource"]?.ToString())));
                        if (hat["flipresource"] != null)
                            willDownloads[packagenamed].Add((hat["name"].ToString() + "_flip", getpath(url, "hats/" + hat["flipresource"]?.ToString())));
                        if (hat["climbresource"] != null)
                            willDownloads[packagenamed].Add((hat["name"].ToString() + "_climb", getpath(url, "hats/" + hat["climbresource"]?.ToString())));
                    }
                }
                else
                    Logger.Error($"hatsが見つかりません: {url}");

                // visorsとVisorsの両方のパターンがあるので
                JToken visorsToken = json["visors"] ?? json["Visors"];
                if (visorsToken != null)
                {
                    Dictionary<string, CustomCosmeticsPackage> customCosmeticsPackages = loadedPackages.ToDictionary(p => p.name);
                    for (var visor = visorsToken.First; visor != null; visor = visor.Next)
                    {
                        string packageName = visor["package"]?.ToString() ?? "NONE_PACKAGE";
                        if (!customCosmeticsPackages.ContainsKey(packageName))
                        {
                            customCosmeticsPackages[packageName] = new CustomCosmeticsPackage(
                                packageName,
                                packageName,
                                0
                            );
                            loadedPackages.Add(customCosmeticsPackages[packageName]);
                        }
                        bool adaptive = visor["adaptive"] != null ? (bool)visor["adaptive"] : false;

                        var visorOption = new CustomCosmeticsVisorOptions(
                            adaptive,
                            visor["flipresource"] != null,
                            visor["IsSNR"] != null ? (bool)visor["IsSNR"] : false
                        );

                        CustomCosmeticsVisor customCosmeticsVisor = new(
                            visor["name"].ToString(),
                            visor["name"]?.ToString(),
                            visor["name"].ToString(),
                            $"./SuperNewRolesNext/CustomCosmetics/{customCosmeticsPackages[packageName].name}/{visor["name"].ToString()}_",
                            visor["author"].ToString(),
                            customCosmeticsPackages[packageName],
                            visorOption,
                            null
                        );
                        customCosmeticsPackages[packageName].visors.Add(customCosmeticsVisor);
                        moddedVisors[customCosmeticsVisor.ProdId] = customCosmeticsVisor;

                        string packagenamed = customCosmeticsPackages[packageName].name;
                        if (!willDownloads.ContainsKey(packagenamed))
                            willDownloads.Add(packagenamed, []);
                        willDownloads[packagenamed].Add((visor["name"].ToString() + "_idle", getpath(url, json["visors"] != null ? "visors/" : "Visors/" + visor["resource"]?.ToString())));
                        if (visor["resourceleft"] != null)
                            willDownloads[packagenamed].Add((visor["name"].ToString() + "_idle_left", getpath(url, json["visors"] != null ? "visors/" : "Visors/" + visor["resourceleft"]?.ToString())));
                        if (visor["flipresource"] != null)
                            willDownloads[packagenamed].Add((visor["name"].ToString() + "_flip", getpath(url, json["visors"] != null ? "visors/" : "Visors/" + visor["flipresource"]?.ToString())));
                    }
                }
                else
                    Logger.Error($"visorsが見つかりません: {url}");

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

        Task.Run(DownloadSprites);

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
    private static string getpath(string url, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }

        // もし path が既に絶対パスならそのまま返す
        if (Uri.TryCreate(path, UriKind.Absolute, out Uri absoluteUri))
        {
            return absoluteUri.ToString();
        }

        try
        {
            // ローカルファイルの場合はそのまま返す
            if (url.StartsWith("./") || url.StartsWith("../"))
            {
                string baseDir = Path.GetDirectoryName(Path.GetFullPath(url)) ?? "";
                return Path.Combine(baseDir, path);
            }

            // url から基底URLを作成し、相対パスを解決する
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri baseUri))
            {
                Uri resultUri = new Uri(baseUri, path);
                return resultUri.ToString();
            }
            else
            {
                Logger.Error($"無効なURL形式です: {url}");
                return path;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"getpathの処理中にエラーが発生しました: url = {url}, path = {path}\n{ex}");
            return path;
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
                    cosmeticsPackage.hats.Add(customCosmeticsHat);
                    moddedHats[customCosmeticsHat.ProdId] = customCosmeticsHat;
                }
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
            // パッケージが見つからなかった場合のみ追加する
            if (!loadedPackages.Contains(cosmeticsPackage))
            {
                loadedPackages.Add(cosmeticsPackage);
            }
            Logger.Info($"{cosmeticsPackage.name} {cosmeticsPackage.version}");

            JToken visorsToken = packageJsonObject["visors"];
            if (visorsToken != null)
            {
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
                    cosmeticsPackage.visors.Add(customCosmeticsVisor);
                    moddedVisors[customCosmeticsVisor.ProdId] = customCosmeticsVisor;
                }
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
    public static void DownloadSprites()
    {
        using HttpClient client = new();
        string basePath = "./SuperNewRolesNext/CustomCosmetics/";
        const int MAX_CONCURRENT_DOWNLOADS = 20; // 同時に処理する定数個数
        using SemaphoreSlim semaphore = new(MAX_CONCURRENT_DOWNLOADS);
        List<Task> downloadTasks = new();

        try
        {
            // ベースディレクトリが存在することを確認
            Directory.CreateDirectory(basePath);

            foreach (var package in willDownloads)
            {
                Logger.Info($"Downloading sprites for {package.Key}");

                // パッケージディレクトリの作成
                string packagePath = Path.Combine(basePath, package.Key);
                Directory.CreateDirectory(packagePath);

                foreach (var (spriteName, spritePath) in package.Value)
                {
                    if (string.IsNullOrEmpty(spritePath))
                    {
                        Logger.Error($"Invalid sprite path for {spriteName}");
                        continue;
                    }

                    downloadTasks.Add(Task.Run(() =>
                    {
                        semaphore.Wait();
                        try
                        {
                            Logger.Info($"Downloading sprite {spriteName} from {spritePath}");
                            var response = client.GetAsync(spritePath, HttpCompletionOption.ResponseContentRead);
                            response.Wait();
                            if (response.Result.StatusCode != HttpStatusCode.OK)
                            {
                                Logger.Error($"Failed to download sprite {spriteName} from {spritePath} ({response.Result.StatusCode})");
                                return;
                            }

                            string filePath = Path.Combine(packagePath, $"{spriteName}.png").Replace("\\", "/");
                            using var responseStream = response.Result.Content.ReadAsStreamAsync().Result;
                            downloadedSprites[filePath] = responseStream.ReadFully();
                            using var fileStream = File.Create(filePath);
                            fileStream.Write(downloadedSprites[filePath]);
                            Logger.Info($"Downloaded sprite {spriteName} to {filePath}");
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error downloading sprite {spriteName} from {spritePath}: {e.Message}");
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error in DownloadSprites: {e}");
        }

        Task.WhenAll(downloadTasks).Wait();
    }
    public static Sprite LoadSpriteFromPath(string path)
    {
        Logger.Info($"LoadSpriteFromPath: {path}");
        try
        {
            if (File.Exists(path))
            {
                // UnityのUIスレッドで実行するために、メインスレッドで処理を行う
                Sprite result = null;

                // メインスレッドでなければ、メインスレッドに処理を移す
                if (Thread.CurrentThread.ManagedThreadId != SuperNewRolesPlugin.MainThreadId)
                {
                    SuperNewRolesPlugin.Instance.ExecuteInMainThread(() =>
                    {
                        result = CreateSpriteFromPath(path);
                    });
                    return result;
                }
                else
                {
                    return CreateSpriteFromPath(path);
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("Error loading texture from disk: " + path + "\n" + ex);
        }
        return null;
    }

    private static Sprite CreateSpriteFromPath(string path)
    {
        Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
        byte[] byteTexture;
        if (!downloadedSprites.TryGetValue(path, out byteTexture))
            byteTexture = File.ReadAllBytes(path);
        else
            Logger.Warning("Used Sprites");
        LoadImage(texture, byteTexture, false);
        if (texture == null)
            return null;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
        if (sprite == null)
            return null;
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }
    internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
    internal static d_LoadImage iCall_LoadImage;
    private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
    {
        Logger.Info($"Current Thread: {Thread.CurrentThread.ManagedThreadId}");
        if (iCall_LoadImage == null)
            iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        var il2cppArray = (Il2CppStructArray<byte>)data;
        return ImageConversion.LoadImage(tex, il2cppArray, markNonReadable);
        // return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
    }

    /// <summary>
    /// バイザー画像を読み込む
    /// </summary>
    /// <param name="path">読み込む画像の(ユーザストレージ内の)絶対パス</param>
    /// <param name="isSNR">SNRの独自規格で読み込むか</param>
    /// <param name="fromDisk">キャッシュを使用せずに読み込むか</param>
    /// <returns>バイザー画像</returns>
    public static Sprite CreateVisorSprite(string path, bool isSNR)
    {
        Sprite sprite = isSNR ? SNRVisorLoadSprite(path) : CreateSprite(path);
        return sprite;
    }

    private static Sprite CreateSprite(string path)
    {
        if (!File.Exists(path))
        {
            Logger.Error($"ファイルが存在しません: {path}");
            return null;
        }
        Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
        byte[] byteTexture = File.ReadAllBytes(path);
        LoadImage(texture, byteTexture, false);
        if (texture == null)
            return null;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
        if (sprite == null)
            return null;
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }

    /// <summary>
    /// バイザー画像を, SNR独自規格で読み込む (画像サイズを一定(115f)に変更し読み込む)
    /// </summary>
    /// <param name="path">読み込む画像の(ユーザストレージ内の)絶対パス</param>
    /// <returns>バイザー画像</returns>
    private static Sprite SNRVisorLoadSprite(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new(2, 2);

            LoadImage(texture, bytes, false);

            Rect rect = new(0f, 0f, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 115f);

            if (sprite == null) return null;

            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }
        catch { }
        return null;
    }
}

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
public static class SplashManagerStartPatch
{
    public static void Postfix(SplashManager __instance)
    {
        // メインスレッドで非同期処理を開始
        LoadCosmeticsAsync();
        Logger.Info("SplashManagerStartPatch");
    }

    private static void LoadCosmeticsAsync()
    {
        try
        {
            CustomCosmeticsLoader.Load();
        }
        catch (Exception e)
        {
            Logger.Error($"カスタムコスメティックのロード中にエラーが発生しました: {e}");
        }
    }
}