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

        /*public static void RpcCheckExile(this PlayerControl __instance)
        {
            if ()
            {
                new LateTask(() =>
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        MeetingRoomManager.Instance.AssignSelf(__instance, null);
                        FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                        __instance.RpcStartMeeting(null);
                    }
                }, 0.5f);
            }
        }*/
    }
}