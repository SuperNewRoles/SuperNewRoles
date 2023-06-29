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
    public HatTempViewData htvd;
    public HatViewData hatViewData;
    public class HatTempViewData
    {
        public Sprite MainImage;
        public Sprite BackImage;
        public Sprite FlipImage;
        public Sprite BackFlipImage;
        public Sprite ClimbImage;
        public bool adaptive;
        public Material AltShader => adaptive ? new Material(Shader.Find("Unlit/PlayerShader")) : null;
        public string name;
        public HatViewData CreateHVD
        {
            get
            {
                return new HatViewData
                {
                    MainImage = MainImage,
                    BackImage = BackImage,
                    LeftMainImage = FlipImage,
                    LeftBackImage = BackFlipImage,
                    ClimbImage = ClimbImage,
                    AltShader = AltShader,
                    name = name
                };
            }
        }
    }
    static Dictionary<string, HatViewData> cache = new();
    static HatViewData getbycache(string id)
    {
        if (!cache.ContainsKey(id) || cache[id] == null)
        {
            cache[id] = HatManagerPatch.addHatData.FirstOrDefault(x => x.ProductId == id).htvd.CreateHVD;
        }
        return cache[id];
    }
    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.GetHat))]
    class CosmeticsCacheGetHatPatch
    {
        public static bool Prefix(CosmeticsCache __instance, string id, ref HatViewData __result)
        {
            if (!id.StartsWith("MOD_")) return true;
            __result = getbycache(id);
            if (__result == null)
                __result = __instance.hats["hat_NoHat"].GetAsset();
            return false;
        }
    }

    [HarmonyPatch(typeof(HatParent), nameof(HatParent.UpdateMaterial))]
    class HatParentUpdateMaterialPatch
    {
        public static bool Prefix(HatParent __instance)
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
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetFloorAnim))]
    class HatParentSetFloorAnimPatch
    {
        public static bool Prefix(HatParent __instance)
        {
            if (__instance.Hat == null || !__instance.Hat.ProductId.StartsWith("MOD_")) return true;
            __instance.BackLayer.enabled = false;
            __instance.enabled = true;
            __instance.FrontLayer.sprite = getbycache(__instance.Hat.ProductId).FloorImage;
            return false;
        }
    }
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetClimbAnim))]
    class HatParentSetClimbAnimPatch
    {
        public static bool Prefix(HatParent __instance)
        {
            if (__instance.Hat == null || !__instance.Hat.ProductId.StartsWith("MOD_")) return true;
            if (__instance.options.ShowForClimb)
            {
                __instance.BackLayer.enabled = false;
                __instance.enabled = true;
                __instance.FrontLayer.sprite = getbycache(__instance.Hat.ProductId).ClimbImage;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
    class HatParentLateUpdatePatch
    {
        public static bool Prefix(HatParent __instance)
        {
            if (__instance.Hat == null || !__instance.Hat.ProductId.StartsWith("MOD_")) return true;
            if (!__instance.Parent)
            {
                return true;
            }
            HatViewData hatViewData = getbycache(__instance.Hat.ProdId);
            if (__instance.FrontLayer.sprite != hatViewData.ClimbImage && __instance.FrontLayer.sprite != hatViewData.FloorImage)
            {
                if ((__instance.Hat.InFront || hatViewData.BackImage) && hatViewData.LeftMainImage)
                {
                    __instance.FrontLayer.sprite = (__instance.Parent.flipX ? hatViewData.LeftMainImage : hatViewData.MainImage);
                }
                if (hatViewData.BackImage && hatViewData.LeftBackImage)
                {
                    __instance.BackLayer.sprite = (__instance.Parent.flipX ? hatViewData.LeftBackImage : hatViewData.BackImage);
                }
                else if (!hatViewData.BackImage && !__instance.Hat.InFront && hatViewData.LeftMainImage)
                {
                    __instance.BackLayer.sprite = (__instance.Parent.flipX ? hatViewData.LeftMainImage : hatViewData.MainImage);
                }
            }
            else if (__instance.FrontLayer.sprite == hatViewData.ClimbImage || __instance.FrontLayer.sprite == hatViewData.LeftClimbImage)
            {
                SpriteAnimNodeSync spriteAnimNodeSync = __instance.SpriteSyncNode ?? __instance.GetComponent<SpriteAnimNodeSync>();
                if (spriteAnimNodeSync)
                {
                    spriteAnimNodeSync.NodeId = 0;
                }
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
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetIdleAnim), new Type[] { typeof(int) })]
    class HatParentSetIdleAnimPatch
    {
        public static bool Prefix(HatParent __instance, int colorId)
        {
            if (__instance.Hat != null && __instance.Hat.ProductId.StartsWith("MOD_"))
            {
                HatViewData hatViewData = getbycache(__instance.Hat.ProductId);
                __instance.PopulateFromHatViewData();
                __instance.SetMaterialColor(colorId);
                return false;
            }
            return true;
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
                __instance.PopulateFromHatViewData();
                __instance.SetMaterialColor(color);
                return false;
            }
            return true;
        }
    }
}
