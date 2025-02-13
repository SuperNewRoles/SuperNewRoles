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
        Sound,
        Wavecannon,
        BodyBuilder
    }
    private static Dictionary<byte, Dictionary<string, UnityEngine.Object>> _cachedAssets { get; } = new();
    private static Dictionary<byte, AssetBundle> Bundles { get; } = new(3);
    private static Tuple<AssetBundleType, string>[] AssetPathes = [
        new(AssetBundleType.Sprite, "snrsprites"),/*
        new(AssetBundleType.Sound, "snrsounds"),
        new(AssetBundleType.Wavecannon, "WaveCannon.WaveCannonEffects"),
        new(AssetBundleType.BodyBuilder, "BodyBuilder.BodyBuilderPoses")*/
    ];
    public static void Load()
    {
        Logger.Info("-------Start AssetBundle-------");
        var ExcAssembly = Assembly.GetExecutingAssembly();
        foreach (var data in AssetPathes)
        {
            Logger.Info($"Loading AssetBundle:" + data.Item1.ToString());
            try
            {
                //AssemblyからAssetBundleファイルを読み込む
                var BundleStream = ExcAssembly.
                    GetManifestResourceStream(
                    $"SuperNewRoles.Resources.{data.Item2}.bundle"
                    );
                //AssetBundleを読み込む
                var assetBundle = AssetBundle.LoadFromMemory(BundleStream.ReadFully());
                //読み込んだAssetBundleを保存
                Bundles[(byte)data.Item1] = assetBundle;
                //キャッシュ用のDictionaryを作成
                _cachedAssets[(byte)data.Item1] = new();

                assetBundle.DontUnload();

                BundleStream.Dispose();
                Logger.Info($"Loaded AssetBundle:" + data.Item1.ToString());
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load AssetBundle:" + data.Item1.ToString(), "LoadAssetBundle");
                Logger.Error(e.ToString(), "LoadAssetBundle");
            }
        }
        Logger.Info("-------End LoadAssetBundle-------");
    }
    /// <summary>
    /// アセットをロードする
    /// </summary>
    /// <typeparam name="T">読み込むタイプ(Spriteなど)</typeparam>
    /// <param name="path">パス</param>
    /// <param name="assetBundleType">保存しているAssetBundle</param>
    /// <returns></returns>
    public static T GetAsset<T>(string path, AssetBundleType assetBundleType = AssetBundleType.Sprite) where T : UnityEngine.Object
    {
        // 指定されたAssetBundleTypeのキャッシュが存在しない場合はnullを返す
        if (!_cachedAssets.TryGetValue((byte)assetBundleType, out var cache))
            return null;

        // 型情報の文字列表現を1度だけ取得してキャッシュキーを生成
        Il2CppSystem.Type il2CppType = Il2CppType.Of<T>();
        string cacheKey = path + il2CppType.ToString();

        // キャッシュに存在する場合、その値を返す
        if (cache.TryGetValue(cacheKey, out UnityEngine.Object cachedObj))
            return cachedObj.TryCast<T>();

        // AssetBundleからアセットを読み込み、DontUnloadを適用
        T asset = Bundles[(byte)assetBundleType]
                  .LoadAsset<T>(path).DontUnload();

        // 読み込んだアセットをキャッシュに保存
        cache[cacheKey] = asset;
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
