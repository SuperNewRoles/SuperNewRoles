using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class AssetManager
{
    public enum AssetBundleType : byte
    {
        Sprite,
    }

    // キャッシュ用のキー構造体を追加
    private readonly struct AssetCacheKey : IEquatable<AssetCacheKey>
    {
        public readonly string Path;
        public readonly Il2CppSystem.Type Type;

        public AssetCacheKey(string path, Il2CppSystem.Type type)
        {
            Path = path;
            Type = type;
        }

        // IEquatable<T>を実装して高速な比較を実現
        public bool Equals(AssetCacheKey other)
        {
            return Path == other.Path && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is AssetCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Path?.GetHashCode() ?? 0);
            hash = hash * 31 + (Type?.GetHashCode() ?? 0);
            return hash;
        }
    }

    // キャッシュ構造を最適化
    private static readonly Dictionary<byte, Dictionary<AssetCacheKey, UnityEngine.Object>> _cachedAssets = new();
    private static readonly Dictionary<AssetBundleType, byte> TypeToByte = new()
    {
        { AssetBundleType.Sprite, (byte)AssetBundleType.Sprite },
    };

    // AssetPathesの定義を最適化
    private static readonly (AssetBundleType Type, string Path)[] AssetPathes =
    {
        (AssetBundleType.Sprite, "snrsprites"),
    };

    private static Dictionary<byte, AssetBundle> Bundles { get; } = new(3);
    public static void Load()
    {
        SuperNewRolesPlugin.Logger.LogInfo("[Splash] Loading AssetBundles...");
        Logger.Info("-------Start AssetBundle-------");
        var ExcAssembly = SuperNewRolesPlugin.Assembly;
        foreach (var data in AssetPathes)
        {
            SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loading AssetBundle: {data.Type}");
            string platform = ModHelpers.IsAndroid() ? "_android" : "";
            AssetBundle assetBundle = null;
            try
            {
                // AssemblyからAssetBundleファイルを読み込む
                string resourceName = $"SuperNewRoles.Resources.{data.Path}{platform}.bundle";
                if (ModHelpers.IsAndroid())
                {
                    // メモリ量削減の為Androidはファイルから読み込む
                    using (var bundleStream = ExcAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (bundleStream == null)
                        {
                            Logger.Error($"Could not find embedded resource: {resourceName}", "LoadAssetBundle");
                            continue;
                        }

                        string assetBundlesDirectory = Path.GetFullPath(Path.Combine(SuperNewRolesPlugin.BaseDirectory, "AssetBundles"));
                        Directory.CreateDirectory(assetBundlesDirectory); // ディレクトリが存在しない場合は作成

                        string fileName = $"{data.Path}{platform}.bundle"; // 例: snrsprites.bundle または snrsprites_android.bundle
                        string filePath = Path.GetFullPath(Path.Combine(assetBundlesDirectory, fileName));

                        // Streamの内容を一時ファイルに保存
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            bundleStream.CopyTo(fileStream);
                        }

                        // 一時ファイルからAssetBundleを読み込む
                        assetBundle = AssetBundle.LoadFromFile(filePath);
                        if (assetBundle == null)
                        {
                            Logger.Error($"Failed to load AssetBundle from file: {filePath}", "LoadAssetBundle");
                            continue;
                        }
                        Logger.Info($"Loaded AssetBundle: {data.Type} from {filePath}");
                    } // bundleStream は using ステートメントにより自動的に破棄されます
                }
                else
                {
                    using (var bundleStream = ExcAssembly.GetManifestResourceStream(resourceName))
                    {
                        assetBundle = AssetBundle.LoadFromMemory(bundleStream.ReadFully());
                    }
                }

                //読み込んだAssetBundleを保存
                Bundles[TypeToByte[data.Type]] = assetBundle;
                //キャッシュ用のDictionaryを作成
                _cachedAssets[TypeToByte[data.Type]] = new(EqualityComparer<AssetCacheKey>.Default);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load AssetBundle:" + data.Type.ToString(), "LoadAssetBundle");
                Logger.Error(e.ToString(), "LoadAssetBundle");
            }
        }
        SuperNewRolesPlugin.Logger.LogInfo("[Splash] AssetBundles loaded");
        Logger.Info("-------End LoadAssetBundle-------");
    }
    /// <summary>
    /// 指定されたパスからアセットを取得します
    /// </summary>
    /// <typeparam name="T">取得するアセットの型（Spriteなど）</typeparam>
    /// <param name="path">アセットパス</param>
    /// <param name="assetBundleType">使用するアセットバンドルの種類</param>
    /// <returns>読み込まれたアセットまたはnull</returns>
    public static T GetAsset<T>(string path, AssetBundleType assetBundleType = AssetBundleType.Sprite) where T : UnityEngine.Object
    {
        var typeKey = TypeToByte[assetBundleType];
        if (!_cachedAssets.TryGetValue(typeKey, out var typeCache))
        {
            Logger.Error($"Cache for AssetBundleType {assetBundleType} not found. Bundle may not have been loaded.", "AssetManager.GetAsset");
            return null;
        }

        var cacheKey = new AssetCacheKey(path, Il2CppType.Of<T>());
        if (typeCache.TryGetValue(cacheKey, out var cachedUnityObject) && cachedUnityObject != null)
        {
            return cachedUnityObject.TryCast<T>();
        }

        if (!Bundles.TryGetValue(typeKey, out var bundle) || bundle == null)
        {
            Logger.Error($"AssetBundle for type {assetBundleType} not loaded or is null. Cannot load asset: {path}", "AssetManager.GetAsset");
            typeCache[cacheKey] = null;
            return null;
        }

        var loadedUnityObject = bundle.LoadAsset(path, Il2CppType.Of<T>());

        if (loadedUnityObject == null)
        {
            Logger.Error($"Failed to load asset: {path} of type {Il2CppType.Of<T>().Name} from bundle {assetBundleType}.", "AssetManager.GetAsset");
            typeCache[cacheKey] = null;
            return null;
        }

        if (ConfigRoles.IsCompressCosmetics)
        {
            // Attempt runtime compression for Texture2D and Sprites
            if (loadedUnityObject is Texture2D texture)
            {
                if (!texture.isReadable)
                {
                    Logger.Info($"Texture2D '{path}' is not readable. Skipping compression.", "AssetManager.GetAsset");
                }
                else if (IsUncompressedTextureFormat(texture.format))
                {
                    try
                    {
                        Logger.Info($"Compressing Texture2D: {path} (Original Format: {texture.format})", "AssetManager.GetAsset");
                        texture.Compress(true); // false for lower quality, faster compression
                        Logger.Info($"Compressed Texture2D: {path} (New Format: {texture.format})", "AssetManager.GetAsset");
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Failed to compress Texture2D '{path}'. Error: {e.Message}", "AssetManager.GetAsset");
                    }
                }
            }
            else if (loadedUnityObject is Sprite sprite)
            {
                Texture2D spriteTexture = sprite.texture;
                if (spriteTexture != null)
                {
                    if (!spriteTexture.isReadable)
                    {
                        Logger.Info($"Sprite's texture '{path}' is not readable. Skipping compression.", "AssetManager.GetAsset");
                    }
                    else if (IsUncompressedTextureFormat(spriteTexture.format))
                    {
                        try
                        {
                            Logger.Info($"Compressing Sprite's Texture: {path} (Original Format: {spriteTexture.format})", "AssetManager.GetAsset");
                            spriteTexture.Compress(true);
                            Logger.Info($"Compressed Sprite's Texture: {path} (New Format: {spriteTexture.format})", "AssetManager.GetAsset");
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Failed to compress Sprite's texture '{path}'. Error: {e.Message}", "AssetManager.GetAsset");
                        }
                    }
                }
            }
        }

        typeCache[cacheKey] = loadedUnityObject; // Cache the potentially modified Unity Object
        return loadedUnityObject.TryCast<T>();
    }

    private static bool IsUncompressedTextureFormat(TextureFormat format)
    {
        switch (format)
        {
            // Common uncompressed formats that Texture2D.Compress() typically targets
            case TextureFormat.Alpha8:
            case TextureFormat.ARGB4444:
            case TextureFormat.RGB24:
            case TextureFormat.RGBA32:
            case TextureFormat.ARGB32:
            case TextureFormat.BGRA32: // Often used on some platforms
                return true;
            default:
                // If it's already a known compressed format (DXT, ETC, ASTC, PVRTC, etc.)
                // or a format that Compress() doesn't handle well or isn't intended for, return false.
                return false;
        }
    }

    public static GameObject Instantiate(string path, Transform parent, AssetBundleType assetBundleType = AssetBundleType.Sprite)
    {
        var asset = GetAsset<GameObject>(path, assetBundleType);
        if (asset == null)
            throw new Exception($"Failed to load Asset: {path}");
        return GameObject.Instantiate(asset, parent);
    }
    public static T Instantiate<T>(string path, Transform parent, AssetBundleType assetBundleType = AssetBundleType.Sprite) where T : Component
    {
        var asset = GetAsset<GameObject>(path, assetBundleType);
        if (asset == null)
            throw new Exception($"Failed to load Asset: {path}");
        return GameObject.Instantiate(asset, parent).GetComponent<T>();
    }

    public static AudioSource PlaySoundFromBundle(string path, bool loop = false)
    {
        var asset = GetAsset<AudioClip>(path, AssetBundleType.Sprite);
        if (asset == null)
            throw new Exception($"Failed to load Asset: {path}");
        return SoundManager.Instance.PlaySound(asset, loop);
    }

    public static byte[] ReadFully(this Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
#nullable enable
    public static T? LoadAsset<T>(this AssetBundle assetBundle, string name) where T : UnityEngine.Object
    {
        return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.TryCast<T>();
    }
#nullable disable
    public static T DontUnload<T>(this T obj) where T : UnityEngine.Object
    {
        obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        return obj;
    }

    public static void UnloadAllAssets()
    {
        SuperNewRolesPlugin.Logger.LogInfo("[AssetManager] Unloading all cached assets...");
        foreach (var typeCache in _cachedAssets.Values)
        {
            foreach (var asset in typeCache)
            {
                if (asset.Value != null)
                    asset.Value.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;
            }
            typeCache.Clear();
        }
        // Optionally, if you also want to clear the Bundles dictionary (though this might not be what you want if bundles are meant to persist across scenes)
        // Bundles.Clear();
        if (!ModHelpers.IsAndroid())
            Resources.UnloadUnusedAssets();
        SuperNewRolesPlugin.Logger.LogInfo("[AssetManager] All cached assets unloaded.");
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnActiveSceneChange))]
    public static class OnActiveSceneChangePatch
    {
        public static void Postfix()
        {
            UnloadAllAssets();
        }
    }
}
