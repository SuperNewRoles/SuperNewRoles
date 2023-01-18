using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Patches;
public class MeetingCalledAnimationPatch
{
    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
    public static class InitPatch
    {
        public static void Postfix(MeetingCalledAnimation __instance, GameData.PlayerInfo reportInfo)
        {
            Revolutionist.MeetingInit(__instance, reportInfo);
        }
    }

    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.CoShow))]
    public static class CoShowPatch
    {
        public static bool Prefix(ref Il2CppSystem.Collections.IEnumerator __result, MeetingCalledAnimation __instance, KillOverlay parent)
        {
            __result = Revolutionist.ShowMeeting(__instance, parent).WrapToIl2Cpp();
            return false;
        }
    }
}