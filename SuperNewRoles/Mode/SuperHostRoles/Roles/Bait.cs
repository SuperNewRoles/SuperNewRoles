using BepInEx.IL2CPP.Utils;
using Hazel;
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
            if (target.isRole(CustomRPC.RoleId.Bait))
            {
                AmongUsClient.Instance.StartCoroutine(ReportbaitBody(__instance,target));
                IEnumerator ReportbaitBody(PlayerControl __instance,PlayerControl target)
                {
                    yield return new WaitForSeconds(3);
                    RoleClass.Bait.ReportedPlayer.Add(target.PlayerId);
                    __instance.CmdReportDeadBody(target.Data);
                }
            }
        }
    }
}
