using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
    private static Tuple<AssetBundleType, string>[] AssetPathes = new Tuple<AssetBundleType, string>[4]
    {
        new(AssetBundleType.Sprite, "snrsprites"),
        new(AssetBundleType.Sound, "SNRSounds"),
        new(AssetBundleType.Wavecannon, "WaveCannon.WaveCannonEffects"),
        new(AssetBundleType.BodyBuilder, "BodyBuilder.BodyBuilderPoses")
    };
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
            } catch (Exception e)
            {
                Logger.Error($"Failed to load AssetBundle:" + data.Item1.ToString(),"LoadAssetBundle");
                Logger.Error(e.ToString(),"LoadAssetBundle");
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
        //_cachedAssetsにないなら読み込まれてないってことだからreturn
        if (!_cachedAssets.TryGetValue((byte)assetBundleType, out var _data))
            return null;
        //キャッシュにあるならそれを返す
        Il2CppSystem.Type il2CppType = Il2CppType.Of<T>();
        if (_data.TryGetValue(path+il2CppType.ToString(), out UnityEngine.Object result))
            return result.TryCast<T>();
        //読み込む
        T rs = Bundles[(byte)assetBundleType]
              .LoadAsset<T>(path).DontUnload();
        //キャッシュに保存
        _data[path + il2CppType.ToString()] = rs;
        return rs;
    }
}