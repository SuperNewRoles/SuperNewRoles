using HarmonyLib;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class Blackoutfix
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        public class CheckForEndVotingPatch
        {
            public static void Prefix()
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    EndMeetingPatch();
                }
            }
        }
        public static void EndMeetingPatch()
        {
            //BotManager.Spawn("暗転対策");
        }
    }
}