using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class Fox
    {
        public static void SetFoxButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Fox))
            {
                if (!RoleClass.Fox.UseReport)
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
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend.material.SetFloat("_Outline", 1f);
            target.MyRend.material.SetColor("_OutlineColor", color);
        }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetFoxButton();
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class FoxDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Fox))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
                }
            }
        }
        public class FoxFixedPatch
        {


        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class FoxMurderPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (AmongUsClient.Instance.AmHost && __instance.PlayerId != target.PlayerId)
                {
                    if (target.isRole(CustomRPC.RoleId.Fox))
                    {
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                        {
                            if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId))
                            {
                                target.RpcProtectPlayer(target, 0);
                            }
                            else
                            {
                                if (!(RoleClass.Fox.KillGuard[target.PlayerId] <= 0))
                                {
                                    RoleClass.Fox.KillGuard[target.PlayerId]--;
                                    target.RpcProtectPlayer(target, 0);
                                }
                            }
                        }
                    }
                }
            }
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
            }
        }

    }
}