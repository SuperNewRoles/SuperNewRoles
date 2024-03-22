using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
using UnityEngine;
namespace SuperNewRoles.CustomCosmetics;

public class CustomVisor
{
    public static Material VisorShader;

    public static bool isAdded = false;
    static readonly List<VisorData> visorData = new();
    public static readonly List<CustomVisorData> customVisorData = new();
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetVisorById))]
    class UnlockedVisorPatch
    {
        private static bool LOADED;
        private static bool SPRITELOADED = false;
        private static bool RUNNING = false;
        public static bool IsLoadingnow = false;
        public static List<VisorData> visordata = new();

        static void Prefix(HatManager __instance)
        {
            if (RUNNING) return;
            if (IsLoadingnow) return;
            if (SPRITELOADED)
            {
                RUNNING = true; // prevent simultanious execution
                IsLoadingnow = true;
                if (!LOADED)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string visorres = $"{assembly.GetName().Name}.Resources.Customvisors";
                    string[] visors = (from r in assembly.GetManifestResourceNames()
                                       where r.StartsWith(visorres) && r.EndsWith(".png")
                                       select r).ToArray<string>();

                    List<CustomVisors.CustomVisor> customvisors = CustomVisors.CreateCustomVisorDetails(visors);
                    foreach (CustomVisors.CustomVisor cv in customvisors)
                    {
                        customVisorData.Add(CreateVisorData(cv));
                    }
                }
                while (DownLoadClassVisor.VisorDetails.Count > 0)
                {
                    CustomVisorData chdata = CreateVisorData(DownLoadClassVisor.VisorDetails[0]);
                    customVisorData.Add(chdata);
                    DownLoadClassVisor.VisorDetails.RemoveAt(0);
                }
                LOADED = true;
                var data = __instance.allVisors.ToList();
                data.AddRange(customVisorData);
                visordata = data;
                __instance.allVisors = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData>(data.ToArray());
                IsLoadingnow = false;
            }
            else
            {
                SPRITELOADED = true;
                __instance.StartCoroutine(LoadVisorSprite().WrapToIl2Cpp());
            }
        }

        static IEnumerator LoadVisorSprite()
        {
            IsLoadingnow = true;
            if (!LOADED)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string visorres = $"{assembly.GetName().Name}.Resources.CustomVisors";
                string[] visors = (from r in assembly.GetManifestResourceNames()
                                   where r.StartsWith(visorres) && r.EndsWith(".png")
                                   select r).ToArray<string>();

                List<CustomVisors.CustomVisor> customvisors = CustomVisors.CreateCustomVisorDetails(visors);
                foreach (CustomVisors.CustomVisor cv in customvisors)
                {
                    CreateVisorSprite(cv.resource, cv.IsSNR);
                    yield return new WaitForSeconds(0.0005f);
                    if (cv.flipresource != null)
                    {
                        CreateVisorSprite(cv.flipresource, cv.IsSNR);
                        yield return new WaitForSeconds(0.0005f);
                    }
                }
            }
            foreach (var data in DownLoadClassVisor.VisorDetails)
            {
                var visor = GenereteVisorData(data);
                CreateVisorSprite(visor.resource, visor.IsSNR, true);
                yield return new WaitForSeconds(0.0005f);
                if (visor.flipresource != null)
                {
                    CreateVisorSprite(visor.flipresource, visor.IsSNR, true);
                    yield return new WaitForSeconds(0.0005f);
                }
            }
            IsLoadingnow = false;
        }
    }

    private static CustomVisors.CustomVisorOnline GenereteVisorData(CustomVisors.CustomVisorOnline cvo)
    {
        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\";
        cvo.resource = filePath + cvo.resource;
        if (cvo.flipresource != null)
            cvo.flipresource = filePath + cvo.flipresource;
        return cvo;
    }
    private static Sprite GetVisorSprite(string path) => VisorSprites.ContainsKey(path) ? VisorSprites[path] : null;

    /// <summary>
    /// バイザー画像を読み込む
    /// </summary>
    /// <param name="path">読み込む画像の(ユーザストレージ内の)絶対パス</param>
    /// <param name="isSNR">SNRの独自規格で読み込むか</param>
    /// <param name="fromDisk">キャッシュを使用せずに読み込むか</param>
    /// <returns>バイザー画像</returns>
    private static Sprite CreateVisorSprite(string path, bool isSNR, bool fromDisk = false)
    {
        Sprite sprite = isSNR ? SNRVisorLoadSprite(path) : ModHelpers.CreateSprite(path, fromDisk);
        if (!VisorSprites.ContainsKey(path)) VisorSprites.Add(path, sprite);
        return sprite;
    }

    /// <summary>
    /// バイザー画像を, SNR独自規格で読み込む (画像サイズを一定(115f)に変更し読み込む)
    /// </summary>
    /// <param name="path">読み込む画像の(ユーザストレージ内の)絶対パス</param>
    /// <returns>バイザー画像</returns>
    private static Sprite SNRVisorLoadSprite(string path)
    {
        //画像サイズは150*150
        if (ModHelpers.iCall_LoadImage == null)
            ModHelpers.iCall_LoadImage = IL2CPP.ResolveICall<ModHelpers.d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        if (ModHelpers.iCall_LoadImage == null) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new(2, 2);
            var Array = (Il2CppStructArray<byte>)bytes;
            ModHelpers.iCall_LoadImage.Invoke(texture.Pointer, Array.Pointer, false);

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

    static readonly Dictionary<string, Sprite> VisorSprites = new();

    private static CustomVisorData CreateVisorData(CustomVisors.CustomVisor cv, bool fromDisk = false, bool testOnly = false)
    {
        if (VisorShader == null && DestroyableSingleton<HatManager>.InstanceExists) VisorShader = new Material(Shader.Find("Unlit/PlayerShader"));

        CustomVisorData.VisorTempViewData visorViewData = new() { MainImage = GetVisorSprite(cv.resource) };

        CustomVisorData visor = new()
        {
            name = cv.name + "\nby " + cv.author,

            // 本体 : CosmticData
            displayOrder = 99,
            ProductId = "CustomVisors_" + cv.package + "_" + cv.name.Replace(' ', '_'),
            ChipOffset = new Vector2(0f, 0.25f),
            Free = true,
            NotInStore = true,

            // 本体 : VisorData
            behindHats = cv.behindHats
        };

        visorViewData.VisorName = visor.ProdId;
        if (cv.adaptive) visorViewData.Adaptive = true;
        if (cv.flipresource != null) visorViewData.FlipImage = GetVisorSprite(cv.flipresource);

        CustomVisors.VisorExtension extend = new()
        {
            author = cv.author ?? "Unknown",
            package = cv.package
            // package = cv.package ?? "YJ*白桜コレクション"
        };

        if (testOnly)
        {
            CustomVisors.TestExt = extend;
            CustomVisors.TestExt.condition = visor.name;
        }
        else
        {
            CustomVisors.CustomVisorRegistry.Add(visor.name, extend);
        }
        visor.vtvd = visorViewData;
        return visor;
    }
}