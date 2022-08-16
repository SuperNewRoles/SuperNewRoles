using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class PositionSwapper
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.PositionSwapperButton.MaxTimer = RoleClass.PositionSwapper.CoolTime;
            HudManagerStartPatch.PositionSwapperButton.Timer = RoleClass.PositionSwapper.CoolTime;
            RoleClass.PositionSwapper.ButtonTimer = DateTime.Now;
        }
        public static void EndMeeting()
        {
            ResetCoolDown();
        }
        public static void SwapStart()
        {
            List<PlayerControl> AlivePlayer = new();
            AlivePlayer.Clear();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsAlive() && p.CanMove && !p.IsImpostor())
                {
                    AlivePlayer.Add(p);
                }
                SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:" + p.PlayerId + "\n生存:" + p.IsAlive());
            }
            var RandomPlayer = ModHelpers.GetRandom<PlayerControl>(AlivePlayer);
            var PushSwapper = PlayerControl.LocalPlayer;
            RPCProcedure.PositionSwapperTP(RandomPlayer.PlayerId, PushSwapper.PlayerId);

            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.PositionSwapperTP, SendOption.Reliable, -1);
            Writer.Write(RandomPlayer.PlayerId);
            Writer.Write(PushSwapper.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
            //SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:"+RandomPlayer.PlayerId+"\n生存:"+!RandomPlayer.IsDead());
        }
        /*public static Vector3 GetSwapPosition(byte SwapPlayerID, byte SwapperID){
            var SwapPlayer = ModHelpers.PlayerById(SwapPlayerID);
            var SwapperPlayer = ModHelpers.PlayerById(SwapperID);
            if (PlayerControl.LocalPlayer.IsRole(RoleId.PositionSwapper)){
                return SwapPlayer.transform.position;
            }
            else{
                return SwapperPlayer.transform.position;
            }
        }*/
    }
}