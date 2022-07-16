using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class NiceTeleporter
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.TeleporterButton.MaxTimer = RoleClass.NiceTeleporter.CoolTime;
            RoleClass.NiceTeleporter.ButtonTimer = DateTime.Now;
        }
        public static void TeleportStart()
        {
            List<PlayerControl> aliveplayers = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove)
                {
                    aliveplayers.Add(p);
                }
            }
            var player = ModHelpers.GetRandom<PlayerControl>(aliveplayers);
            CustomRPC.RPCProcedure.TeleporterTP(player.PlayerId);

            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static bool IsNiceTeleporter(PlayerControl Player)
        {
            if (Player.isRole(RoleId.NiceTeleporter))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.NiceTeleporter.CoolTime;
            RoleClass.NiceTeleporter.ButtonTimer = DateTime.Now;
        }
    }
}
