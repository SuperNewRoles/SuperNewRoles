using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    public class Minimalist
    {
        public class MurderPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (PlayerControl.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
                }
            }
        }
        public static void SetMinimalistButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
            {
                HudManager.Instance.ImpostorVentButton.gameObject.SetActive(RoleClass.Minimalist.UseVent);
                HudManager.Instance.SabotageButton.gameObject.SetActive(RoleClass.Minimalist.UseSabo);
                HudManager.Instance.ReportButton.gameObject.SetActive(RoleClass.Minimalist.UseReport);
            } else if(PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Sidekick))
            {
                HudManager.Instance.ImpostorVentButton.gameObject.SetActive(RoleClass.Minimalist.UseVent);
                HudManager.Instance.SabotageButton.gameObject.SetActive(RoleClass.Minimalist.UseSabo);
                HudManager.Instance.ReportButton.gameObject.SetActive(RoleClass.Minimalist.UseReport);
            }
        }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetMinimalistButton();
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class MinimalistDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {

                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
                }
            }
        }
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
                    SuperNewRolesPlugin.Logger.LogInfo("Minimalist:" + e);
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
                    SuperNewRolesPlugin.Logger.LogInfo("Minimalist:" + e);
                }
            }
        }
        public static void endm()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
            {
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
            }
        }
    }
}
