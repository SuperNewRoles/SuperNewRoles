using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;


namespace SuperNewRoles.Roles;

class NiceTeleporter
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.TeleporterButton.MaxTimer = RoleClass.NiceTeleporter.CoolTime;
        RoleClass.NiceTeleporter.ButtonTimer = DateTime.Now;
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
    public static bool IsNiceTeleporter(PlayerControl Player)
    {
        return Player.IsRole(RoleId.NiceTeleporter);
    }
    public static void EndMeeting()
    {
        HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.NiceTeleporter.CoolTime;
        RoleClass.NiceTeleporter.ButtonTimer = DateTime.Now;
    }
}