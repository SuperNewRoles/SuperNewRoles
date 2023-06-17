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
public class CustomHatData : HatData
{
    public HatViewData hatViewData;
    public CustomHatData(HatViewData hvd)
    {
        hatViewData = hvd;
    }
    public override AddressableAsset<HatViewData> CreateAddressableAsset()
    {
        return new CustomAddressableAsset<HatViewData>(hatViewData);
    }

    static Dictionary<string, HatViewData> cache = new();
    static HatViewData getbycache(string id)
    {
        if (!cache.ContainsKey(id))
        {
            cache[id] = HatManagerPatch.addHatData.FirstOrDefault(x => x.ProductId == id).hatViewData;
        }
        return cache[id];
    }
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetMaterialColor))]
    class HatParentSetMaterialColorPatch
    {
        public static bool Prefix(HatParent __instance, int color)
        {
            if (__instance.Hat == null || !__instance.Hat.ProductId.StartsWith("MOD_")) return true;
            HatViewData hatViewData = getbycache(__instance.Hat.ProductId);
            if (hatViewData && hatViewData.AltShader)
            {
                __instance.FrontLayer.sharedMaterial = hatViewData.AltShader;
                if (__instance.BackLayer)
                {
                    __instance.BackLayer.sharedMaterial = hatViewData.AltShader;
                }
            }
            else
            {
                __instance.FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
                if (__instance.BackLayer)
                {
                    __instance.BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
                }
            }
            int colorId = __instance.matProperties.ColorId;
            PlayerMaterial.SetColors(colorId, __instance.FrontLayer);
            if (__instance.BackLayer)
            {
                PlayerMaterial.SetColors(colorId, __instance.BackLayer);
            }
            __instance.FrontLayer.material.SetInt(PlayerMaterial.MaskLayer, __instance.matProperties.MaskLayer);
            if (__instance.BackLayer)
            {
                __instance.BackLayer.material.SetInt(PlayerMaterial.MaskLayer, __instance.matProperties.MaskLayer);
            }
            switch (__instance.matProperties.MaskType)
            {
                case PlayerMaterial.MaskType.ScrollingUI:
                    if (__instance.FrontLayer)
                    {
                        __instance.FrontLayer.maskInteraction = (SpriteMaskInteraction)1;
                    }
                    if (__instance.BackLayer)
                    {
                        __instance.BackLayer.maskInteraction = (SpriteMaskInteraction)1;
                    }
                    break;
                case PlayerMaterial.MaskType.Exile:
                    if (__instance.FrontLayer)
                    {
                        __instance.FrontLayer.maskInteraction = (SpriteMaskInteraction)2;
                    }
                    if (__instance.BackLayer)
                    {
                        __instance.BackLayer.maskInteraction = (SpriteMaskInteraction)2;
                    }
                    break;
                default:
                    if (__instance.FrontLayer)
                    {
                        __instance.FrontLayer.maskInteraction = (SpriteMaskInteraction)0;
                    }
                    if (__instance.BackLayer)
                    {
                        __instance.BackLayer.maskInteraction = (SpriteMaskInteraction)0;
                    }
                    break;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(HatParent), nameof(HatParent.PopulateFromHatViewData))]
    class HatParentPopulateFromHatViewDataPatch
    {
        public static bool Prefix(HatParent __instance)
        {
            if (__instance.Hat == null || !__instance.Hat.ProductId.StartsWith("MOD_")) return true;
            __instance.UpdateMaterial();
            HatViewData asset = getbycache(__instance.Hat.ProductId);
            SpriteAnimNodeSync spriteAnimNodeSync = __instance.SpriteSyncNode ?? __instance.GetComponent<SpriteAnimNodeSync>();
            if (spriteAnimNodeSync)
            {
                spriteAnimNodeSync.NodeId = (__instance.Hat.NoBounce ? 1 : 0);
            }
            if (__instance.Hat.InFront)
            {
                __instance.BackLayer.enabled = false;
                __instance.FrontLayer.enabled = true;
                __instance.FrontLayer.sprite = asset.MainImage;
            }
            else if (asset.BackImage)
            {
                __instance.BackLayer.enabled = true;
                __instance.FrontLayer.enabled = true;
                __instance.BackLayer.sprite = asset.BackImage;
                __instance.FrontLayer.sprite = asset.MainImage;
            }
            else
            {
                __instance.BackLayer.enabled = true;
                __instance.FrontLayer.enabled = false;
                __instance.FrontLayer.sprite = null;
                __instance.BackLayer.sprite = asset.MainImage;
            }
            if (__instance.options.Initialized && __instance.HideHat())
            {
                __instance.FrontLayer.enabled = false;
                __instance.BackLayer.enabled = false;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), new Type[] { typeof(int) })]
    class HatParentSetHatPatch
    {
        public static bool Prefix(HatParent __instance, int color)
        {
            if (__instance.Hat != null && __instance.Hat.ProductId.StartsWith("MOD_"))
            {
                HatViewData hatViewData = getbycache(__instance.Hat.ProductId);
                __instance.hatDataAsset = new CustomAddressableAsset<HatViewData>(hatViewData);
                __instance.PopulateFromHatViewData();
                __instance.SetMaterialColor(color);
                return false;
            }
            return true;
        }
    }
}