using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class Arsonist
    {
        public static void ArsonistDouse(this PlayerControl target, PlayerControl source = null)
        {
            try
            {
                if (source == null) source = PlayerControl.LocalPlayer;
                MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ArsonistDouse);
                Writer.Write(source.PlayerId);
                Writer.Write(target.PlayerId);
                Writer.EndRPC();
                RPCProcedure.ArsonistDouse(source.PlayerId, target.PlayerId);
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogError(e);
            }
        }

        public static List<PlayerControl> GetDouseData(this PlayerControl player)
        {
            return RoleClass.Arsonist.DouseDatas.ContainsKey(player.PlayerId) ? RoleClass.Arsonist.DouseDatas[player.PlayerId] : new();
        }

        public static List<PlayerControl> GetUntarget()
        {
            return RoleClass.Arsonist.DouseDatas.ContainsKey(CachedPlayer.LocalPlayer.PlayerId)
                ? RoleClass.Arsonist.DouseDatas[CachedPlayer.LocalPlayer.PlayerId]
                : (new());
        }

        public static bool IsDoused(this PlayerControl source, PlayerControl target)
        {
            if (source == null || source.Data.Disconnected || target == null || target.IsDead() || target.IsBot()) return true;
            if (source.PlayerId == target.PlayerId) return true;
            if (RoleClass.Arsonist.DouseDatas.ContainsKey(source.PlayerId))
            {
                if (RoleClass.Arsonist.DouseDatas[source.PlayerId].IsCheckListPlayerControl(target))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<PlayerControl> GetIconPlayers(PlayerControl player = null)
        {
            if (player == null) player = PlayerControl.LocalPlayer;
            return RoleClass.Arsonist.DouseDatas.ContainsKey(player.PlayerId) ? RoleClass.Arsonist.DouseDatas[player.PlayerId] : (new());
        }
        public static bool IsViewIcon(PlayerControl player)
        {
            if (player == null) return false;
            foreach (var data in RoleClass.Arsonist.DouseDatas)
            {
                foreach (PlayerControl Player in data.Value)
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
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (player.PlayerId != Arsonist.PlayerId && !IsDoused(Arsonist, player))
                {
                    return false;
                }
            }
            return !Arsonist.IsDead();
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class HudManagerUpdatePatch
        {
            public static void Postfix()
            {
                if (RoleClass.Arsonist.DouseTarget == null) return;
                if (RoleClass.Arsonist.IsDouse)
                {
                    if (!(RoleClass.Arsonist.DouseTarget == HudManagerStartPatch.SetTarget(untarget: GetUntarget())))
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
                    }
                }
            }
        }

        public static bool IsArsonistWinFlag()
        {
            foreach (PlayerControl player in RoleClass.Arsonist.ArsonistPlayer)
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
                ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }
            return false;
        }

        public static void SetWinArsonist()
        {
            RoleClass.Arsonist.TriggerArsonistWin = true;
        }
        public static Dictionary<byte, float> ArsonistTimer = new();

        public static void ArsonistFinalStatus(PlayerControl __instance)
        {
            if (RoleClass.Arsonist.TriggerArsonistWin)
            {
                FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.Ignite;
            }
        }
    }
}