using System;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;
using UnityEngine.UI;

//TOHより!
namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.SetRecommendations), new Type[] { typeof(int), typeof(bool) })]
public static class ChangeRecommendedSettingPatch
{
    public static bool Prefix(GameOptionsData __instance, int numPlayers, bool isOnline)
    {
        //通常モードとSHRモード
        numPlayers = Mathf.Clamp(numPlayers, 4, 15);
        __instance.PlayerSpeedMod = __instance.MapId == 4 ? 1.25f : 1f; //AirShipなら1.25、それ以外は1
        __instance.CrewLightMod = 0.5f;
        __instance.ImpostorLightMod = 1.75f;
        __instance.KillCooldown = 30;
        __instance.NumCommonTasks = 2;
        __instance.NumLongTasks = 3;
        __instance.NumShortTasks = 5;
        __instance.NumEmergencyMeetings = 1;
        __instance.TaskBarMode = AmongUs.GameOptions.TaskBarMode.Invisible;
        if (!isOnline)
            __instance.NumImpostors = GameOptionsData.RecommendedImpostors[numPlayers];
        __instance.KillDistance = 0;
        __instance.DiscussionTime = 0;
        __instance.VotingTime = 150;
        __instance.IsDefaults = true;
        __instance.ConfirmImpostor = false;
        __instance.VisualTasks = false;
        __instance.EmergencyCooldown = (int)__instance.KillCooldown - 15; //キルクールより15秒短く

        __instance.SetFloat(FloatOptionNames.ShapeshifterCooldown, 10f);
        __instance.SetFloat(FloatOptionNames.ShapeshifterDuration, 30f);
        __instance.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
        __instance.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
        __instance.SetFloat(FloatOptionNames.ScientistCooldown, 15f);
        __instance.SetFloat(FloatOptionNames.ScientistBatteryCharge, 5f);
        __instance.SetFloat(FloatOptionNames.GuardianAngelCooldown, 60f);
        __instance.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 10f);
        __instance.SetFloat(FloatOptionNames.EngineerCooldown, 30f);
        __instance.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 15f);
        switch (ModeHandler.GetMode(false))
        {
            //ハイドアンドシーク
            case ModeId.HideAndSeek:
                __instance.PlayerSpeedMod = 1.75f;
                __instance.CrewLightMod = 5f;
                __instance.ImpostorLightMod = 0.25f;
                __instance.NumImpostors = 1;
                __instance.NumCommonTasks = 0;
                __instance.NumLongTasks = 0;
                __instance.NumShortTasks = 6;
                __instance.KillCooldown = 10f;
                __instance.TaskBarMode = 0;
                break;
            //バトルロイヤル
            case ModeId.BattleRoyal:
                __instance.PlayerSpeedMod = 1.75f;
                __instance.ImpostorLightMod = 2f;
                __instance.KillCooldown = 1f;
                __instance.TaskBarMode = AmongUs.GameOptions.TaskBarMode.Invisible;
                break;
            //ゾンビモード
            case ModeId.Zombie:
                __instance.PlayerSpeedMod = 1.5f;
                __instance.CrewLightMod = 1.5f;
                __instance.ImpostorLightMod = 0.25f;
                __instance.NumImpostors = 1;
                __instance.NumCommonTasks = 0;
                __instance.NumLongTasks = 0;
                __instance.NumShortTasks = 6;
                __instance.TaskBarMode = 0;
                break;
            //ケイドロモード
            case ModeId.CopsRobbers:
                __instance.PlayerSpeedMod = 1.5f;
                __instance.CrewLightMod = 2.0f;
                __instance.ImpostorLightMod = 1.5f;
                __instance.NumCommonTasks = 0;
                __instance.NumLongTasks = 0;
                __instance.NumShortTasks = 6;
                __instance.TaskBarMode = 0;
                break;
        }
        return false;
    }
}