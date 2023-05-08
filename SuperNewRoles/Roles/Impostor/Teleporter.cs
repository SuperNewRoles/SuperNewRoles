using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles;

class Teleporter
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.TeleporterButton.MaxTimer = CoolTime;
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
    public static float CoolTime
    {
        get
        {
            switch (PlayerControl.LocalPlayer.GetRole())
            {
                case RoleId.Levelinger:
                case RoleId.Teleporter:
                    return RoleClass.Teleporter.CoolTime;
                case RoleId.NiceTeleporter:
                    return RoleClass.NiceTeleporter.CoolTime;
                case RoleId.TeleportingJackal:
                    return RoleClass.TeleportingJackal.CoolTime;
                default:
                    return 0f;
            }
        }
    }
    public static void EndMeeting()
    {
        HudManagerStartPatch.SheriffKillButton.MaxTimer = CoolTime;
        RoleClass.Teleporter.ButtonTimer = DateTime.Now;
    }
}