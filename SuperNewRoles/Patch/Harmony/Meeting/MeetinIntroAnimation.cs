using HarmonyLib;

namespace SuperNewRoles.Patch.Harmony
{
    [HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
    public static class MeetingIntroAnimationInitPatch
    {
        public static void Postfix(MeetingIntroAnimation __instance)
        {
            SuperNewRoles.Modules.ProctedMessager.StartMeeting(__instance);
        }
    }
}