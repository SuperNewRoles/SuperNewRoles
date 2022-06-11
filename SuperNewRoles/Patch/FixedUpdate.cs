using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.Sabotage;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
    public class StartGame
    {
        public static void Postfix(PlayerControl __instance)
        {
            FixedUpdate.IsProDown = ConfigRoles.CustomProcessDown.Value;
        }
    }
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
    public class AbilityUpdate {
        public static void Postfix(AbilityButton __instance)
        {
            if (CachedPlayer.LocalPlayer.Data.Role.IsSimpleRole && __instance.commsDown.active)
            {
                __instance.commsDown.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class DebugManager
    {
        public static void Postfix(ControllerManager __instance)
        {
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                if (AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift))
                {
                    RPCHelper.StartRPC(CustomRPC.CustomRPC.SetHaison).EndRPC();
                    RPCProcedure.SetHaison();
                    MapUtilities.CachedShipStatus.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class FixedUpdate
    {
        static void setBasePlayerOutlines()
        {
            foreach (PlayerControl target in CachedPlayer.AllPlayers)
            {
                if (target == null || target.MyRend == null) continue;
                target.MyRend.material.SetFloat("_Outline", 0f);
            }
        }

        private static bool ProDown = false;
        public static bool IsProDown;

        public static void Postfix(PlayerControl __instance)
        {
            if (__instance == PlayerControl.LocalPlayer)
            {
                if (IsProDown)
                {
                    ProDown = !ProDown;
                    if (ProDown)
                    {
                        return;
                    }
                }
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                {
                    setBasePlayerOutlines();
                    VentAndSabo.VentButtonVisibilityPatch.Postfix(__instance);
                    SerialKiller.FixedUpdate();
                    if (ModeHandler.isMode(ModeId.NotImpostorCheck))
                    {
                        Mode.NotImpostorCheck.NameSet.Postfix();
                    }
                    else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        Mode.SuperHostRoles.FixedUpdate.Update();
                        Fox.FixedUpdate.Postfix();
                    }
                    else if (ModeHandler.isMode(ModeId.Default))
                    {
                        SabotageManager.Update();
                        SetNameUpdate.Postfix(__instance);
                        Jackal.JackalFixedPatch.Postfix(__instance);
                        JackalSeer.JackalSeerFixedPatch.Postfix(__instance);
                        if (PlayerControl.LocalPlayer.isAlive())
                        {
                            if (PlayerControl.LocalPlayer.isImpostor()) { SetTarget.ImpostorSetTarget(); }
                            var MyRole = PlayerControl.LocalPlayer.getRole();
                            switch (MyRole)
                            {
                                case RoleId.Researcher:
                                    Researcher.ReseUseButtonSetTargetPatch.Postfix(PlayerControl.LocalPlayer);
                                    break;
                                case RoleId.Pursuer:
                                    Pursuer.PursureUpdate.Postfix();
                                    break;
                                case RoleId.Levelinger:
                                    if (RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Pursuer))
                                    {
                                        if (!RoleClass.Pursuer.arrow.arrow.active)
                                        {
                                            RoleClass.Pursuer.arrow.arrow.SetActive(true);
                                        }
                                        Pursuer.PursureUpdate.Postfix();

                                    }
                                    else
                                    {
                                        if (RoleClass.Pursuer.arrow.arrow.active)
                                        {
                                            RoleClass.Pursuer.arrow.arrow.SetActive(false);
                                        }
                                    }
                                    break;
                                case RoleId.Hawk:
                                    Hawk.FixedUpdate.Postfix();
                                    break;
                                case RoleId.NiceHawk:
                                    NiceHawk.FixedUpdate.Postfix();
                                    break;
                                case RoleId.MadHawk:
                                    MadHawk.FixedUpdate.Postfix();
                                    break;
                                case RoleId.Vampire:
                                    Vampire.FixedUpdate.Postfix();
                                    break;
                                case RoleId.DarkKiller:
                                    DarkKiller.FixedUpdate.Postfix();
                                    break;
                                case RoleId.Vulture:
                                    Vulture.FixedUpdate.Postfix();
                                    break;
                            }
                            Fox.FixedUpdate.Postfix();
                            Minimalist.FixedUpdate.Postfix();
                        }
                        else
                        {
                            if (PlayerControl.LocalPlayer.isRole(RoleId.Bait))
                            {
                                if (!RoleClass.Bait.Reported)
                                {
                                    Bait.BaitUpdate.Postfix(__instance);

                                }
                            }
                            else if (PlayerControl.LocalPlayer.isRole(RoleId.SideKiller))
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
                        }
                    }
                    else
                    {
                        ModeHandler.FixedUpdate(__instance);
                    }
                }
                else if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined)
                {

                }
            }
        }
    }
}
