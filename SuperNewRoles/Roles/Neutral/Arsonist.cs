using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using Il2CppSystem;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles;

public static class Arsonist
{
    public static void ArsonistDouse(this PlayerControl target, PlayerControl source = null)
    {
        try
        {
            if (source == null) source = PlayerControl.LocalPlayer;
            MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.ArsonistDouse);
            Writer.Write(source.PlayerId);
            Writer.Write(target.PlayerId);
            Writer.EndRPC();
            RPCProcedure.ArsonistDouse(source.PlayerId, target.PlayerId);
        }
        catch (System.Exception e)
        {
            SuperNewRolesPlugin.Logger.LogError(e);
        }
    }

    public static List<PlayerControl> GetDouseData(this PlayerControl player) =>
        RoleClass.Arsonist.DouseData[player.PlayerId];

    public static List<PlayerControl> GetUntarget() => GetDouseData(PlayerControl.LocalPlayer);

    public static bool IsDoused(this PlayerControl source, PlayerControl target)
    {
        if (source == null || source.Data.Disconnected || target == null || target.IsDead() || target.IsBot()) return true;
        if (source.PlayerId == target.PlayerId) return true;
        if (RoleClass.Arsonist.DouseData.Contains(source.PlayerId))
        {
            if (RoleClass.Arsonist.DouseData[source.PlayerId].IsCheckListPlayerControl(target))
            {
                return true;
            }
        }
        return false;
    }

    public static List<PlayerControl> GetIconPlayers(PlayerControl player = null)
    {
        if (player == null) player = PlayerControl.LocalPlayer;
        return RoleClass.Arsonist.DouseData[player.PlayerId];
    }
    public static bool IsViewIcon(PlayerControl player)
    {
        if (player == null) return false;
        foreach (KeyValuePair<byte, List<PlayerControl>> data in RoleClass.Arsonist.DouseData)
        {
            foreach (PlayerControl Player in data.Value.AsSpan())
            {
                if (player.PlayerId == Player.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsButton()
    {
        return ModeHandler.IsMode(ModeId.Default) && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist);
    }

    public static bool IseveryButton()
    {
        return (ModeHandler.IsMode(ModeId.SuperHostRoles) && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist)) || (ModeHandler.IsMode(ModeId.Default) && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist));

    }

    public static bool IsWin(PlayerControl Arsonist)
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.PlayerId != Arsonist.PlayerId && !IsDoused(Arsonist, player))
            {
                return false;
            }
        }
        return !Arsonist.IsDead();
    }

    public static void HudUpdate()
    {
        if (RoleClass.Arsonist.DouseTarget is null) return;
        if (RoleClass.Arsonist.IsDouse)
        {
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
            Vector2 vector = RoleClass.Arsonist.DouseTarget.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num || PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
            {
                RoleClass.Arsonist.IsDouse = false;
                HudManagerStartPatch.ArsonistDouseButton.Timer = 0;
                SuperNewRolesPlugin.Logger.LogInfo("アーソ二ストが塗るのをやめた");
                return;
            }
            if (HudManagerStartPatch.ArsonistDouseButton.Timer <= 0.1f)
            {
                HudManagerStartPatch.ArsonistDouseButton.MaxTimer = RoleClass.Arsonist.CoolTime;
                HudManagerStartPatch.ArsonistDouseButton.Timer = HudManagerStartPatch.ArsonistDouseButton.MaxTimer;
                HudManagerStartPatch.ArsonistDouseButton.actionButton.cooldownTimerText.color = Color.white;
                RoleClass.Arsonist.DouseTarget.ArsonistDouse();
                SuperNewRolesPlugin.Logger.LogInfo("アーソ二ストが塗った:" + RoleClass.Arsonist.DouseTarget);
                RoleClass.Arsonist.DouseTarget = null;
            }
        }
    }

    public static bool IsArsonistWinFlag()
    {
        foreach (PlayerControl player in RoleClass.Arsonist.ArsonistPlayer.AsSpan())
        {
            if (IsWin(player))
            {
                SuperNewRolesPlugin.Logger.LogInfo("アーソニストが勝利条件を達成");
                return true;
            }
        }
        return false;
    }

    public static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
    {
        if (RoleClass.Arsonist.TriggerArsonistWin)
        {
            SuperNewRolesPlugin.Logger.LogInfo("CheckAndEndGame");
            __instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
            return true;
        }
        return false;
    }

    public static void SettingAfire()
    {
        foreach (KeyValuePair<byte, List<PlayerControl>> data in RoleClass.Arsonist.DouseData)
        {
            foreach (PlayerControl player in data.Value.AsSpan())
            {
                if (player == null) continue;
                if (player.IsDead()) continue;
                if (player == PlayerControl.LocalPlayer && player.IsRole(RoleId.Arsonist)) continue;
                if (player.IsRole(RoleId.FireFox)) continue; // 炎狐が燃やされるわけないだろ!!

                ModHelpers.CheckMurderAttemptAndKill(player, player);

                player.RpcSetFinalStatus(FinalStatus.Ignite);
            }
        }
    }

    public static void SetWinArsonist()
    {
        RoleClass.Arsonist.TriggerArsonistWin = true;
    }
    public static Dictionary<byte, float> ArsonistTimer = new();
}