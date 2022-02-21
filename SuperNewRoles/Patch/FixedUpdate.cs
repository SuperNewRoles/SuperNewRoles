using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
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
            __instance.commsDown.SetActive(false);
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class FixedUpdate
    {
        static void setBasePlayerOutlines()
        {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.myRend == null) continue;
                target.myRend.material.SetFloat("_Outline", 0f);
            }
        }

        static bool ProDown = false;
        public static bool IsProDown;

        public static void Postfix(PlayerControl __instance)
        {
            if (__instance == PlayerControl.LocalPlayer)
            {
                ProDown = !ProDown;
                if (IsProDown && ProDown) return;
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                {
                    setBasePlayerOutlines();
                    VentAndSabo.VentButtonVisibilityPatch.Postfix(__instance);
                    if (AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift)) {
                        ShipStatus.Instance.enabled = false;
                        ShipStatus.RpcEndGame((GameOverReason)EndGame.CustomGameOverReason.HAISON,false);
                    }
                    if (ModeHandler.isMode(ModeId.Default))
                    {
                        SetNameUpdate.Postfix(__instance);
                        Jackal.JackalFixedPatch.Postfix(__instance);
                        if (PlayerControl.LocalPlayer.isAlive())
                        {
                            if (RoleClass.Bait.BaitPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                            {
                                if (!RoleClass.Bait.Reported && !PlayerControl.LocalPlayer.isAlive())
                                {
                                    Bait.BaitUpdate.Postfix(__instance);

                                }
                            }
                            if (RoleClass.Researcher.ResearcherPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                            {
                                Researcher.ReseUseButtonSetTargetPatch.Postfix(__instance);
                            }
                        }
                        else
                        {

                        }
                    }
                    else {
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
