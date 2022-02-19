using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

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
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class FixedUpdate
    {
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
                    VentAndSabo.VentButtonVisibilityPatch.Postfix(__instance);
                    if (ModeHandler.isMode(ModeId.Default))
                    {
                        SetNameUpdate.Postfix(__instance);
                        Jackal.JackalFixedPatch.Postfix(__instance);
                        if (PlayerControl.LocalPlayer.isAlive())
                        {
                            if (RoleClass.Bait.BaitPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                            {
                                if (!RoleClass.Bait.Reported)
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
