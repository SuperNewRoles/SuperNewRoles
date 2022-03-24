
using HarmonyLib;
using Hazel;
using SuperNewRoles.EndGame;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections;
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
                    /*
                    
                    */
                    if (ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.Zombie))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                        writer.WritePacked(127);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        new LateTask(() => {
                            int clientId = __instance.myPlayer.getClientId();
                            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                            writer2.Write(id);
                            AmongUsClient.Instance.FinishRpcImmediately(writer2);
                            __instance.myPlayer.inVent = false;
                        }, 0.5f, "Anti Vent");
                        return false;
                    } else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        return CoEnterVent.Prefix(__instance,id);
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
                if (PlusModeHandler.isMode(PlusModeId.NotSabotage))
                {
                    return false;
                }
                if ((ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.Zombie) || ModeHandler.isMode(ModeId.HideAndSeek)) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors)) return false;
                if (systemType == SystemTypes.Electrical && 0 <= amount && amount <= 4 && player.isRole(CustomRPC.RoleId.MadMate))
                {
                    return false;
                }
                if (ModeHandler.isMode(ModeId.SuperHostRoles) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors))
                {
                    bool returndata = MorePatch.RepairSystem(__instance, systemType, player, amount);
                    SuperNewRolesPlugin.Logger.LogInfo(returndata);
                    return returndata;
                }
                SuperNewRolesPlugin.Logger.LogInfo("OK");
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
                ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        public static void ClearAndReload()
        {
        }            
        class ChangeRole
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance.AmHost && ModeHandler.isMode(ModeId.BattleRoyal))
                {
                    foreach (PlayerControl p1 in PlayerControl.AllPlayerControls)
                    {
                        DestroyableSingleton<RoleManager>.Instance.SetRole(p1, RoleTypes.Crewmate);
                        p1.SetPrivateRole(RoleTypes.Impostor);
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            p1.SetPrivateRole(RoleTypes.Scientist,p2);
                            p2.SetPrivateRole(RoleTypes.Scientist, p1);
                        }
                    }
                    DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                }
            }
        }
    }
}
