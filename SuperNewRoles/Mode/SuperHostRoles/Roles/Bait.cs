
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Bait
    {
        public static void MurderPostfix(PlayerControl __instance,PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (target.isRole(CustomRPC.RoleId.Bait))
            {
                new LateTask(() => {
                    RoleClass.Bait.ReportedPlayer.Add(target.PlayerId);
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, target.Data);
                    DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                    PlayerControl.LocalPlayer.RpcStartMeeting(__instance.Data);
                }, CustomOptions.BaitReportTime.getFloat(), "ReportBaitBody");
            }
        }
    }
}
