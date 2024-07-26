using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles;

class PositionSwapper
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.PositionSwapperButton.MaxTimer = RoleClass.PositionSwapper.CoolTime;
        HudManagerStartPatch.PositionSwapperButton.Timer = RoleClass.PositionSwapper.CoolTime;
        RoleClass.PositionSwapper.ButtonTimer = DateTime.Now;
    }
    public static void EndMeeting()
    {
        ResetCooldown();
    }
    public static void SwapStart()
    {
        List<PlayerControl> AlivePlayer = new();
        AlivePlayer.Clear();
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
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

        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PositionSwapperTP, SendOption.Reliable, -1);
        Writer.Write(RandomPlayer.PlayerId);
        Writer.Write(PushSwapper.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(Writer);
        //SuperNewRolesPlugin.Logger.LogInfo("ポジションスワップ:"+RandomPlayer.PlayerId+"\n生存:"+!RandomPlayer.IsDead());
    }
}