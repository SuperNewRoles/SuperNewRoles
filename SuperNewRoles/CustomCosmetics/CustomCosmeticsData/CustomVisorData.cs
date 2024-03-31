using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Innersloth.Assets;
using PowerTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static SuperNewRoles.CustomCosmetics.CustomHats;
using static SuperNewRoles.Modules.Blacklist;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
public class CustomVisorData : VisorData
{
    public VisorTempViewData vtvd;

    public class VisorTempViewData
    {
        public Sprite MainImage; // IdleFrame;
        public Sprite FlipImage; // LeftIdleFrame
        public Sprite ClimbImage; // ClimbFrame (未使用)
        public Sprite FloorFrame; // FloorFrame (未使用)
        public bool Adaptive; // MatchPlayerColor

        public string VisorName;

        public VisorViewData CreateVVD
        {
            get
            {
                return new VisorViewData
                {
                    IdleFrame = MainImage,
                    LeftIdleFrame = FlipImage,
                    ClimbFrame = ClimbImage, // (未使用)
                    FloorFrame = FloorFrame, // (未使用)
                    MatchPlayerColor = Adaptive,
                    name = VisorName
                };
            }
        }
    }
    static Dictionary<string, VisorViewData> cache = new();
    static VisorViewData getbycache(string id)
    {
        if (!cache.ContainsKey(id) || cache[id] == null)
        {
            cache[id] = CustomVisor.customVisorData.FirstOrDefault(x => x.ProductId == id).vtvd.CreateVVD;
        }
        return cache[id];
    }
    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.GetVisor))]
    class CosmeticsCacheGetVisorPatch
    {
        public static bool Prefix(CosmeticsCache __instance, string id, ref VisorViewData __result)
        {
            if (!id.StartsWith("CustomVisors_")) return true;
            __result = getbycache(id);
            if (__result == null)
                __result = __instance.visors["visor_EmptyVisor"].GetAsset();
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.UpdateMaterial))]
    class VisorLayerUpdateMaterialPatch
    {
        public static bool Prefix(VisorLayer __instance)
        {
            if (__instance.visorData == null || !__instance.visorData.ProductId.StartsWith("CustomVisors_")) return true;
            VisorViewData asset = getbycache(__instance.visorData.ProductId);
            PlayerMaterial.MaskType maskType = __instance.matProperties.MaskType;
            if (asset.MatchPlayerColor)
            {
                if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
                {
                    __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
                }
                else
                {
                    __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                }
            }
            else if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
            {
                __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
            }
            else
            {
                __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
            }

            switch (maskType)
            {
                case PlayerMaterial.MaskType.SimpleUI:
                    __instance.Image.maskInteraction = (SpriteMaskInteraction)1;
                    break;
                case PlayerMaterial.MaskType.Exile:
                    __instance.Image.maskInteraction = (SpriteMaskInteraction)2;
                    break;
                default:
                    __instance.Image.maskInteraction = (SpriteMaskInteraction)0;
                    break;
            }

            __instance.Image.material.SetInt(PlayerMaterial.MaskLayer, __instance.matProperties.MaskLayer);
            if (asset.MatchPlayerColor) // 上のifと統合していないのは公式がこの形だった為
            {
                PlayerMaterial.SetColors(__instance.matProperties.ColorId, __instance.Image);
            }

            if (__instance.matProperties.MaskLayer <= 0)
            {
                PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(__instance.Image, __instance.matProperties.IsLocalPlayer);
                return false;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetFlipX))]
    class VisorLayerSetFlipXPatch
    {
        public static bool Prefix(VisorLayer __instance, bool flipX)
        {
            if (__instance.visorData == null || !__instance.visorData.ProductId.StartsWith("CustomVisors_")) return true;
            __instance.Image.flipX = flipX;
            VisorViewData asset = getbycache(__instance.visorData.ProdId);
            if (flipX && asset.LeftIdleFrame)
            {
                __instance.Image.sprite = asset.LeftIdleFrame;
            }
            else
            {
                __instance.Image.sprite = asset.IdleFrame;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetFloorAnim))]
    class VisorLayerSetFloorAnimPatch
    {
        public static bool Prefix(VisorLayer __instance)
        {
            if (__instance.visorData == null || !__instance.visorData.ProductId.StartsWith("CustomVisors_")) return true;
            VisorViewData asset = getbycache(__instance.visorData.ProdId);
            __instance.Image.sprite = asset.FloorFrame ? asset.FloorFrame : asset.IdleFrame;
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.PopulateFromViewData))]
    class VisorLayerPopulateFromViewDataPatch
    {
        public static bool Prefix(VisorLayer __instance)
        {
            if (__instance.visorData == null || !__instance.visorData.ProductId.StartsWith("CustomVisors_"))
                return true;
            __instance.UpdateMaterial();
            if (!__instance.IsDestroyedOrNull())
            {
                __instance.transform.SetLocalZ(__instance.DesiredLocalZPosition);
                __instance.SetFlipX(__instance.Image.flipX);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetVisor), new Type[] { typeof(VisorData), typeof(int) })]
    class VisorLayerSetVisorPatch
    {
        public static bool Prefix(VisorLayer __instance, VisorData data, int color)
        {
            if (!data.ProductId.StartsWith("CustomVisors_")) return true;
            __instance.visorData = data;
            __instance.SetMaterialColor(color);
            __instance.PopulateFromViewData();
            return false;
        }
    }
}

