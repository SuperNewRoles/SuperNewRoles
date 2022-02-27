using HarmonyLib;
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
            public static void Prefix(ExileController __instance)
            {
                try
                {
                    endm();
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                }
            }
        }
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        public class CheckAirShipEndGamePatch
        {
            public static void Prefix(AirshipExileController __instance)
            {
                try
                {
                    endm();
                }
                catch (Exception e)
                {
                }
            }
        }
        public static void endm() {
            //HudManager.Instance.KillButton.SetCoolDown(EvilGamblerMurder.temp, EvilGamblerMurder.temp);
            //PlayerControl.LocalPlayer.SetKillTimer(EvilGamblerMurder.temp);
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public class EvilGamblerMurder
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (__instance == PlayerControl.LocalPlayer && RoleClass.EvilGambler.EvilGamblerPlayer.IsCheckListPlayerControl(__instance)) {
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
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (PlayerControl.GameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (PlayerControl.LocalPlayer.getRole() == CustomRPC.RoleId.EvilGambler) addition = RoleClass.EvilGambler.NotSucCool;

            float max = Mathf.Max(PlayerControl.GameOptions.KillCooldown * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, max), max);
            return false;
        }

        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;

            player.killTimer = time;
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
    }
}
