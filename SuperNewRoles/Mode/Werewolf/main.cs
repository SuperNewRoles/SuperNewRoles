using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnhollowerBaseLib;

namespace SuperNewRoles.Mode.Werewolf
{
    class Main
    {
        public static bool IsChatBlock(PlayerControl sourcePlayer)
        {
            if (MeetingHud.Instance == null) return false;
            if (ModeHandler.IsMode(ModeId.Werewolf) && MeetingHud.Instance.CurrentState == MeetingHud.VoteStates.Discussion) return PlayerControl.LocalPlayer.IsImpostor() && sourcePlayer.IsImpostor();
            return false;
        }
        public static bool IsUseButton()
        {
            if (MeetingHud.Instance == null) return true;
            if (MeetingHud.Instance.CurrentState == MeetingHud.VoteStates.Discussion) return true;
            return false;
        }
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        class TranslationControllerGetStringPatch
        {
            static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id, [HarmonyArgument(1)] Il2CppReferenceArray<Il2CppSystem.Object> parts)
            {
                if (id is StringNames.MeetingVotingBegins && ModeHandler.IsMode(ModeId.Werewolf, false) && parts.Count > 0) __result = string.Format(ModTranslation.GetString("WerewolfAbilityTime"), parts[0]);
            }
        }
    }
}
