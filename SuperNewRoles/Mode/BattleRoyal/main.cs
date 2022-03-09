using BepInEx.IL2CPP.Utils;
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
                    AmongUsClient.Instance.StartCoroutine(VentGuard());
                    IEnumerator VentGuard()
                    {
                        yield return null;
                        int clientId = __instance.myPlayer.getClientId();

                            SuperNewRolesPlugin.Logger.LogInfo("UPDATE!");
                            foreach (Vent vent in ShipStatus.Instance.AllVents)
                            {
                                if (vent.Id != id)
                                {
                                    MessageWriter val2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, (SendOption)1, clientId);
                                    val2.Write(vent.Id);
                                    AmongUsClient.Instance.FinishRpcImmediately(val2);
                                }
                            }
                    }
                    if (ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.Zombie))
                    {
                        MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, (SendOption)1);
                        val.WritePacked(127);
                        AmongUsClient.Instance.FinishRpcImmediately(val);
                        AmongUsClient.Instance.StartCoroutine(Vent());
                        IEnumerator Vent() { 
                            yield return new WaitForSeconds(0.5f);
                            int clientId = __instance.myPlayer.getClientId();
                            MessageWriter val2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, (SendOption)1, clientId);
                            val2.Write(id);
                            AmongUsClient.Instance.FinishRpcImmediately(val2);
                        }
                        return false;
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
                if ((ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.Zombie) || ModeHandler.isMode(ModeId.HideAndSeek)) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors)) return false;
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
                ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        public static void ClearAndReload()
        {
            PlayerControl.GameOptions.NumImpostors = 15;
            PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
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
