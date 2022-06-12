using HarmonyLib;
using SuperNewRoles.Mode;
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
                if (CachedPlayer.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
                }
            }
        }
        public static void SetMinimalistButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
            {
                if (!RoleClass.Minimalist.UseVent)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                }
                if (!RoleClass.Minimalist.UseSabo)
                {
                    FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(false);
                }
                if (!RoleClass.Minimalist.UseReport)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                }
            }
        }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetMinimalistButton();
            }
        }
    }
}
