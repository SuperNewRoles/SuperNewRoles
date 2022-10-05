using HarmonyLib;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    public static class NotBlackOut
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class CheckForEndVotingPatch
        {
            public static void Prefix()
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    MorePatch.MeetingEnd();
                }
            }
        }
    }
}