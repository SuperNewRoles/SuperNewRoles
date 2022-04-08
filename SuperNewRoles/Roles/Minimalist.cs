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
                    HudManager.Instance.ImpostorVentButton.gameObject.SetActiveRecursively(false);
                    HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
                    HudManager.Instance.ImpostorVentButton.graphic.enabled = false;
                    HudManager.Instance.ImpostorVentButton.enabled = false;
                    HudManager.Instance.ImpostorVentButton.graphic.sprite = null;
                    HudManager.Instance.ImpostorVentButton.buttonLabelText.enabled = false;
                    HudManager.Instance.ImpostorVentButton.buttonLabelText.SetText("");
                }
                if (!RoleClass.Minimalist.UseSabo)
                {
                    HudManager.Instance.SabotageButton.gameObject.SetActiveRecursively(false);
                    HudManager.Instance.SabotageButton.gameObject.SetActive(false);
                    HudManager.Instance.SabotageButton.graphic.enabled = false;
                    HudManager.Instance.SabotageButton.enabled = false;
                    HudManager.Instance.SabotageButton.graphic.sprite = null;
                    HudManager.Instance.SabotageButton.buttonLabelText.enabled = false;
                    HudManager.Instance.SabotageButton.buttonLabelText.SetText("");
                }
                if (!RoleClass.Minimalist.UseReport)
                {

                    HudManager.Instance.ReportButton.gameObject.SetActiveRecursively(false);
                    HudManager.Instance.ReportButton.SetActive(false);
                    HudManager.Instance.ReportButton.graphic.enabled = false;
                    HudManager.Instance.ReportButton.enabled = false;
                    HudManager.Instance.ReportButton.graphic.sprite = null;
                    HudManager.Instance.ReportButton.buttonLabelText.enabled = false;
                    HudManager.Instance.ReportButton.buttonLabelText.SetText("");
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
    }
}
