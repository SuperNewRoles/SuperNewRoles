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
            ResetCoolDown();
        }
        public static void SwapStart(){
            List<PlayerControl> AlivePlayer = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove && !p.isRole(CustomRPC.RoleId.PositionSwapper))
                {
                    AlivePlayer.Add(p);
                }
            }
            var RandomPlayer = ModHelpers.GetRandom<PlayerControl>(AlivePlayer);
            /*if (SubmergedCompatibility.isSubmerged())
            {
                SubmergedCompatibility.ChangeFloor(SubmergedCompatibility.GetFloor(Player));
            }*/
            CustomRPC.RPCProcedure.PositionSwapperTP(RandomPlayer.PlayerId);
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
        }
    }
}
