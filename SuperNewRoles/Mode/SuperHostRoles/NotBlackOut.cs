
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomRPC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles { 
    public static class NotBlackOut
    {

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class CheckForEndVotingPatch
        {
            public static void Prefix(MeetingHud __instance)
			{
				if (!AmongUsClient.Instance.AmHost) return ;
				if (Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles))
                {
                    EndMeetingPatch();
                }
            }

        }
        public static void EndMeetingPatch()
        {/*
            //霊界用暗転バグ対処
            foreach (var pc in CachedPlayer.AllPlayers)
                if (IsAntiBlackOut(pc) && pc.isDead()) pc.ResetPlayerCam(19f);*/
        }
		public static bool IsAntiBlackOut(PlayerControl player)
        {
			if (player.IsMod()) return false;
            /*
			if (player.isRole(RoleId.Egoist)) return true;
			if (player.isRole(RoleId.Sheriff)) return true;
            if (player.isRole(RoleId.truelover)) return true;
            if (player.isRole(RoleId.FalseCharges)) return true;
            if (player.isRole(RoleId.RemoteSheriff)) return true;
            */
            return false;
        }
        public static void ResetPlayerCam(this PlayerControl pc, float delay = 0f)
        {
            if (pc == null || !AmongUsClient.Instance.AmHost || pc.AmOwner) return;
            int clientId = pc.getClientId();

            byte reactorId = 3;
            if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;

            
            new LateTask(() => {
                MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, clientId);
                MessageExtensions.WriteNetObject(MurderWriter, pc);
                AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
            }, delay, "Murder To Reset Cam");
            
            new LateTask(() => {
                SuperNewRolesPlugin.Logger.LogInfo("SetDesyncSabotage");
                MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageWriter, pc);
                SabotageWriter.Write((byte)128);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
            }, delay, "Reactor Desync");
            new LateTask(() => {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, 0.1f + delay, "Fix Desync Reactor");

            if (PlayerControl.GameOptions.MapId == 4) //Airship用
                new LateTask(() => {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, 0.1f + delay, "Fix Desync Reactor 2");
        }
    }
}
