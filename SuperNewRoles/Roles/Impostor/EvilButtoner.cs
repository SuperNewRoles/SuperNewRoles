using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public static class EvilButtoner
    {
        //SNR
        public static void EvilButtonerStartMeeting(PlayerControl source)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ReportDeadBody);
                writer.Write(source.PlayerId);
                writer.Write(source.PlayerId);
                writer.EndRPC();
                RPCProcedure.ReportDeadBody(source.PlayerId, source.PlayerId);
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
            }, 0.5f, "EvilButtonerStartMeetingSHR");
        }
    }
}