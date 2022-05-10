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
using SuperNewRoles.Helpers;
using static SuperNewRoles.ModHelpers;

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

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!ModeHandler.isMode(ModeId.Default)) return true;
            if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.isAlive() && PlayerControl.LocalPlayer.CanMove)
            {
                if (!(__instance.currentTarget.isRole(CustomRPC.RoleId.Bait) || __instance.currentTarget.isRole(CustomRPC.RoleId.NiceRedRidingHood)) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Vampire))
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.getCoolTime(PlayerControl.LocalPlayer);
                    RoleClass.Vampire.target = __instance.currentTarget;
                    RoleClass.Vampire.KillTimer = DateTime.Now;
                    RoleClass.Vampire.Timer = RoleClass.Vampire.KillDelay;
                    return false;
                }
                bool showAnimation = true;
                /*
                if (PlayerControl.LocalPlayer.isRole(RoleType.Ninja) && Ninja.isStealthed(PlayerControl.LocalPlayer))
                {
                    showAnimation = false;
                }
                */

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderAttemptResult res = ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
                // Handle blank kill
                if (res == MurderAttemptResult.BlankKill)
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.getCoolTime(PlayerControl.LocalPlayer);
                }
                __instance.SetTarget(null);
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch
    {
        public static bool isKill = false;
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            SuperNewRolesPlugin.Logger.LogInfo("キル:" + __instance.name + "(" + __instance.PlayerId + ")" + " => " + target.name + "(" + target.PlayerId + ")");
            if (__instance.isDead()) return false;
            if (__instance.PlayerId == target.PlayerId) { __instance.RpcMurderPlayer(target); return false; }
            if (!RoleClass.IsStart && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                return false;
            if (!AmongUsClient.Instance.AmHost)
            {
                return true;
            }
            if (ModeHandler.isMode(ModeId.BattleRoyal))
            {
                if (isKill)
                {
                    return false;
                }
                if (Mode.BattleRoyal.main.StartSeconds <= 0)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("キルでした:" + __instance.name + "(" + __instance.PlayerId + ")" + " => " + target.name + "(" + target.PlayerId + ")");
                    if (Mode.BattleRoyal.main.IsTeamBattle)
                    {
                        foreach (List<PlayerControl> teams in Mode.BattleRoyal.main.Teams)
                        {
                            if (teams.Count > 0)
                            {
                                if (teams.IsCheckListPlayerControl(__instance) && teams.IsCheckListPlayerControl(target))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    if (__instance.PlayerId != 0)
                    {
                        if (__instance.isAlive() && target.isAlive())
                        {
                            __instance.RpcMurderPlayer(target);
                            target.Data.IsDead = true;
                        }
                    } else
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("レートタスク:"+ (AmongUsClient.Instance.Ping / 1000f) * 2f);
                        isKill = true;
                        new LateTask(() => {
                            if (__instance.isAlive() && target.isAlive())
                            {
                                __instance.RpcMurderPlayer(target);
                            }
                            isKill = false;
                            }, (AmongUsClient.Instance.Ping / 1000f)* 1.1f);
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            if (ModeHandler.isMode(ModeId.Zombie)) return false;
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (__instance.isRole(RoleId.FalseCharges))
                {
                    target.RpcMurderPlayer(__instance);
                    RoleClass.FalseCharges.FalseChargePlayers[__instance.PlayerId] = target.PlayerId;
                    RoleClass.FalseCharges.AllTurns[__instance.PlayerId] = RoleClass.FalseCharges.DefaultTurn;
                    return false;
                }
            }
            if (ModeHandler.isMode(ModeId.Detective) && target.PlayerId == Mode.Detective.main.DetectivePlayer.PlayerId) return false;
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (__instance.isRole(RoleId.Egoist) && !RoleClass.Egoist.UseKill)
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
                if (target.isRole(RoleId.StuntMan) && !__instance.isRole(RoleId.OverKiller))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                    {
                        if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            RoleClass.StuntMan.GuardCount[target.PlayerId] = (int)CustomOptions.StuntManMaxGuardCount.getFloat() - 1;
                            target.RpcProtectPlayer(target, 0);
                            new LateTask(() => __instance.RpcMurderPlayer(target), 0.1f);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0))
                            {
                                RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                                target.RpcProtectPlayer(target, 0);
                                new LateTask(() => __instance.RpcMurderPlayer(target), 0.1f);
                                return false;
                            }
                        }
                    }
                }
                if (target.isRole(RoleId.MadStuntMan) && !__instance.isRole(RoleId.OverKiller))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.MadStuntmanGuard, __instance))
                    {
                        if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            target.RpcProtectPlayer(target, 0);
                            new LateTask(() => __instance.RpcMurderPlayer(target), 0.1f);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.MadStuntMan.GuardCount[target.PlayerId] <= 0))
                            {
                                RoleClass.MadStuntMan.GuardCount[target.PlayerId]--;
                                target.RpcProtectPlayer(target, 0);
                                new LateTask(() => __instance.RpcMurderPlayer(target), 0.1f);
                                return false;
                            }
                        }
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
            if (PlayerControl.GameOptions.killCooldown == time && !RoleClass.IsCoolTimeSetted)
            {
                __instance.SetKillTimerUnchecked(RoleHelpers.GetEndMeetingKillCoolTime(__instance), RoleHelpers.GetEndMeetingKillCoolTime(__instance));
                RoleClass.IsCoolTimeSetted = true;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;
        public static void Prefix(PlayerControl __instance, PlayerControl target)
        {
            if (ModeHandler.isMode(ModeId.Default))
            {
                target.resetChange();
                if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.SideKiller))
                    {
                        var sideplayer = RoleClass.SideKiller.getSidePlayer(PlayerControl.LocalPlayer);
                        if (sideplayer != null)
                        {
                            if (!RoleClass.SideKiller.IsUpMadKiller)
                            {
                                sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                RoleClass.SideKiller.IsUpMadKiller = true;
                            }
                        }
                    }
                } else if(__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    if (__instance.isRole(RoleId.EvilGambler))
                    {
                        if (RoleClass.EvilGambler.GetSuc())
                        {
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.SucCool);
                        } else
                        {
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.NotSucCool);
                        }
                    }
                }
            }
        }
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
                Levelinger.MurderPlayer(__instance,target);
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
            if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (__instance.isImpostor() && !__instance.isRole(RoleId.EvilGambler))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleHelpers.getCoolTime(__instance),RoleHelpers.getCoolTime(__instance));
                }
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