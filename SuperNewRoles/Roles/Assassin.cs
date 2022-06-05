using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    public static class Assassin
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (RoleClass.Assassin.DeadPlayer != null)
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (AmongUsClient.Instance.AmHost) {
                        RoleClass.Assassin.DeadPlayer.RpcInnerExiled();
                    }
                } else
                {
                    RoleClass.Assassin.DeadPlayer.Exiled();
                }
                RoleClass.Assassin.DeadPlayer = null;
            }
            if (RoleClass.Assassin.IsImpostorWin)
            {
                ShipStatus.Instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
            }
            var exile = Mode.SuperHostRoles.main.RealExiled;
            if (ModeHandler.isMode(ModeId.SuperHostRoles) && exile != null && exile.isRole(CustomRPC.RoleId.Assassin))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    new LateTask(() =>
                    {
                        MeetingRoomManager.Instance.AssignSelf(exile, null);
                        DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(exile);
                        exile.RpcStartMeeting(null);
                    }, 10.5f);
                }
                RoleClass.Assassin.TriggerPlayer = exile;
            }
        }
    }
}
