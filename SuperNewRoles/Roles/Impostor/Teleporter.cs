using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;

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
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
        {
            if (p.IsDead() || !p.CanMove)
                continue;
            aliveplayers.Add(p);
        }
        var player = ModHelpers.GetRandom(aliveplayers);

        MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.TeleporterTP);
        Writer.Write(player.PlayerId);
        Writer.EndRPC();
        RPCProcedure.TeleporterTP(player.PlayerId);
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
        ResetCooldown();
    }
}