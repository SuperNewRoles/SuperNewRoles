using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using static SuperNewRoles.Helpers.DesyncHelpers;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch
    {
        public static bool Prefix(PlayerControl __instance,[HarmonyArgument(0)] PlayerControl target)
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles)) return false;
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (__instance.isDead()) return false;
            if (__instance.PlayerId == target.PlayerId) { __instance.RpcMurderPlayer(target); return false; }
            if (!RoleClass.IsStart && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                return false;
            if (!AmongUsClient.Instance.AmHost)
            {
                return true;
            }
            if (ModeHandler.isMode(ModeId.BattleRoyal)) return true;
            if (target.isRole(RoleId.StuntMan) && !__instance.isRole(RoleId.OverKiller))
            {
                if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                {
                    if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                    {
                        RoleClass.StuntMan.GuardCount[target.PlayerId] = (int)CustomOptions.StuntManMaxGuardCount.getFloat() - 1;
                        target.RpcProtectPlayer(target, 0);
                    }
                    else
                    {
                        if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0))
                        {
                            RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                            target.RpcProtectPlayer(target, 0);
                        }
                    }
                }
            }
            if (ModeHandler.isMode(ModeId.Detective) && target.PlayerId == Mode.Detective.main.DetectivePlayer.PlayerId) return false;
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (__instance.isRole(RoleId.Egoist))
                {
                    return false;
                }
                else if (__instance.isRole(RoleId.truelover))
                {
                    if (!__instance.IsLovers())
                    {
                        if (target == null || target.IsLovers() || RoleClass.truelover.CreatePlayers.Contains(__instance.PlayerId)) return false;
                        RoleClass.truelover.CreatePlayers.Add(__instance.PlayerId);
                        RoleHelpers.SetLovers(__instance, target);
                        RoleHelpers.SetLoversRPC(__instance, target);
                        //__instance.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                        Mode.SuperHostRoles.FixedUpdate.SetRoleNames();
                    }
                    return false;
                }
                else if (__instance.isRole(RoleId.Sheriff))
                {
                    if (!RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId) || RoleClass.Sheriff.KillCount[__instance.PlayerId] >= 1)
                    {
                        if (!Sheriff.IsSheriffKill(target) || target.isRole(RoleId.Sheriff))
                        {
                            FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.SheriffMisFire;
                            __instance.RpcMurderPlayer(__instance);
                            return false;
                        }
                        else
                        {
                            FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.SheriffKill;
                            if (RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId))
                            {
                                RoleClass.Sheriff.KillCount[__instance.PlayerId]--;
                            }
                            else
                            {
                                RoleClass.Sheriff.KillCount[__instance.PlayerId] = (int)CustomOptions.SheriffKillMaxCount.getFloat() - 1;
                            }
                            __instance.RpcMurderPlayer(target);
                            return false;
                        }
                    } else
                    {
                        return false;
                    }
                }
            }
            if (__instance.isRole(RoleId.OverKiller))
            {
                __instance.RpcMurderPlayer(target);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.Disconnected && p.PlayerId != target.PlayerId)
                    {
                        if (p.PlayerId != 0)
                        {
                            for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                            {
                                __instance.RPCMurderPlayerPrivate(target,p);
                            }
                        } else
                        {
                            for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                            {
                                __instance.MurderPlayer(target);
                            }
                        }
                    }
                }
                return false;
            }
            if (!ModeHandler.isMode(ModeId.Default))
            {
                __instance.RpcMurderPlayer(target);
                return false;
            } else
            {
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static class DiePatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (target.isRole(RoleId.truelover))
                {
                    target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                }
            }
            else
            {
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (PlayerControl.GameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (__instance.isRole(RoleId.SerialKiller)) addition = RoleClass.SerialKiller.KillTime;
                else if (__instance.isRole(RoleId.OverKiller)) addition = RoleClass.OverKiller.KillCoolTime;
            }
            float max = Mathf.Max(PlayerControl.GameOptions.KillCooldown * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, max), max);
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // SuperNewRolesPlugin.Logger.LogInfo("MurderPlayer発生！元:" + __instance.getDefaultName() + "、ターゲット:" + target.getDefaultName());
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;

            SerialKiller.MurderPlayer(__instance,target);

            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MurderPlayer.Postfix(__instance, target);
                }
            }
            else if (ModeHandler.isMode(ModeId.Detective))
            {
                Mode.Detective.main.MurderPatch(target);
            }
            else if (ModeHandler.isMode(ModeId.Default))
            {
                if (RoleClass.Lovers.SameDie && target.IsLovers())
                {
                    if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = target.GetOneSideLovers();
                        if (SideLoverPlayer.isAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, Hazel.SendOption.Reliable, -1);
                            writer.Write(SideLoverPlayer.PlayerId);
                            writer.Write(SideLoverPlayer.PlayerId);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.RPCMurderPlayer(SideLoverPlayer.PlayerId, SideLoverPlayer.PlayerId, byte.MaxValue);
                        }
                    }
                }
                if (target.IsQuarreled())
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var Side = RoleHelpers.GetOneSideQuarreled(target);
                        if (Side.isDead())
                        {
                            RPCProcedure.ShareWinner(target.PlayerId);

                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                            Writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            Roles.RoleClass.Quarreled.IsQuarreledWin = true;
                            ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                        }
                    }
                }
                Minimalist.MurderPatch.Postfix(__instance);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.Exiled;
            if (ModeHandler.isMode(ModeId.Default))
            {

                if (RoleClass.Lovers.SameDie && __instance.IsLovers())
                {
                    if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = __instance.GetOneSideLovers();
                        if (SideLoverPlayer.isAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ExiledRPC, Hazel.SendOption.Reliable, -1);
                            writer.Write(SideLoverPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.ExiledRPC(SideLoverPlayer.PlayerId);
                        }
                    }
                }
            }
        }
    }
    public static class PlayerControlFixedUpdatePatch
    {
        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new List<PlayerControl>();
            }

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }
    }
}