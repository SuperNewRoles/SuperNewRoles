using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    private readonly struct AssetCacheKey
    {
        public readonly string Path;
        public readonly Il2CppSystem.Type Type;

        public AssetCacheKey(string path, Il2CppSystem.Type type)
        {
            Path = path;
            Type = type;
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
        Logger.Info("-------Start AssetBundle-------");
        var ExcAssembly = Assembly.GetExecutingAssembly();
        foreach (var data in AssetPathes)
        {
            Logger.Info($"Loading AssetBundle: {data.Type}");
            try
            {
                //AssemblyからAssetBundleファイルを読み込む
                var BundleStream = ExcAssembly.
                    GetManifestResourceStream(
                    $"SuperNewRoles.Resources.{data.Path}.bundle"
                    );
                //AssetBundleを読み込む
                var assetBundle = AssetBundle.LoadFromMemory(BundleStream.ReadFully());
                //読み込んだAssetBundleを保存
                Bundles[TypeToByte[data.Type]] = assetBundle;
                //キャッシュ用のDictionaryを作成
                _cachedAssets[TypeToByte[data.Type]] = new();

                assetBundle.DontUnload();

                BundleStream.Dispose();
                Logger.Info($"Loaded AssetBundle: {data.Type}");
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load AssetBundle:" + data.Type.ToString(), "LoadAssetBundle");
                Logger.Error(e.ToString(), "LoadAssetBundle");
            }
        }
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
            return null;

        var cacheKey = new AssetCacheKey(path, Il2CppType.Of<T>());
        if (typeCache.TryGetValue(cacheKey, out var cached))
            return cached.TryCast<T>();

        var bundle = Bundles[typeKey];
        var loadedAsset = bundle.LoadAsset<T>(path);
        if (loadedAsset == null)
        {
            Logger.Error($"Failed to load Asset: {path}", "GetAsset");
            typeCache[cacheKey] = null;
            return null;
        }
        var asset = loadedAsset.DontUnload();
        typeCache[cacheKey] = asset;
        return asset;
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
}
