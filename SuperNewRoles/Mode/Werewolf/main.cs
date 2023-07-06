using System;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace SuperNewRoles.Mode.Werewolf;

class Main
{
    public static bool IsChatBlock(PlayerControl sourcePlayer, string text)
    {
        if (MeetingHud.Instance == null) return false;
        if (!ModeHandler.IsMode(ModeId.Werewolf)) return false;
        if (sourcePlayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.SoothSayer) && text.EndsWith(ModTranslation.GetString("SoothSayerCrewmateText")))
            {
                return false;
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.SpiritMedium) && (text.EndsWith(ModTranslation.GetString("SoothSayerCrewmateText")) || text.EndsWith(ModTranslation.GetString("SoothSayerNotCrewmateText"))))
            {
                return false;
            }
        }
        if (ModeHandler.IsMode(ModeId.Werewolf) && MeetingHud.Instance.CurrentState == MeetingHud.VoteStates.Discussion) return (!PlayerControl.LocalPlayer.IsImpostor() && PlayerControl.LocalPlayer.IsAlive()) || !sourcePlayer.IsImpostor();
        return false;
    }
    public static bool IsUseButton()
    {
        if (MeetingHud.Instance == null) return true;
        if (!ModeHandler.IsMode(ModeId.Werewolf)) return true;
        if (MeetingHud.Instance.CurrentState == MeetingHud.VoteStates.Discussion) return true;
        return false;
    }
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class TranslationControllerGetStringPatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id, [HarmonyArgument(1)] Il2CppReferenceArray<Il2CppSystem.Object> parts)
        {
            if (id is StringNames.MeetingVotingBegins && ModeHandler.IsMode(ModeId.Werewolf, false))
            {
                float num3 = (float)GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.DiscussionTime) - MeetingHud.Instance.discussionTimer;
                __result = string.Format(ModTranslation.GetString("WerewolfAbilityTime"), Mathf.CeilToInt(num3));
            }
        }
    }
}