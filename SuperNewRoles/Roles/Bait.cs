using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Patch;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Bait
    {
        public class BaitUpdate
        {
            public static void Postfix(PlayerControl __instance)
            {
                    bool IsError = false;
                    RoleClass.Bait.ReportTime -= Time.fixedDeltaTime;
                    SuperNewRolesPlugin.Logger.LogInfo(RoleClass.Bait.ReportTime);
                    DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == PlayerControl.LocalPlayer.PlayerId)?.FirstOrDefault();
                    if (deadPlayer.killerIfExisting != null && RoleClass.Bait.ReportTime <= 0f)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ReportDeadBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(deadPlayer.killerIfExisting.PlayerId);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RoleClass.Bait.Reported = true;
                        CustomRPC.RPCProcedure.ReportDeadBody(deadPlayer.killerIfExisting.PlayerId, PlayerControl.LocalPlayer.PlayerId);
                    }
            }
        }
    }
}
