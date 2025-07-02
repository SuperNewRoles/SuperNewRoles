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
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using Unity.Collections;

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
            "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/refs/heads/main/cosmetics_next.json",
            // $"{SuperNewRolesPlugin.BaseDirectory}/debug_assets.json",
            "https://raw.githubusercontent.com/hinakkyu/TheOtherHats/refs/heads/master/CustomHats.json",
            "https://raw.githubusercontent.com/catudon1276/CatudonCostume/refs/heads/main/CustomHats.json",
            "https://raw.githubusercontent.com/catudon1276/Mememura-Hats/refs/heads/main/CustomHats.json",
            "https://raw.githubusercontent.com/Ujet222/TOPHats/refs/heads/main/CustomHats.json",
            // "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/refs/heads/main/CustomHats.json",
            // "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/refs/heads/main/CustomVisors.json",
            "https://raw.githubusercontent.com/Ujet222/TOPVisors/refs/heads/main/CustomVisors.json",
    };
    public static Action willLoad;
    public static bool runned = true;
    public const string ModdedPrefix = "Modded_";
    private static readonly HttpClient client = new();
    private static readonly int maxRetryAttempts = 1;
    private static readonly TimeSpan retryDelay = TimeSpan.FromSeconds(5);
    private static readonly List<string> notLoadedAssetBundles = new();
    private static readonly List<CustomCosmeticsPackage> loadedPackages = new();
    public static List<CustomCosmeticsPackage> LoadedPackages => loadedPackages;
    public static readonly Dictionary<string, CustomCosmeticsHat> moddedHats = new();
    public static readonly Dictionary<string, CustomCosmeticsVisor> moddedVisors = new();
    public static readonly Dictionary<string, CustomCosmeticsNamePlate> moddedNamePlates = new();
    private static Dictionary<string, List<(string, string)>> willDownloads = new();

    // Android版はメモリ節約のため、ダウンロードしたバイト列を保持しない
    private static readonly Dictionary<string, byte[]> downloadedSprites = null; //ModHelpers.IsAndroid() ? null : new();

    public static int AssetBundlesDownloadedCount;
    public static int AssetBundlesAllCount;
    public static bool AssetBundlesDownloading = false;

    public static int SpritesDownloadingCount;
    public static int SpritesAllCount;
    public static bool SpritesDownloading = false;

    public static readonly int MAX_CONCURRENT_DOWNLOADS = ModHelpers.IsAndroid() ? 10 : 30;

    private static string SanitizeFileName(string fileName)
    {
        string sanitized = fileName.Replace("...", ".");
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c.ToString(), "");
        }
        sanitized = sanitized.Replace("?", ""); // Explicitly remove question mark
        return sanitized;
    }

    public static IEnumerator LoadAsync(Func<IEnumerator, Coroutine> startCoroutine)
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                Logger.Error("インターネットに接続されていません");
                willLoad = () => { };
                CustomLoadingScreen.PleaseDoWillLoad = true;
                runned = true;
                yield break;
            case NetworkReachability.ReachableViaCarrierDataNetwork when !ConfigRoles.CanUseDataConnection.Value:
                Logger.Error("データ通信ではダウンロードしない設定です。");
                willLoad = () => { };
                CustomLoadingScreen.PleaseDoWillLoad = true;
                runned = true;
                yield break;
            default:
                break;
        }
        runned = false;
        AssetBundlesDownloading = true;
        client.Timeout = TimeSpan.FromSeconds(5);
        List<(string url, string content)> fetchTasks = new();
        List<Task> waitTasks = new();
        int waiting = 0;
        foreach (var url in CustomCosmeticsURLs)
        {
            bool isLocalFile = Path.IsPathRooted(url) || File.Exists(url);
            if (isLocalFile)
            {
                if (File.Exists(url))
                    fetchTasks.Add((url, File.ReadAllText(url)));
            }
            else
            {
                startCoroutine(GetStringAsync(url, (string content) =>
                {
                    fetchTasks.Add((url, content));
                    waiting--;
                }, (string error) =>
                {
                    Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {url}");
                    waiting--;
                }));
                waiting++;
            }
        }
        // 全タスクを待機
        yield return new WaitUntil((Il2CppSystem.Func<bool>)(() => waiting <= 0));

        // 置き換え: foreach (string url in CustomCosmeticsURLs) から始まるループを以下に変更:
        int assetBundleLoadingCount = 0;
        Dictionary<string, CustomCosmeticsPackage> packageMap = loadedPackages.ToDictionary(p => p.name);

        foreach (var ft in fetchTasks)
        {
            string url = ft.url;
            try
            {
                string jsonContent = ft.content;
                // JSONをパース
                JObject json = JObject.Parse(jsonContent);
                JToken assetBundlesToken = json["assetbundles"];
                if (assetBundlesToken != null)
                {

                    // 各assetbundle要素についてダウンロードを試行
                    for (var assetBundle = assetBundlesToken.First; assetBundle != null; assetBundle = assetBundle.Next)
                    {
                        string assetBundleUrl = assetBundle["url"]?.ToString() ?? "";
                        string assetBundleAndroidUrl = assetBundle["url_android"]?.ToString() ?? "";
                        string expectedHash = assetBundle["hash"]?.ToString() ?? "";
                        string expectedHashAndroid = assetBundle["hash_android"]?.ToString() ?? "";

                        bool isAndroid = ModHelpers.IsAndroid();

                        string currentUrl = isAndroid ? assetBundleAndroidUrl : assetBundleUrl;
                        string currentExpectedHash = isAndroid ? expectedHashAndroid : expectedHash;
                        startCoroutine(DownloadAssetBundleWithRetryAsync(currentUrl, currentExpectedHash, () =>
                        {
                            assetBundleLoadingCount--;
                            AssetBundlesDownloadedCount++;
                        }));
                        assetBundleLoadingCount++;
                        AssetBundlesAllCount++;
                    }
                }
                else
                    Logger.Error($"assetbundlesが見つかりません: {url}");

                JToken hatsToken = json["hats"];
                if (hatsToken != null)
                {
                    for (var hat = hatsToken.First; hat != null; hat = hat.Next)
                    {
                        string packageName = hat["package"]?.ToString() ?? "NONE_PACKAGE";
                        if (!packageMap.TryGetValue(packageName, out var currentPackage))
                        {
                            currentPackage = new CustomCosmeticsPackage(
                                packageName,
                                packageName,
                                 0,
                                1
                            );
                            packageMap[packageName] = currentPackage;
                            loadedPackages.Add(currentPackage);
                        }
                        bool adaptive = hat["adaptive"] != null ? (bool)hat["adaptive"] : false;
                        bool resource_bounce = hat["resource"]?.ToString().Contains("bounce") ?? false;
                        bool flip_bounce = hat["flipresource"]?.ToString().Contains("bounce") ?? false;
                        bool climb_bounce = hat["climbresource"]?.ToString().Contains("bounce") ?? false;
                        bool back_bounce = hat["backresource"]?.ToString().Contains("bounce") ?? false;
                        bool backflip_bounce = hat["backflipresource"]?.ToString().Contains("bounce") ?? false;

                        string hatName = hat["name"].ToString();
                        string sanitizedHatName = SanitizeFileName(hatName);

                        var front = adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive;
                        if (resource_bounce)
                            front |= HatOptionType.Bounce;
                        var front_left = HatOptionType.None;
                        var back = hat["backresource"] != null ? adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            back |= HatOptionType.Bounce;
                        var back_left = HatOptionType.None;
                        var backflip = hat["backflipresource"] != null ? adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            backflip |= HatOptionType.Bounce;
                        var flip = hat["flipresource"] != null ? adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive : HatOptionType.None;
                        if (resource_bounce)
                            flip |= HatOptionType.Bounce;
                        var climb = hat["climbresource"] != null ? adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive : HatOptionType.None;
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
                            hatName,
                            hat["name"]?.ToString(),
                            hatName,
                            $"{SuperNewRolesPlugin.BaseDirectory}/CustomCosmetics/{currentPackage.name}/{sanitizedHatName}_",
                            hat["author"].ToString(),
                            currentPackage,
                            hatOption,
                            null
                        );
                        currentPackage.hats.Add(customCosmeticsHat);
                        moddedHats[customCosmeticsHat.ProdId] = customCosmeticsHat;

                        string packagenamed = currentPackage.name;
                        if (!willDownloads.ContainsKey(packagenamed))
                            willDownloads.Add(packagenamed, []);
                        willDownloads[packagenamed].Add((hatName + "_front", getpath(url, "hats/" + hat["resource"]?.ToString())));
                        if (hat["resourceleft"] != null)
                            willDownloads[packagenamed].Add((hatName + "_front_left", getpath(url, "hats/" + hat["resourceleft"]?.ToString())));
                        if (hat["backresource"] != null)
                            willDownloads[packagenamed].Add((hatName + "_back", getpath(url, "hats/" + hat["backresource"]?.ToString())));
                        if (hat["backresourceleft"] != null)
                            willDownloads[packagenamed].Add((hatName + "_back_left", getpath(url, "hats/" + hat["backresourceleft"]?.ToString())));
                        if (hat["backflipresource"] != null)
                            willDownloads[packagenamed].Add((hatName + "_backflip", getpath(url, "hats/" + hat["backflipresource"]?.ToString())));
                        if (hat["flipresource"] != null)
                            willDownloads[packagenamed].Add((hatName + "_flip", getpath(url, "hats/" + hat["flipresource"]?.ToString())));
                        if (hat["climbresource"] != null)
                            willDownloads[packagenamed].Add((hatName + "_climb", getpath(url, "hats/" + hat["climbresource"]?.ToString())));
                    }
                }
                else
                    Logger.Error($"hatsが見つかりません: {url}");

                // visorsとVisorsの両方のパターンがあるので
                JToken visorsToken = json["visors"] ?? json["Visors"];
                if (visorsToken != null)
                {
                    for (var visor = visorsToken.First; visor != null; visor = visor.Next)
                    {
                        string packageName = visor["package"]?.ToString() ?? "NONE_PACKAGE";
                        if (!packageMap.TryGetValue(packageName, out var currentPackage))
                        {
                            currentPackage = new CustomCosmeticsPackage(
                                packageName,
                                packageName,
                                 0,
                                1
                            );
                            packageMap[packageName] = currentPackage;
                            loadedPackages.Add(currentPackage);
                        }
                        bool adaptive = visor["adaptive"] != null ? (bool)visor["adaptive"] : false;

                        string visorName = visor["name"].ToString();
                        string sanitizedVisorName = SanitizeFileName(visorName);

                        var visorOption = new CustomCosmeticsVisorOptions(
                            adaptive,
                            visor["flipresource"] != null,
                            visor["isSNR"] != null ? (bool)visor["isSNR"] : false
                        );
                        visorOption.climb = visor["climbresource"] != null;

                        CustomCosmeticsVisor customCosmeticsVisor = new(
                            visorName,
                            visor["name"]?.ToString(),
                            visorName,
                            $"{SuperNewRolesPlugin.BaseDirectory}/CustomCosmetics/{currentPackage.name}/{sanitizedVisorName}_",
                            visor["author"].ToString(),
                            currentPackage,
                            visorOption,
                            null
                        );
                        currentPackage.visors.Add(customCosmeticsVisor);
                        moddedVisors[customCosmeticsVisor.ProdId] = customCosmeticsVisor;

                        string packagenamed = currentPackage.name;
                        if (!willDownloads.ContainsKey(packagenamed))
                            willDownloads.Add(packagenamed, []);
                        willDownloads[packagenamed].Add((visorName + "_idle", getpath(url, (json["visors"] != null ? "visors/" : "Visors/") + visor["resource"]?.ToString())));
                        if (visor["flipresource"] != null)
                            willDownloads[packagenamed].Add((visorName + "_flip", getpath(url, (json["visors"] != null ? "visors/" : "Visors/") + visor["flipresource"]?.ToString())));
                        if (visor["climbresource"] != null)
                            willDownloads[packagenamed].Add((visorName + "_climb", getpath(url, (json["visors"] != null ? "visors/" : "Visors/") + visor["climbresource"]?.ToString())));
                    }
                }
                else
                    Logger.Error($"visorsが見つかりません: {url}");

                // nameplatesとNamePlatesの両方のパターンがあるので
                JToken namePlatesToken = json["nameplates"];
                if (namePlatesToken != null)
                {
                    for (var namePlate = namePlatesToken.First; namePlate != null; namePlate = namePlate.Next)
                    {
                        string packageName = namePlate["package"]?.ToString() ?? "NONE_PACKAGE";
                        if (!packageMap.TryGetValue(packageName, out var currentPackage))
                        {
                            currentPackage = new CustomCosmeticsPackage(
                                packageName,
                                packageName,
                                 0,
                                1
                            );
                            packageMap[packageName] = currentPackage;
                            loadedPackages.Add(currentPackage);
                        }

                        string namePlateName = namePlate["name"].ToString();
                        string sanitizedNamePlateName = SanitizeFileName(namePlateName);

                        CustomCosmeticsNamePlate customCosmeticsNamePlate = new(
                            namePlateName,
                            namePlate["name"]?.ToString(),
                            namePlateName,
                            $"{SuperNewRolesPlugin.BaseDirectory}/CustomCosmetics/{currentPackage.name}/{sanitizedNamePlateName}_",
                            namePlate["author"].ToString(),
                            currentPackage,
                            null
                        );
                        currentPackage.namePlates.Add(customCosmeticsNamePlate);
                        moddedNamePlates[customCosmeticsNamePlate.ProdId] = customCosmeticsNamePlate;

                        string packagenamed = currentPackage.name;
                        if (!willDownloads.ContainsKey(packagenamed))
                            willDownloads.Add(packagenamed, []);
                        willDownloads[packagenamed].Add((namePlateName + "_nameplate", getpath(url, "nameplates/" + namePlate["resource"]?.ToString())));
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

        Logger.Info("DownloadSpritesAsync done");
        // Wait for asset bundles to finish loading
        yield return DownloadSpritesAsync(startCoroutine);
        yield return new WaitUntil((Il2CppSystem.Func<bool>)(() => assetBundleLoadingCount <= 0));
        AssetBundlesDownloading = false;
        Logger.Info("assetBundleLoadingCount done");
        // After asset bundles are done, wait for sprite downloads to complete
        SpritesDownloading = false;
        Logger.Info("spriteDownloadCoroutine done");

        while (notLoadedAssetBundles.Count > 0)
        {
            string bundlePath = notLoadedAssetBundles[0];
            AssetBundle loadedBundleInstance = null;
            bool loadBundleFinished = false;
            Logger.Info("Loading asset bundle: " + bundlePath);

            // LoadAssetBundleコルーチンをstartCoroutineで実行し、完了を待機
            startCoroutine(LoadAssetBundle(bundlePath, (assetBundle) =>
            {
                loadedBundleInstance = assetBundle;
                loadBundleFinished = true;
            }, () =>
            {
                Logger.Error($"アセットバンドルのロードに失敗しました: {bundlePath}。キューから削除します。");
                loadedBundleInstance = null;
                loadBundleFinished = true;
            }));

            // 完了通知フラグが立つまで待機
            yield return new WaitUntil((Il2CppSystem.Func<bool>)(() => loadBundleFinished));
            Logger.Info("Loaded AssetBundle");

            if (loadedBundleInstance != null)
            {
                Logger.Info("Loading Packages");
                var allPackages = GetAllPackagesByType(loadedBundleInstance);
                string[] packagesHats = allPackages["Hats"];
                string[] packagesVisors = allPackages["Visors"];
                string[] packagesNamePlates = allPackages["NamePlates"];

                // LoadPackages は IEnumerator を返すように変更される
                // startCoroutine を使って実行し、完了を待つ
                Logger.Info("Loading Packages");
                yield return LoadPackages(loadedBundleInstance, packagesHats, packagesVisors, packagesNamePlates, startCoroutine);
                Logger.Info("Loaded Packages");
                notLoadedAssetBundles.RemoveAt(0);
            }
            else
            {
                Logger.Error($"アセットバンドルのロードに失敗しました: {bundlePath}。キューから削除します。");
                notLoadedAssetBundles.RemoveAt(0); // エラー時もキューから削除して無限ループを防ぐ
            }
        }
        LoadedPackages.Sort((a, b) =>
        {
            // orderが高いほど先頭にするため、降順でソート
            int orderComparison = b.order.CompareTo(a.order);
            if (orderComparison != 0)
                return orderComparison;
            // orderが同じ場合は名前でソート
            return a.name.CompareTo(b.name);
        });

        willLoad = () => { };
        CustomLoadingScreen.PleaseDoWillLoad = true;

        runned = true;
        Logger.Info("CustomCosmeticsLoader willLoad done");
    }
    public static IEnumerator GetStringAsync(string url, Action<string> onSuccess, Action<string> onError)
    {
        // UnityWebRequestを使用して非同期でデータを取得
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess(request.downloadHandler.text);
        }
        else
        {
            Logger.Error($"カスタムコスメティックの読み込みに失敗しました: {url} {request.error}");
            onError(request.error);
        }
        request.Dispose();
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

    private static IEnumerator LoadPackages(AssetBundle assetBundle, string[] packagesHats, string[] packagesVisors, string[] packagesNamePlates, Func<IEnumerator, Coroutine> startCoroutine)
    {
        foreach (string package in packagesHats)
        {
            Logger.Info("Loading PackagesLoadAssetASync");
            // package.jsonをロードするパスを組み立て、読み込み
            string packageJsonPath = $"assets/hats/{package}/package.json";
            var packageTextAsset = assetBundle.LoadAsset<TextAsset>(packageJsonPath);

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
                (int)packageInfo["parseversion"],
                packageInfo["order"] != null ? (int)packageInfo["order"] : 1
            );
            loadedPackages.Add(cosmeticsPackage);

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
            GameObject.Destroy(packageTextAsset);
        }
        foreach (string package in packagesVisors)
        {
            string visorsPath = $"assets/visors/{package}/package.json";
            var visorsTextAsset = assetBundle.LoadAsset<TextAsset>(visorsPath);

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
                (int)packageInfo["parseversion"],
                packageInfo["order"] != null ? (int)packageInfo["order"] : 1
            );
            // パッケージが見つからなかった場合のみ追加する
            if (!loadedPackages.Contains(cosmeticsPackage))
            {
                loadedPackages.Add(cosmeticsPackage);
            }

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
            GameObject.Destroy(visorsTextAsset);
        }
        foreach (string package in packagesNamePlates)
        {
            string namePlatesPath = $"assets/nameplates/{package}/package.json";
            var namePlatesTextAsset = assetBundle.LoadAsset<TextAsset>(namePlatesPath);

            if (namePlatesTextAsset == null)
            {
                Logger.Error($"パッケージ: {package} の package.json が読み込めません(NamePlates)");
                continue;
            }
            JObject packageJsonObject = JObject.Parse(namePlatesTextAsset.text);
            JToken packageInfo = packageJsonObject["package"];
            if (packageInfo == null)
            {
                Logger.Error($"パッケージ: {package} に package が見つかりません");
                continue;
            }
            CustomCosmeticsPackage cosmeticsPackage = loadedPackages.FirstOrDefault(p => p.name == packageInfo["name"].ToString() && p.name_en == packageInfo["name_en"]?.ToString() && p.version == (int)packageInfo["parseversion"]) ?? new(
                packageInfo["name"].ToString(),
                packageInfo["name_en"]?.ToString(),
                (int)packageInfo["parseversion"],
                packageInfo["order"] != null ? (int)packageInfo["order"] : 1
            );
            // パッケージが見つからなかった場合のみ追加する
            if (!loadedPackages.Contains(cosmeticsPackage))
            {
                loadedPackages.Add(cosmeticsPackage);
            }

            JToken namePlatesToken = packageJsonObject["nameplates"];
            if (namePlatesToken != null)
            {
                for (var namePlate = namePlatesToken.First; namePlate != null; namePlate = namePlate.Next)
                {
                    CustomCosmeticsNamePlate customCosmeticsNamePlate = new(
                        name: namePlate["name"].ToString(),
                        name_en: namePlate["name_en"]?.ToString(),
                        plate_id: namePlate["nameplate_id"].ToString(),
                        path_base: $"Assets/NamePlates/{package}/{namePlate["nameplate_id"].ToString()}/",
                        author: namePlate["author"].ToString(),
                        package: cosmeticsPackage,
                        assetBundle: assetBundle
                    );
                    cosmeticsPackage.namePlates.Add(customCosmeticsNamePlate);
                    moddedNamePlates[customCosmeticsNamePlate.ProdId] = customCosmeticsNamePlate;
                }
            }
            else
            {
                cosmeticsPackage.namePlates = [];
            }
            GameObject.Destroy(namePlatesTextAsset);
        }
        yield break;
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

    public static CustomCosmeticsNamePlate? GetModdedNamePlate(string namePlateId)
    {
        return moddedNamePlates.TryGetValue(namePlateId, out var namePlate) ? namePlate : null;
    }

    public static ICosmeticData? GetModdedNamePlateData(string namePlateId)
    {
        return moddedNamePlates.TryGetValue(namePlateId, out var namePlate) ? new ModdedNamePlateDataWrapper(namePlate) : null;
    }

    private static Dictionary<string, string[]> GetAllPackagesByType(AssetBundle assetBundle)
    {
        var categorizedPackagesSet = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Hats", new HashSet<string>(StringComparer.OrdinalIgnoreCase) },
            { "Visors", new HashSet<string>(StringComparer.OrdinalIgnoreCase) },
            { "NamePlates", new HashSet<string>(StringComparer.OrdinalIgnoreCase) }
        };

        string assetsPrefix = "assets/";
        // Dictionary keys are "Hats", "Visors", "NamePlates". Subdirectory names are lowercase.
        string[] typeSubdirectories = new string[] { "hats", "visors", "nameplates" };
        string[] typeKeys = new string[] { "Hats", "Visors", "NamePlates" };

        foreach (string assetPath in assetBundle.GetAllAssetNames())
        {
            if (!assetPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // e.g., assetPath = "assets/hats/packagename/resource.png"
            // pathAfterAssets = "hats/packagename/resource.png"
            string pathAfterAssets = assetPath.Substring(assetsPrefix.Length);

            for (int i = 0; i < typeSubdirectories.Length; i++)
            {
                string currentTypeSubdirectory = typeSubdirectories[i] + "/"; // e.g., "hats/"
                if (pathAfterAssets.StartsWith(currentTypeSubdirectory, StringComparison.OrdinalIgnoreCase))
                {
                    string determinedTypeKey = typeKeys[i]; // e.g., "Hats"

                    // packagePathInTypeFolder = "packagename/resource.png"
                    string packagePathInTypeFolder = pathAfterAssets.Substring(currentTypeSubdirectory.Length);

                    if (string.IsNullOrEmpty(packagePathInTypeFolder))
                    {
                        continue;
                    }

                    int endOfPackageNameIndex = packagePathInTypeFolder.IndexOf('/');
                    // If no further '/', it means path is like "assets/type/packagename"
                    // Original GetPackages logic (segments.Length >= 4) would skip this.
                    if (endOfPackageNameIndex == -1)
                    {
                        continue;
                    }

                    ReadOnlySpan<char> packageNameSpan = packagePathInTypeFolder.AsSpan(0, endOfPackageNameIndex);
                    string packageName = packageNameSpan.ToString();

                    if (!string.IsNullOrEmpty(packageName))
                    {
                        categorizedPackagesSet[determinedTypeKey].Add(packageName);
                    }
                    // Found type for this assetPath, move to next assetPath
                    break;
                }
            }
        }

        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in categorizedPackagesSet)
        {
            result[kvp.Key] = kvp.Value.ToArray();
        }
        return result;
    }

    private static IEnumerator LoadAssetBundle(string assetBundlePath, Action<AssetBundle> onFinish, Action onError)
    {
        Logger.Info("Loading!!! AssetBundle");
        // var assetBundleRequest = AssetBundle.LoadFromFileAsync(assetBundlePath);
        try
        {
            var assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            assetBundle.DontUnload();
            onFinish(assetBundle);
        }
        catch (Exception e)
        {
            onError();
        }
        yield break;
        /*
        yield return assetBundle;
        Logger.Info("Loaded!!! AssetBunde");
        assetBundle.assetBundle.DontUnload();
        Logger.Info($"アセットバンドルをロードしました: {assetBundlePath} {assetBundle != null}");
        onFinish(assetBundle.assetBundle);*/
    }

    private static IEnumerator DownloadAssetBundleWithRetryAsync(string assetBundleUrl, string expectedHash, Action onFinish)
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                yield return null;
                Logger.Error("インターネットに接続されていません");
                onFinish();
                yield break;
            case NetworkReachability.ReachableViaCarrierDataNetwork when !ConfigRoles.CanUseDataConnection.Value:
                yield return null;
                Logger.Error("データ通信ではダウンロードしない設定です。");
                onFinish();
                yield break;
            default:
                break;
        }
        string fileNameFromUrl = Path.GetFileName(assetBundleUrl);
        string bundleStorageDir = Path.Combine(SuperNewRolesPlugin.BaseDirectory, "CustomCosmetics", fileNameFromUrl);
        string targetPath = Path.Combine(bundleStorageDir, $"{expectedHash}.bundle");

        if (File.Exists(targetPath))
        {
            Logger.Info($"アセットバンドルが既に存在します: {targetPath}");
            if (!notLoadedAssetBundles.Contains(targetPath))
            {
                notLoadedAssetBundles.Add(targetPath);
            }
            onFinish();
            yield break;
        }

        for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
        {
            byte[] assetBundleData = null;
            bool successThisAttempt = false;
            SNRHttpClient request = null;

            bool isLocal = assetBundleUrl.StartsWith("./", StringComparison.Ordinal) ||
                           assetBundleUrl.StartsWith("../", StringComparison.Ordinal) ||
                           (Path.IsPathRooted(assetBundleUrl) && File.Exists(assetBundleUrl));

            if (isLocal)
            {
                try
                {
                    string localPath = Path.GetFullPath(assetBundleUrl);
                    Logger.Info($"ローカルアセットバンドルを読み込みます: {localPath}");
                    if (File.Exists(localPath))
                    {
                        assetBundleData = File.ReadAllBytes(localPath);
                    }
                    else
                    {
                        Logger.Error($"ローカルアセットバンドルが見つかりません: {localPath}");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"ローカルアセットバンドルの読み込み中にエラーが発生しました (Attempt {attempt + 1}): {assetBundleUrl}\n{e}");
                    // assetBundleData は null のまま
                }
            }
            else // Remote
            {
                request = SNRHttpClient.Get(assetBundleUrl);
                // ユーザーが設定したタイムアウト値を使用
                request.timeout = 60;
                request.ignoreSslErrors = true;

                IEnumerator webRequestEnumerator = SendSNRHttpClientHelper(request);
                bool moveNextSuccess = true;
                bool webRequestFailedMidExecution = false;

                // このループが yield を含みます。このループ自体は try-catch で囲みません。
                // try-catch は MoveNext() の呼び出しのみを囲みます。
                while (moveNextSuccess)
                {
                    try
                    {
                        moveNextSuccess = webRequestEnumerator.MoveNext();
                    }
                    catch (Exception ex) // MoveNext() 自体から例外が発生した場合
                    {
                        Logger.Error($"Webリクエストの実行中にエラー (MoveNext failed): {assetBundleUrl}\n{ex}");
                        webRequestFailedMidExecution = true;
                        moveNextSuccess = false; // ループを停止
                    }

                    if (webRequestFailedMidExecution) break;

                    if (moveNextSuccess)
                    {
                        yield return webRequestEnumerator.Current; // ここが yield return
                    }
                    else // IEnumerator が終了した場合
                    {
                        break; // ループを抜けて結果を処理
                    }
                }

                // ループ後 (リクエスト完了または MoveNext エラー後) に結果を処理
                try
                {
                    if (!webRequestFailedMidExecution && string.IsNullOrEmpty(request.error))
                    {
                        assetBundleData = request.downloadHandler.data;
                    }
                    else if (!webRequestFailedMidExecution) // webRequestFailedMidExecution が true の場合、request.result は信頼できない可能性
                    {
                        Logger.Error($"アセットバンドルのダウンロードに失敗しました: {assetBundleUrl}\nError: {request.error} Code: {request.responseCode}");
                    }
                    // webRequestFailedMidExecution が true の場合、エラーは既にログ記録済みで assetBundleData は null
                }
                catch (Exception ex) // request.result や request.downloadHandler.data アクセス時のエラーをキャッチ
                {
                    Logger.Error($"Webリクエスト結果の処理中にエラー: {assetBundleUrl}\n{ex}");
                    // assetBundleData は null のまま
                }
                finally
                {
                }
            }

            // ローカルおよびリモートデータの共通処理 (assetBundleData が null でない場合)
            if (assetBundleData != null)
            {
                try
                {
                    string actualFileHash;
                    using (var localMd5 = MD5.Create())
                    {
                        actualFileHash = BitConverter.ToString(localMd5.ComputeHash(assetBundleData))
                            .Replace("-", "")
                            .ToLowerInvariant();
                    }

                    if (!string.IsNullOrEmpty(expectedHash) && !actualFileHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Error($"ハッシュミスマッチ。URL: {assetBundleUrl}, Expected: {expectedHash}, Actual: {actualFileHash}");
                    }
                    Directory.CreateDirectory(bundleStorageDir);
                    foreach (string existingBundleFile in Directory.GetFiles(bundleStorageDir, "*.bundle"))
                    {
                        if (!Path.GetFullPath(existingBundleFile).Equals(Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                File.Delete(existingBundleFile);
                                Logger.Info($"古い/異なるハッシュのアセットバンドルを削除しました: {existingBundleFile}");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"古いアセットバンドルの削除に失敗しました: {existingBundleFile}\n{ex}");
                            }
                        }
                    }
                    File.WriteAllBytes(targetPath, assetBundleData);
                    Logger.Info($"アセットバンドルをダウンロード/検証し保存しました: {assetBundleUrl} -> {targetPath}");
                    if (!notLoadedAssetBundles.Contains(targetPath))
                        notLoadedAssetBundles.Add(targetPath);
                    successThisAttempt = true;
                }
                catch (Exception e)
                {
                    Logger.Error($"アセットバンドルの処理中にエラーが発生しました (Attempt {attempt + 1}): {assetBundleUrl}\n{e}");
                }
            }

            if (successThisAttempt)
            {
                onFinish();
                yield break;
            }

            if (attempt < maxRetryAttempts - 1)
            {
                Logger.Warning($"リトライします ({attempt + 2}/{maxRetryAttempts}) ... URL: {assetBundleUrl}");
                yield return new WaitForSeconds((float)retryDelay.TotalSeconds);
            }
        }

        Logger.Error($"アセットバンドルのダウンロードに最大リトライ回数({maxRetryAttempts})を超えました: {assetBundleUrl}");
        onFinish();
        yield break;
    }

    private static IEnumerator SendSNRHttpClientHelper(SNRHttpClient request)
    {
        yield return request.SendWebRequest();
    }

    public static IEnumerator DownloadSpritesAsync(Func<IEnumerator, Coroutine> startCoroutine)
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                yield return null;
                Logger.Error("インターネットに接続されていません");
                yield break;
            case NetworkReachability.ReachableViaCarrierDataNetwork when !ConfigRoles.CanUseDataConnection.Value:
                yield return null;
                Logger.Error("データ通信ではダウンロードしない設定です。");
                yield break;
            default:
                break;
        }
        string basePath = $"{SuperNewRolesPlugin.BaseDirectory}/CustomCosmetics/";
        int activeDownloads = 0;
        Queue<(string spriteName, string spritePath, string packageKey, string packagePath)> downloadQueue = new();
        SpritesAllCount = willDownloads.Sum(x => x.Value.Count);
        try // Setup phase
        {
            if (!willDownloads.Any())
            {
                Logger.Info("No sprites to download.");
                yield break;
            }

            Directory.CreateDirectory(basePath);

            foreach (var packageEntry in willDownloads)
            {
                string packageKey = packageEntry.Key;
                string packageDir = Path.Combine(basePath, packageKey);
                Directory.CreateDirectory(packageDir);
                foreach (var (spriteName, spritePath) in packageEntry.Value)
                {
                    if (string.IsNullOrEmpty(spritePath))
                    {
                        Logger.Error($"Invalid sprite path for {spriteName} in package {packageKey}");
                        continue;
                    }
                    downloadQueue.Enqueue((spriteName, spritePath, packageKey, packageDir));
                }
            }
            willDownloads = null;
        }
        catch (Exception e)
        {
            Logger.Error($"Error in DownloadSpritesAsync setup: {e}\\n{e.StackTrace}");
            yield break; // Stop if setup fails
        }

        // Main download loop - outside the initial try-catch for yield compatibility
        List<Coroutine> runningCoroutines = new List<Coroutine>();
        while (downloadQueue.Count > 0 || activeDownloads > 0)
        {
            while (downloadQueue.Count > 0 && activeDownloads < MAX_CONCURRENT_DOWNLOADS)
            {
                var item = downloadQueue.Dequeue();
                activeDownloads++;
                string sanitizedSpriteName = SanitizeFileName(item.spriteName);
                string filePath = Path.Combine(item.packagePath, $"{sanitizedSpriteName}.png").Replace("\\", "/");
                if (File.Exists(filePath))
                {
                    activeDownloads--;
                    continue;
                }
                Coroutine downloadCoroutine = startCoroutine(DownloadSingleSprite(item.spriteName, item.spritePath, item.packagePath, filePath, () => activeDownloads--));
                SpritesDownloadingCount = downloadQueue.Count;
                if (downloadCoroutine != null) // Check if StartCoroutine succeeded
                {
                    runningCoroutines.Add(downloadCoroutine);
                }
                else
                {
                    Logger.Error($"Failed to start download coroutine for {item.spriteName}");
                    activeDownloads--; // Decrement since it didn't start
                }
            }
            yield return null; // 次のフレームまで待機
        }


        Logger.Info("Finished all sprite download tasks.");
    }

    private static IEnumerator DownloadSingleSprite(string spriteName, string spriteUrl, string packageSavePath, string filePath, Action onComplete)
    {
        SNRHttpClient request = SNRHttpClient.Get(spriteUrl);
        request.timeout = 30; // 30 seconds timeout
        request.ignoreSslErrors = true;

        yield return request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error))
        {
            SNRDownloadHandler handler = request.downloadHandler; // DownloadHandlerを明示的に保持
            byte[] data = null;
            try
            {
                data = handler.data; // handlerからdataを取得
            }
            catch (ObjectCollectedException oce)
            {
                Logger.Error($"ObjectCollectedException while accessing handler.data for {spriteName} from {spriteUrl}. Error: {oce.Message}\n{oce.StackTrace}");
                // 可能であれば、ここでリトライ処理やエラー報告を行う
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception while accessing handler.data for {spriteName} from {spriteUrl}. Error: {ex.Message}\n{ex.StackTrace}");
            }

            if (data != null)
            {
                if (downloadedSprites != null)
                    downloadedSprites[filePath] = data;
                try
                {
                    File.WriteAllBytes(filePath, data);
                    Logger.Info($"Downloaded sprite {spriteName} to {filePath}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error writing sprite {spriteName} to {filePath}: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Logger.Error($"Failed to get data for sprite {spriteName} from {spriteUrl} (data was null after download attempt).");
            }
        }
        else
        {
            Logger.Error($"Failed to download sprite {spriteName} from {spriteUrl}. Error: {request.error}, Code: {request.responseCode}");
        }
        onComplete?.Invoke();
    }

    public static Sprite LoadSpriteFromPath(string path)
    {
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
        Texture2D texture = new(2, 2, TextureFormat.ARGB32, false);
        byte[] byteTexture;
        if (downloadedSprites == null || !downloadedSprites.TryGetValue(path, out byteTexture))
            byteTexture = File.ReadAllBytes(path);
        else
        {
            Logger.Warning("Used Sprites");
            downloadedSprites.Remove(path);
        }
        LoadImage(texture, byteTexture, true);
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
        if (iCall_LoadImage == null)
            iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        var il2cppArray = (Il2CppStructArray<byte>)data;
        return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        // return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
    }

    public static void ClearDownloadedSpriteCache()
    {
        // Only clear if the cache is actually in use (not on Android where it's null by default)
        if (downloadedSprites != null)
        {
            Logger.Info($"Clearing downloadedSprites cache. Count before: {downloadedSprites.Count}");
            downloadedSprites.Clear();
            // Forcing GC might be an option for aggressive testing, but generally not recommended for production.
            // System.GC.Collect();
            Logger.Info("downloadedSprites cache cleared.");
        }
        else
        {
            Logger.Info("downloadedSprites cache is not in use (likely Android platform). No action taken.");
        }
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
        Texture2D texture = new(2, 2, TextureFormat.ARGB32, false);
        byte[] byteTexture = File.ReadAllBytes(path);
        LoadImage(texture, byteTexture, true);
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

            LoadImage(texture, bytes, true);

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

    public static IEnumerator LoadCosmeticsTaskAsync(Func<IEnumerator, Coroutine> startCoroutine)
    {
        IEnumerator loadAsyncEnumerator = null;
        try
        {
            // LoadAsyncの呼び出し自体が例外を投げる可能性を考慮
            loadAsyncEnumerator = LoadAsync(startCoroutine);
        }
        catch (Exception setupEx)
        {
            Logger.Error($"LoadAsyncのセットアップ中にエラーが発生しました: {setupEx}");
            yield break;
        }

        if (loadAsyncEnumerator == null)
        {
            // 基本的に上記catchで捕捉されるはずだが念のため
            Logger.Error("LoadAsyncEnumeratorの取得に失敗しました。");
            yield break;
        }

        while (true)
        {
            object currentYieldedValue = null;
            bool hasMore;
            try
            {
                hasMore = loadAsyncEnumerator.MoveNext();
                if (hasMore)
                {
                    currentYieldedValue = loadAsyncEnumerator.Current;
                }
                else
                {
                    // Inner coroutine finished
                    yield break;
                }
            }
            catch (Exception runEx)
            {
                Logger.Error($"カスタムコスメティックのロード中にエラーが発生しました (LoadAsync実行中): {runEx}");
                yield break; // Stop on error from inner coroutine
            }
            yield return currentYieldedValue; // Yield what the sub-coroutine yielded
        }
    }
}