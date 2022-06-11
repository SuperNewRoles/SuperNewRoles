using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
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
                SuperNewRolesPlugin.Logger.LogInfo(RoleClass.Bait.ReportTime);
                RoleClass.Bait.ReportTime -= Time.fixedDeltaTime;
                DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId)?.FirstOrDefault();

                if (deadPlayer.killerIfExisting != null && RoleClass.Bait.ReportTime <= 0f)
                {

                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.BaitReport)) { 
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ReportDeadBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(deadPlayer.killerIfExisting.PlayerId);
                        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        CustomRPC.RPCProcedure.ReportDeadBody(deadPlayer.killerIfExisting.PlayerId, CachedPlayer.LocalPlayer.PlayerId);
                    }
                    RoleClass.Bait.Reported = true;
                }
            }
        }
    }
}