public class CustomVisors
{
    public static Dictionary<string, VisorExtension> CustomVisorRegistry = new();
    public static VisorExtension TestExt = new() { IsNull = true };
    public static List<string> PackageNames = new();

    public struct VisorExtension
    {
        public bool IsNull;
        public string author;
        public string package;
        public string condition;
    }

    public class CustomVisor
    {
        /// <summary>制作者名</summary>
        public string author { get; set; }

        /// <summary>パッケージ名</summary>
        public string package { get; set; }

        /// <summary>バイザー名</summary>
        public string name { get; set; }

        /// <summary>本体:IdleFrame (vtvd:MainImage) に値する画像の ファイル名</summary>
        public string resource { get; set; }

        /// <summary>本体:LeftIdleFrame (vtvd:FlipImage) に値する画像の ファイル名</summary>
        public string flipresource { get; set; }

        /// <summary>本体:MatchPlayerColor (ボディカラー反映) を有効にするか</summary>
        public bool adaptive { get; set; }

        /// <summary>本体(VisorData):behindHats (バイザーをハットの裏にする) を有効にするか</summary>
        public bool behindHats { get; set; }

        /// <summary>バイザーが, SNR独自規格か (設定しない時は, TORと同様に画像を処理する)</summary>
        public bool IsSNR { get; set; }
    }

    public class CustomVisorOnline : CustomVisor
    {
        public string reshasha { get; set; }
        public string reshashf { get; set; }
    }

    public static List<CustomVisor> CreateCustomVisorDetails(string[] visors, bool fromDisk = false)
    {
        Dictionary<string, CustomVisor> fronts = new();
        Dictionary<string, string> backs = new();
        Dictionary<string, string> flips = new();
        Dictionary<string, string> backflips = new();
        Dictionary<string, string> climbs = new();

        for (int i = 0; i < visors.Length; i++)
        {
            string s = fromDisk ? visors[i][(visors[i].LastIndexOf("\\") + 1)..].Split('.')[0] : visors[i].Split('.')[3];
            string[] p = s.Split('_');

            HashSet<string> options = new();
            for (int j = 1; j < p.Length; j++)
                options.Add(p[j]);

            if (options.Contains("back") && options.Contains("flip"))
                backflips.Add(p[0], visors[i]);
            else if (options.Contains("climb"))
                climbs.Add(p[0], visors[i]);
            else if (options.Contains("back"))
                backs.Add(p[0], visors[i]);
            else if (options.Contains("flip"))
                flips.Add(p[0], visors[i]);
            else
            {
                CustomVisor custom = new()
                {
                    resource = visors[i],
                    name = p[0].Replace('-', ' '),
                    adaptive = options.Contains("adaptive"),
                    IsSNR = options.Contains("IsSNR"),
                };

                fronts.Add(p[0], custom);
            }
        }

        List<CustomVisor> customVisors = new();

        foreach (string k in fronts.Keys)
        {
            CustomVisor visor = fronts[k];
            flips.TryGetValue(k, out string fr);
            if (fr != null) visor.flipresource = fr;
            customVisors.Add(visor);
        }
        return customVisors;
    }

    public static VisorExtension GetVisorExtension(VisorData visor)
    {
        if (!TestExt.IsNull && TestExt.condition.Equals(visor.name)) { return TestExt; }
        CustomVisorRegistry.TryGetValue(visor.name, out VisorExtension ret);
        return ret;
    }
}
