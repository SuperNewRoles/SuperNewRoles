using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.Data;
using HarmonyLib;
using Innersloth.Assets;
using PowerTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static SuperNewRoles.CustomCosmetics.CustomHats;
using static SuperNewRoles.Modules.Blacklist;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
public class CustomPlateData : NamePlateData
{
    public TempPlateViewData tpvd;
    public class TempPlateViewData
    {
        public Sprite Image;
        public NamePlateViewData Create
        {
            get
            {
                return new()
                {
                    Image = Image
                };
            }
        }
    };
    static Dictionary<string, NamePlateViewData> cache = new();
    static NamePlateViewData getbycache(string id)
    {
        if (!cache.ContainsKey(id) || cache[id] == null)
        {
            CustomPlateData cpd = CustomPlate.customPlateData.FirstOrDefault(x => x.ProductId == id);
            if (cpd != null)
            {
                cache[id] = cpd.tpvd.Create;
            }
            else
            {
                cache[id] = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById(id)?.CreateAddressableAsset()?.GetAsset();
            }
        }
        return cache[id];
    }
    [HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.GetNameplate))]
    class CosmeticsCacheGetPlatePatch
    {
        public static bool Prefix(CosmeticsCache __instance, string id, ref NamePlateViewData __result)
        {
            if (!id.StartsWith("CustomNamePlates_")) return true;
            __result = getbycache(id);
            return false;
        }
    }
    [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable))]
    class NameplatesTabOnEnablePatch
    {
        static void makecoro(NameplatesTab __instance,  NameplateChip chip)
        {
            __instance.StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>(__instance, FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById(chip.ProductId).ViewDataRef, (Action<NamePlateViewData>)delegate (NamePlateViewData viewData)
            {
                chip.image.sprite = viewData?.Image;
            }));
        }
        public static void Postfix(NameplatesTab __instance)
        {
            __instance.StopAllCoroutines();
            foreach (NameplateChip chip in __instance.scroller.Inner.GetComponentsInChildren<NameplateChip>())
            {
                if (chip.ProductId.StartsWith("CustomNamePlates_"))
                {
                    NamePlateViewData npvd = getbycache(chip.ProductId);
                    chip.image.sprite = npvd.Image;
                }
                else
                {
                    makecoro(__instance, chip);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.PreviewNameplate))]
    class VisorLayerUpdateMaterialPatch
    {
        public static void Postfix(PlayerVoteArea __instance, string plateID)
        {
            if (!plateID.StartsWith("CustomNamePlates_")) return;
            NamePlateViewData npvd = getbycache(plateID);
            if (npvd != null)
            {
                __instance.Background.sprite = npvd.Image;
            }
        }
    }
}