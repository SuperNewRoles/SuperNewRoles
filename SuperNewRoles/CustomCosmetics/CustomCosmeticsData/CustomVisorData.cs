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
        public static bool Prefix(VisorLayer __instance, VisorData data, int color) // FIXME 仮
        {
            if (!data.ProductId.StartsWith("CustomVisors_")) return true;
            __instance.visorData = data;
            __instance.SetMaterialColor(color);
            __instance.PopulateFromViewData();
            return false;
        }
    }
}