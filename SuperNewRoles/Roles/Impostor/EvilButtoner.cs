using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public static class EvilButtoner
    {
        //SNR
        public static void EvilButtonerStartMeeting(PlayerControl sourceId)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                MeetingRoomManager.Instance.AssignSelf(sourceId, null);
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(sourceId);
                sourceId.RpcStartMeeting(null);
            }
        }
        //SHR
        public static void EvilButtonerStartMeetingSHR(this PlayerControl __instance)
        {
            new LateTask(() =>
            {
                MeetingRoomManager.Instance.AssignSelf(__instance, null);
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                __instance.RpcStartMeeting(null);
            }, 0.5f);
        }
    }
}