using System;
using System.Collections.Generic;
using AmongUs.Data;
using HarmonyLib;
using Innersloth.Assets;
using Sentry.Unity.NativeUtils;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

// ネームプレート取得時にカスタムネームプレートを返すようにパッチ
[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaSetCosmeticsPatch
{
    public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
    {
        if (!playerInfo.DefaultOutfit.NamePlateId.StartsWith(CustomCosmeticsLoader.ModdedPrefix)) return;
        var namePlateId = playerInfo.DefaultOutfit.NamePlateId;
        var namePlateData = CustomCosmeticsLoader.GetModdedNamePlate(namePlateId);
        if (namePlateData == null) return;
        __instance.Background.sprite = namePlateData.LoadSprite();
    }
}
[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.PreviewNameplate))]
public static class PlayerVoteAreaPreviewNameplatePatch
{
    public static bool Prefix(PlayerVoteArea __instance, string plateID)
    {
        __instance.gameObject.SetActive(true);
        // カスタムネームプレートの場合
        if (plateID.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
        {
            var namePlateData = CustomCosmeticsLoader.GetModdedNamePlate(plateID);
            if (namePlateData != null)
                __instance.Background.sprite = namePlateData.LoadSprite();
        }
        else
        {
            NamePlateData namePlateById = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(plateID);
            // 通常ネームプレートの場合
            __instance.StartCoroutine(__instance.CoLoadAssetAsync(
                namePlateById.TryCast<IAddressableAssetProvider<NamePlateViewData>>(),
                (Action<NamePlateViewData>)delegate (NamePlateViewData viewData)
                {
                    __instance.Background.sprite = viewData.Image;
                })
            );
        }
        __instance.PlayerIcon.gameObject.SetActive(value: false);
        __instance.NameText.text = DataManager.Player.Customization.Name;
        __instance.LevelNumberText.text = ProgressionManager.Instance.CurrentVisualLevel;
        return false;
    }
}