using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class EvilGambler
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public class CheckEndGamePatch
        {
            public static void Postfix(ExileController __instance)
            {
                try
                {
                    endm();
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("EvilGambler:" + e);
                }
            }
        }
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        public class CheckAirShipEndGamePatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                try
                {
                    endm();
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("EvilGambler:" + e);
                }
            }
        }
        public static void endm() {
            //HudManager.Instance.KillButton.SetCoolDown(EvilGamblerMurder.temp, EvilGamblerMurder.temp);
            //PlayerControl.LocalPlayer.SetKillTimer(EvilGamblerMurder.temp);
        }

        public static class EvilGamblerMurder
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (__instance.isRole(CustomRPC.RoleId.EvilGambler))
                    {
                        SyncSetting.GamblersetCool(__instance);
                    }
                    return;
                }
                else if (__instance == PlayerControl.LocalPlayer && RoleClass.EvilGambler.EvilGamblerPlayer.IsCheckListPlayerControl(__instance)) {
                    if (RoleClass.EvilGambler.GetSuc()) {
                        //成功
                        PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.SucCool);
                    } else {
                        //失敗
                        PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.NotSucCool);
                    };
                }
            }
        }
    }
}
