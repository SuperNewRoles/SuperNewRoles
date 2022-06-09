﻿using HarmonyLib;
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
                if (!RoleClass.Minimalist.UseVent)
                {
                    HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
                }
                if (!RoleClass.Minimalist.UseSabo)
                {
                    HudManager.Instance.SabotageButton.gameObject.SetActive(false);
                }
                if (!RoleClass.Minimalist.UseReport)
                {
                    HudManager.Instance.ReportButton.SetActive(false);
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
