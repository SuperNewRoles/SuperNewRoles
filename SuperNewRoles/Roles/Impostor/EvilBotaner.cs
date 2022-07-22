using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnhollowerBaseLib;

namespace SuperNewRoles.Roles
{
    public static class EvilBotaner
    {
        //SNR
        public static void EvilBotanerStartMeeting(PlayerControl sourceId)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                //RPCProcedure.UncheckedMeeting(sourceId);
                MeetingRoomManager.Instance.AssignSelf(sourceId, null);
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(sourceId);
                sourceId.RpcStartMeeting(null);
            }
        }
        //SHR
        public static void EvilBotanerStartMeetingSHR(this PlayerControl __instance)
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