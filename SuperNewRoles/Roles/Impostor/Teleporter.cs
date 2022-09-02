using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;


namespace SuperNewRoles.Roles
{
    class Teleporter
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.TeleporterButton.MaxTimer = RoleClass.Teleporter.CoolTime;
            RoleClass.Teleporter.ButtonTimer = DateTime.Now;
        }
        public static void TeleportStart()
        {
            List<PlayerControl> aliveplayers = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsAlive() && p.CanMove)
                {
                    aliveplayers.Add(p);
                }
            }
            var player = ModHelpers.GetRandom(aliveplayers);
            RPCProcedure.TeleporterTP(player.PlayerId);

            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TeleporterTP, SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static bool IsTeleporter(PlayerControl Player)
        {
            return Player.IsRole(RoleId.Teleporter);
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Teleporter.CoolTime;
            RoleClass.Teleporter.ButtonTimer = DateTime.Now;
        }
    }
}