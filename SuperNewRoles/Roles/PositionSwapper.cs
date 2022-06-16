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
            RoleClass.PositionSwapper.ButtonTimer = DateTime.Now;
        }
        public static void EndMeeting(){
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Teleporter.CoolTime;
            RoleClass.Teleporter.ButtonTimer = DateTime.Now;
        }
        public static void SwapStart(){
            List<PlayerControl> AlivePlayer = new List<PlayerControl>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove)
                {
                    AlivePlayer.Add(p);
                }
            }
            var RandomPlayer = ModHelpers.GetRandom<PlayerControl>(AlivePlayer);
            var Player = ModHelpers.playerById(RandomPlayer.PlayerId);
            Vector3 PlayerPosition = Player.transform.position;
            var RandomPlayer2 = CachedPlayer.LocalPlayer;
            var Player2 = ModHelpers.playerById(RandomPlayer2.PlayerId);
            Vector3 PlayerPosition2 = Player2.transform.position;


            CachedPlayer.LocalPlayer.transform.position = PlayerPosition2;
            if (SubmergedCompatibility.isSubmerged())
            {
                SubmergedCompatibility.ChangeFloor(SubmergedCompatibility.GetFloor(p));
            }
            CustomRPC.RPCProcedure.PositionSwapperTP(Player.PlayerId);
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
        }
    }
}