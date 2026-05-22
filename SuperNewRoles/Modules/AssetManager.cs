using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.U2D;

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

    private readonly struct SpriteAtlasEntry
    {
        public readonly string AtlasAssetName;
        public readonly string SpriteName;

        public SpriteAtlasEntry(string atlasAssetName, string spriteName)
        {
            AtlasAssetName = atlasAssetName;
            SpriteName = spriteName;
        }
    }

    private static Dictionary<byte, AssetBundle> Bundles { get; } = new(3);
    private static readonly Dictionary<byte, Dictionary<string, SpriteAtlasEntry>> SpriteAtlasLookup = new(3);
    private const string SpriteAtlasLookupAssetName = "SpriteAtlasLookup";
    private const string AndroidAssetBundleStampExtension = ".stamp";
    public static void Load()
    {
        SuperNewRoles.Logger.Info("[Splash] Loading AssetBundles...");
        Logger.Info("-------Start AssetBundle-------");
        Logger.Info("IsAndroid:" + ModHelpers.IsAndroid());
        var ExcAssembly = SuperNewRolesPlugin.Assembly;
        foreach (var data in AssetPathes)
        {
            SuperNewRoles.Logger.Info($"[Splash] Loading AssetBundle: {data.Type}");
            string platform = ModHelpers.IsAndroid() ? "_android" : "";
            AssetBundle assetBundle = null;
            try
            {
                // AssemblyからAssetBundleファイルを読み込む
                string resourceName = $"SuperNewRoles.Resources.{data.Path}{platform}.bundle";
                if (ModHelpers.IsAndroid())
                {
                    assetBundle = LoadAndroidAssetBundleFromFileCache(ExcAssembly, resourceName, $"{data.Path}{platform}.bundle");
                    if (assetBundle == null)
                        continue;
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
                LoadSpriteAtlasLookup(TypeToByte[data.Type], assetBundle);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load AssetBundle:" + data.Type.ToString(), "LoadAssetBundle");
                Logger.Error(e.ToString(), "LoadAssetBundle");
            }
        }
        SuperNewRoles.Logger.Info("[Splash] AssetBundles loaded");
        Logger.Info("-------End LoadAssetBundle-------");
    }

    // 毎回AssetBundleをコピーするのではなく、AndroidでのみAssemblyのメタデータの判定によるファイルキャッシュを利用して効率化
    private static AssetBundle LoadAndroidAssetBundleFromFileCache(Assembly assembly, string resourceName, string fileName)
    {
        string assetBundlesDirectory = Path.GetFullPath(Path.Combine(SuperNewRolesPlugin.BaseDirectory, "AssetBundles"));
        Directory.CreateDirectory(assetBundlesDirectory);

        string filePath = Path.GetFullPath(Path.Combine(assetBundlesDirectory, fileName));
        string stampPath = filePath + AndroidAssetBundleStampExtension;
        string expectedStamp = GetAndroidAssetBundleStamp(assembly, resourceName);
        bool hasMatchingStamp = false;

        try
        {
            hasMatchingStamp = File.Exists(filePath) && File.Exists(stampPath) && File.ReadAllText(stampPath) == expectedStamp;
        }
        catch (Exception e)
        {
            Logger.Warning($"Failed to read Android AssetBundle stamp: {e.Message}", "LoadAssetBundle");
        }

        if (hasMatchingStamp)
        {
            AssetBundle cachedBundle = AssetBundle.LoadFromFile(filePath);
            if (cachedBundle != null)
            {
                Logger.Info($"Loaded cached Android AssetBundle: {filePath}", "LoadAssetBundle");
                return cachedBundle;
            }

            Logger.Warning($"Cached Android AssetBundle was invalid. Re-extracting: {filePath}", "LoadAssetBundle");
            File.Delete(stampPath);
        }

        using (var bundleStream = assembly.GetManifestResourceStream(resourceName))
        {
            if (bundleStream == null)
            {
                Logger.Error($"Could not find embedded resource: {resourceName}", "LoadAssetBundle");
                return null;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                bundleStream.CopyTo(fileStream);
            }
        }

        File.WriteAllText(stampPath, expectedStamp);

        AssetBundle assetBundle = AssetBundle.LoadFromFile(filePath);
        if (assetBundle == null)
        {
            Logger.Error($"Failed to load AssetBundle from file: {filePath}", "LoadAssetBundle");
            return null;
        }

        Logger.Info($"Extracted and loaded Android AssetBundle: {filePath}", "LoadAssetBundle");
        return assetBundle;
    }

    private static string GetAndroidAssetBundleStamp(Assembly assembly, string resourceName)
    {
        string assemblyLocation = string.Empty;
        long assemblyLength = 0;
        long assemblyTicks = 0;

        try
        {
            assemblyLocation = assembly.Location ?? string.Empty;
            if (!string.IsNullOrEmpty(assemblyLocation) && File.Exists(assemblyLocation))
            {
                var info = new FileInfo(assemblyLocation);
                assemblyLength = info.Length;
                assemblyTicks = info.LastWriteTimeUtc.Ticks;
            }
        }
        catch (Exception e)
        {
            Logger.Warning($"Failed to read assembly stamp: {e.Message}", "LoadAssetBundle");
        }

        return string.Join("\n", resourceName, assembly.FullName, assemblyLocation, assemblyLength.ToString(), assemblyTicks.ToString());
    }

    private static void LoadSpriteAtlasLookup(byte typeKey, AssetBundle assetBundle)
    {
        var lookup = new Dictionary<string, SpriteAtlasEntry>(StringComparer.OrdinalIgnoreCase);
        SpriteAtlasLookup[typeKey] = lookup;

        int spriteCount = 0;
        int duplicateCount = 0;

        try
        {
            TextAsset lookupAsset = LoadSpriteAtlasLookupAsset(assetBundle);
            if (lookupAsset == null)
            {
                Logger.Warning("SpriteAtlas lookup asset was not found. SpriteAtlas fallback is disabled until the bundle is rebuilt.", "AssetManager.SpriteAtlasLookup");
                return;
            }

            string[] lines = lookupAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                if (rawLine.StartsWith("#", StringComparison.Ordinal))
                    continue;

                string[] parts = rawLine.Split('\t');
                if (parts.Length < 3)
                    continue;

                string spriteKey = NormalizeSpriteAtlasName(parts[0]);
                string atlasAssetName = parts[1];
                string spriteName = NormalizeSpriteAtlasName(parts[2]);

                if (string.IsNullOrEmpty(spriteKey) || string.IsNullOrEmpty(atlasAssetName) || string.IsNullOrEmpty(spriteName))
                    continue;

                var entry = new SpriteAtlasEntry(atlasAssetName, spriteName);
                if (AddSpriteAtlasLookupKey(lookup, spriteKey, entry))
                    spriteCount++;
                else
                    duplicateCount++;

                if (!spriteKey.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    AddSpriteAtlasLookupKey(lookup, spriteKey + ".png", entry);
            }

            Logger.Info($"Loaded SpriteAtlas lookup: sprites={spriteCount}, keys={lookup.Count}, duplicates={duplicateCount}", "AssetManager.SpriteAtlasLookup");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load SpriteAtlas lookup: {e}", "AssetManager.SpriteAtlasLookup");
        }
    }

    private static TextAsset LoadSpriteAtlasLookupAsset(AssetBundle assetBundle)
    {
        return assetBundle.LoadAsset(SpriteAtlasLookupAssetName, Il2CppType.Of<TextAsset>())?.TryCast<TextAsset>();
    }

    private static SpriteAtlas GetOrLoadSpriteAtlas(byte typeKey, string atlasAssetName)
    {
        if (!_cachedAssets.TryGetValue(typeKey, out var typeCache))
            return null;

        var atlasType = Il2CppType.Of<SpriteAtlas>();
        foreach (string name in GetSpriteAtlasAssetLookupNames(atlasAssetName))
        {
            if (typeCache.TryGetValue(new AssetCacheKey(name, atlasType), out var cachedAtlas) && cachedAtlas != null)
                return cachedAtlas.TryCast<SpriteAtlas>();
        }

        if (!Bundles.TryGetValue(typeKey, out var bundle) || bundle == null)
            return null;

        foreach (string name in GetSpriteAtlasAssetLookupNames(atlasAssetName))
        {
            SpriteAtlas atlas = bundle.LoadAsset(name, atlasType)?.TryCast<SpriteAtlas>();
            if (atlas == null)
                continue;

            atlas.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            CacheSpriteAtlas(typeKey, atlasAssetName, atlas);
            CacheSpriteAtlas(typeKey, name, atlas);
            return atlas;
        }

        Logger.Warning($"Failed to load SpriteAtlas: {atlasAssetName}", "AssetManager.SpriteAtlasLookup");
        return null;
    }

    private static IEnumerable<string> GetSpriteAtlasAssetLookupNames(string atlasAssetName)
    {
        if (string.IsNullOrEmpty(atlasAssetName))
            yield break;

        yield return atlasAssetName;

        string fileName = Path.GetFileName(atlasAssetName);
        if (!string.IsNullOrEmpty(fileName) && fileName != atlasAssetName)
            yield return fileName;

        string pathWithoutExtension = Path.ChangeExtension(atlasAssetName, null);
        if (!string.IsNullOrEmpty(pathWithoutExtension) && pathWithoutExtension != atlasAssetName)
            yield return pathWithoutExtension;

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(atlasAssetName);
        if (!string.IsNullOrEmpty(fileNameWithoutExtension) && fileNameWithoutExtension != pathWithoutExtension)
            yield return fileNameWithoutExtension;
    }

    private static bool EnsureSpriteAtlasLookup(byte typeKey)
    {
        if (SpriteAtlasLookup.TryGetValue(typeKey, out var lookup) && lookup.Count > 0)
            return true;

        if (!Bundles.TryGetValue(typeKey, out var bundle) || bundle == null)
            return false;

        LoadSpriteAtlasLookup(typeKey, bundle);
        return SpriteAtlasLookup.TryGetValue(typeKey, out lookup) && lookup.Count > 0;
    }

    private static void RemoveCachedSpriteAtlas(byte typeKey, string atlasAssetName)
    {
        if (!_cachedAssets.TryGetValue(typeKey, out var typeCache))
            return;

        var atlasType = Il2CppType.Of<SpriteAtlas>();
        foreach (string name in GetSpriteAtlasAssetLookupNames(atlasAssetName))
            typeCache.Remove(new AssetCacheKey(name, atlasType));
    }

    private static void CacheSpriteAtlas(byte typeKey, string assetName, SpriteAtlas atlas)
    {
        if (!_cachedAssets.TryGetValue(typeKey, out var typeCache))
            return;

        var atlasType = Il2CppType.Of<SpriteAtlas>();
        typeCache[new AssetCacheKey(assetName, atlasType)] = atlas;

        if (!string.IsNullOrEmpty(atlas.name))
            typeCache[new AssetCacheKey(atlas.name, atlasType)] = atlas;
    }

    private static bool AddSpriteAtlasLookupKey(Dictionary<string, SpriteAtlasEntry> lookup, string key, SpriteAtlasEntry entry)
    {
        if (lookup.ContainsKey(key))
            return false;

        lookup[key] = entry;
        return true;
    }

    private static string NormalizeSpriteAtlasName(string name)
    {
        const string cloneSuffix = "(Clone)";
        if (name != null && name.EndsWith(cloneSuffix, StringComparison.Ordinal))
            return name.Substring(0, name.Length - cloneSuffix.Length);

        return name;
    }

    /// <summary>
    /// 指定されたパスからアセットを取得します
    /// </summary>
    /// <typeparam name="T">取得するアセットの型（Spriteなど）</typeparam>
    /// <param name="path">アセットパス</param>
    /// <param name="assetBundleType">使用するアセットバンドルの種類</param>
    /// <returns>読み込まれたアセットまたはnull</returns>
    public static T? GetAsset<T>(string path, AssetBundleType assetBundleType = AssetBundleType.Sprite) where T : UnityEngine.Object
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
            loadedUnityObject = TryLoadSpriteFromAtlas<T>(path, typeKey);
            if (loadedUnityObject == null)
            {
                Logger.Error($"Failed to load asset: {path} of type {Il2CppType.Of<T>().Name} from bundle {assetBundleType}.", "AssetManager.GetAsset");
                typeCache[cacheKey] = null;
                return null;
            }
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

    private static UnityEngine.Object TryLoadSpriteFromAtlas<T>(string path, byte typeKey) where T : UnityEngine.Object
    {
        if (typeof(T) != typeof(Sprite))
            return null;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            if (!EnsureSpriteAtlasLookup(typeKey) || !SpriteAtlasLookup.TryGetValue(typeKey, out var lookup))
                return null;

            bool foundStaleAtlas = false;
            foreach (string lookupName in GetSpriteAtlasLookupNames(path))
            {
                if (!lookup.TryGetValue(lookupName, out var entry))
                    continue;

                try
                {
                    SpriteAtlas atlas = GetOrLoadSpriteAtlas(typeKey, entry.AtlasAssetName);
                    if (atlas == null)
                    {
                        foundStaleAtlas = true;
                        lookup.Remove(lookupName);
                        continue;
                    }

                    Sprite sprite = atlas.GetSprite(entry.SpriteName);
                    if (sprite == null)
                        continue;

                    Logger.Debug($"Loaded sprite from SpriteAtlas fallback: {path} -> {atlas.name}/{entry.SpriteName}", "AssetManager.GetAsset");
                    return sprite;
                }
                catch (Exception e)
                {
                    foundStaleAtlas = true;
                    RemoveCachedSpriteAtlas(typeKey, entry.AtlasAssetName);
                    Logger.Warning($"SpriteAtlas fallback failed: {path} -> {entry.SpriteName}: {e.Message}", "AssetManager.GetAsset");
                }
            }

            if (!foundStaleAtlas)
                return null;

            if (!Bundles.TryGetValue(typeKey, out var bundle) || bundle == null)
                return null;

            LoadSpriteAtlasLookup(typeKey, bundle);
        }

        return null;
    }

    private static IEnumerable<string> GetSpriteAtlasLookupNames(string path)
    {
        if (string.IsNullOrEmpty(path))
            yield break;

        yield return path;

        string fileName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(fileName) && fileName != path)
            yield return fileName;

        string pathWithoutExtension = Path.ChangeExtension(path, null);
        if (!string.IsNullOrEmpty(pathWithoutExtension) && pathWithoutExtension != path)
            yield return pathWithoutExtension;

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        if (!string.IsNullOrEmpty(fileNameWithoutExtension) && fileNameWithoutExtension != pathWithoutExtension)
            yield return fileNameWithoutExtension;
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
        SuperNewRoles.Logger.Info("[AssetManager] Unloading all cached assets...");
        bool isAndroid = ModHelpers.IsAndroid();
        int removedAssetCount = 0;
        int removedAndroidAtlasCount = 0;

        foreach (var typeCache in _cachedAssets.Values)
        {
            var keysToRemove = new List<AssetCacheKey>();
            foreach (var asset in typeCache)
            {
                if (asset.Value == null)
                {
                    keysToRemove.Add(asset.Key);
                    continue;
                }

                if (asset.Value is SpriteAtlas)
                {
                    if (!isAndroid)
                    {
                        asset.Value.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                        continue;
                    }

                    asset.Value.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;
                    keysToRemove.Add(asset.Key);
                    removedAndroidAtlasCount++;
                    continue;
                }

                asset.Value.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;
                keysToRemove.Add(asset.Key);
            }

            foreach (var key in keysToRemove)
            {
                typeCache.Remove(key);
                removedAssetCount++;
            }
        }

        // Optionally, if you also want to clear the Bundles dictionary (though this might not be what you want if bundles are meant to persist across scenes)
        // Bundles.Clear();
        if (!isAndroid || removedAssetCount > 0)
            Resources.UnloadUnusedAssets();
        SuperNewRoles.Logger.Info($"[AssetManager] Cached assets unloaded. removed={removedAssetCount}, androidAtlases={removedAndroidAtlasCount}");
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
