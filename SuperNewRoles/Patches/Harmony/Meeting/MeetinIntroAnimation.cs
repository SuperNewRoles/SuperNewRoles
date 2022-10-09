using HarmonyLib;

namespace SuperNewRoles.Patches.Harmony
{
    [HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
    public static class MeetingIntroAnimationInitPatch
    {
        public static void Postfix(MeetingIntroAnimation __instance)
        {
            Modules.ProctedMessager.StartMeeting(__instance);
        }
    }
}