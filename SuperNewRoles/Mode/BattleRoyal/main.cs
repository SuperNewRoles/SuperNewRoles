using HarmonyLib;
using Hazel;
using SuperNewRoles.EndGame;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class main
    {
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
        class CoEnterVentPatch
        {
            public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (ModeHandler.isMode(ModeId.BattleRoyal))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                        writer.WritePacked(id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    } else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        SuperHostRoles.CoEnterVent.Prefix(__instance,id);
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
        class RepairSystemPatch
        {
            public static bool Prefix(ShipStatus __instance,
                [HarmonyArgument(0)] SystemTypes systemType,
                [HarmonyArgument(1)] PlayerControl player,
                [HarmonyArgument(2)] byte amount)
            {
                if (!AmongUsClient.Instance.AmHost) return true;
                if ((ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.HideAndSeek)) && (systemType == SystemTypes.Sabotage ||systemType == SystemTypes.Doors)) return false;
                if (ModeHandler.isMode(ModeId.SuperHostRoles)) return MorePatch.RepairSystem(__instance, systemType, player, amount);
                return true;
            }
        }
        public static bool EndGameCheck(ShipStatus __instance, PlayerStatistics statistics)
        {
            var alives = 0;
            HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p.isAlive()) {
                    alives++;
                }
            }
            if (alives == 1)
            {
                __instance.enabled = false;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (p.isAlive())
                    {
                        p.RpcSetRole(RoleTypes.Impostor);
                    }
                    else {
                        p.RpcSetRole(RoleTypes.GuardianAngel);
                    }
                }
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            else if(alives == 0){
                __instance.enabled = false;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    p.RpcSetRole(RoleTypes.GuardianAngel);
                }
                ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
        class ChangeRole
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance.AmHost && ModeHandler.isMode(ModeId.BattleRoyal))
                {
                    foreach (PlayerControl p1 in PlayerControl.AllPlayerControls)
                    {
                        if (p1.PlayerId == 0)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (p1.PlayerId == p2.PlayerId)
                                {
                                    DestroyableSingleton<RoleManager>.Instance.SetRole(p2, RoleTypes.Impostor);

                                }
                                else
                                {
                                    DestroyableSingleton<RoleManager>.Instance.SetRole(p2, RoleTypes.Scientist);
                                }
                            }
                        }
                        else
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (p1.PlayerId == p2.PlayerId)
                                {
                                    p1.SetPrivateRole(RoleTypes.Impostor, p1);
                                }
                                else
                                {
                                    p1.SetPrivateRole(RoleTypes.Scientist, p2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
