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
            List<PlayerControl> AlivePlayer = new List<PlayerControl>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove && !p.isRole(CustomRPC.RoleId.PositionSwapper))
                {
                    AlivePlayer.Add(p);
                }
            }
            var RandomPlayer = ModHelpers.GetRandom<PlayerControl>(AlivePlayer);
            var Player = ModHelpers.playerById(RandomPlayer.PlayerId);
            var PlayerPosition = Player.transform.position;
            var RandomPlayer2 = CachedPlayer.LocalPlayer;
            var Player2 = ModHelpers.playerById(RandomPlayer2.PlayerId);
            var PlayerPosition2 = Player2.transform.position;

            RandomPlayer2.transform.position = PlayerPosition;
            RandomPlayer.transform.position = PlayerPosition2;

            /*if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.PositionSwapper)) {
                CachedPlayer.LocalPlayer.transform.position = PlayerPosition;
            }
            else {
                CachedPlayer.LocalPlayer.transform.position = PlayerPosition2;
            }*/

            /*if (SubmergedCompatibility.isSubmerged())
            {
                SubmergedCompatibility.ChangeFloor(SubmergedCompatibility.GetFloor(Player));
            }*/
            CustomRPC.RPCProcedure.PositionSwapperTP();
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
        }
    }
}