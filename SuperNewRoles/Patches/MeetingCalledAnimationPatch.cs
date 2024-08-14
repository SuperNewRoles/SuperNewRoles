using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Patches;
public class MeetingCalledAnimationPatch
{
    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
    public static class InitPatch
    {
        public static void Postfix()
        {
            if (RoleClass.Revolutionist.MeetingTrigger is not null) Revolutionist.MeetingInit();
        }
    }

    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.CoShow))]
    public static class CoShowPatch
    {
        public static bool Prefix(ref Il2CppSystem.Collections.IEnumerator __result, MeetingCalledAnimation __instance, KillOverlay parent)
        {
            if (RoleClass.Revolutionist.MeetingTrigger is not null) __result = Revolutionist.ShowMeeting(__instance, parent).WrapToIl2Cpp();
            return RoleClass.Revolutionist.MeetingTrigger is null;
        }
    }
}