using Hazel;
using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class PositionSwapper
    {
        public static void ResetCoolDown(){
            HudManagerStartPatch.PositionSwapperButton.MaxTimer = RoleClass.PositionSwapper.CoolTime;
            HudManagerStartPatch.PositionSwapperButton.Timer = RoleClass.PositionSwapper.CoolTime;
            RoleClass.PositionSwapper.ButtonTimer = DateTime.Now;
        }
        public static void EndMeeting(){
            ResetCoolDown();
        }
        public static void SwapStart(){
            List<PlayerControl> AlivePlayer = new();
            //List<PlayerControl> SwapperPlayer = new();
            AlivePlayer.Clear();
            //SwapperPlayer.Clear();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.isDead() && p.CanMove && !p.isImpostor())
                {
                    AlivePlayer.Add(p);
                }
                SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:"+p.PlayerId+"\n生存:"+!p.isDead());
            }
            //SwapperPlayer.Add(PlayerControl.LocalPlayer);
            var RandomPlayer = ModHelpers.GetRandom<PlayerControl>(AlivePlayer);
            var PushSwapper = PlayerControl.LocalPlayer;

            //SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:"+RandomPlayer.PlayerId+"\n生存:"+!RandomPlayer.isDead());
            CustomRPC.RPCProcedure.PositionSwapperTP(RandomPlayer.PlayerId, PushSwapper.PlayerId);
            //MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
        }
    }
}
