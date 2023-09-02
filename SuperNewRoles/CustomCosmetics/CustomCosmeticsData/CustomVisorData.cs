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
    public VisorViewData visorViewData;
    public CustomVisorData(VisorViewData hvd)
    {
        visorViewData = hvd;
    }
    static Dictionary<string, VisorViewData> cache = new();
    static VisorViewData getbycache(string id)
    {
        if (!cache.ContainsKey(id))
        {
            cache[id] = CustomVisor.customVisorData.FirstOrDefault(x => x.ProductId == id).visorViewData;
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
            if (__instance.currentVisor == null || !__instance.currentVisor.ProductId.StartsWith("CustomVisors_")) return true;
            VisorViewData asset = getbycache(__instance.currentVisor.ProductId);
            if (asset.AltShader)
            {
                __instance.Image.sharedMaterial = asset.AltShader;
            }
            else
            {
                __instance.Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
            }
            PlayerMaterial.SetColors(__instance.matProperties.ColorId, __instance.Image);
            switch (__instance.matProperties.MaskType)
            {
                case PlayerMaterial.MaskType.SimpleUI:
                case PlayerMaterial.MaskType.ScrollingUI:
                    __instance.Image.maskInteraction = (SpriteMaskInteraction)1;
                    break;
                case PlayerMaterial.MaskType.Exile:
                    __instance.Image.maskInteraction = (SpriteMaskInteraction)2;
                    break;
                default:
                    __instance.Image.maskInteraction = (SpriteMaskInteraction)0;
                    break;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetFlipX))]
    class VisorLayerSetFlipXPatch
    {
        public static bool Prefix(VisorLayer __instance, bool flipX)
        {
            if (__instance.currentVisor == null || !__instance.currentVisor.ProductId.StartsWith("CustomVisors_")) return true;
            __instance.Image.flipX = flipX;
            VisorViewData asset = getbycache(__instance.currentVisor.ProdId);
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
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetVisor), new Type[] { typeof(VisorData), typeof(VisorViewData), typeof(int) })]
    class VisorLayerSetVisor2Patch
    {
        public static bool Prefix(VisorLayer __instance, VisorData data, VisorViewData visorView, int colorId)
        {
            if (!data.ProductId.StartsWith("CustomVisors_")) return true;
            __instance.currentVisor = data;
            if (data.BehindHats)
            {
                __instance.transform.SetLocalZ(__instance.ZIndexSpacing * -1.5f);
            }
            else
            {
                __instance.transform.SetLocalZ(__instance.ZIndexSpacing * -3f);
            }
            __instance.SetFlipX(__instance.Image.flipX);
            __instance.SetMaterialColor(colorId);
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetVisor), new Type[] { typeof(VisorData), typeof(int) })]
    class VisorLayerSetVisorPatch
    {
        public static bool Prefix(VisorLayer __instance, VisorData data, int colorId)
        {
            if (!data.ProductId.StartsWith("CustomVisors_")) return true;
            __instance.currentVisor = data;
            __instance.SetVisor(__instance.currentVisor, getbycache(data.ProdId), colorId);
            return false;
        }
    }
}