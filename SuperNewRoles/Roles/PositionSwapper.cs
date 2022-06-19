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
            AlivePlayer.Clear();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove && !p.isImpostor())
                {
                    AlivePlayer.Add(p);
                }
                SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:"+p.PlayerId+"\n生存:"+p.isAlive());
            }
            var RandomPlayer = ModHelpers.GetRandom<PlayerControl>(AlivePlayer);
            var PushSwapper = PlayerControl.LocalPlayer;
            CustomRPC.RPCProcedure.PositionSwapperTP(RandomPlayer.PlayerId, PushSwapper.PlayerId);

            //SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:"+RandomPlayer.PlayerId+"\n生存:"+!RandomPlayer.isDead());
        }
        public static Vector3 GetSwapPosition(byte SwapPlayerID, byte SwapperID){
            var SwapPlayer = ModHelpers.playerById(SwapPlayerID);
            var SwapperPlayer = ModHelpers.playerById(SwapperID);
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.PositionSwapper)){
                return SwapPlayer.transform.position;
            }
            else{
                return SwapperPlayer.transform.position;
            }
        }
    }
}
