using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Achievement
{
    class AchievementTitlePatch
    {
        [HarmonyPatch(typeof(PlayerVoteArea),nameof(PlayerVoteArea.Start))]
        class PlayerVoteAreaStart
        {
            public static void Postfix(PlayerVoteArea __instance)
            {
                if (__instance.transform.FindChild("TitleText") != null) return;
                AchievementData TargetData = AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started ? (__instance.TargetPlayerId == CachedPlayer.LocalPlayer.PlayerId ? AchievementManagerSNR.SelectedData : AchievementManagerSNR.GetPlayerData(ModHelpers.PlayerById(__instance.TargetPlayerId))) : AchievementManagerSNR.SelectedData;
                if (TargetData == null) return;
                TextMeshPro TitleText = GameObject.Instantiate(__instance.NameText, __instance.NameText.transform.parent);
                TitleText.name = "TitleText";
                TitleText.text = TargetData.Title;
                TitleText.enableWordWrapping = false;
                TitleText.fontSize = 1.15f;
                TitleText.transform.localPosition = new(0.6f, -0.215f, -0.1f);
                TitleText.color = Color.white;
            }
        }
    }
}
