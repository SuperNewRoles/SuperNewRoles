using Hazel;
using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;
using System.Text;

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
            List<PlayerControl> aliveplayers = new List<PlayerControl>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove)
                {
                    aliveplayers.Add(p);
                }
            }
            var player = ModHelpers.GetRandom<PlayerControl>(aliveplayers);
            CustomRPC.RPCProcedure.TeleporterTP(player.PlayerId);

            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static bool IsNiceTeleporter(PlayerControl Player)
        {
            if (RoleClass.NiceTeleporter.NiceTeleporterPlayer.IsCheckListPlayerControl(Player))
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
