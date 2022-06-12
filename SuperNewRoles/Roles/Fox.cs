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
            if (PlayerControl.LocalPlayer.isRole(RoleId.Fox))
            {
                if (!RoleClass.Fox.UseReport)
                {
                    if (FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.active) {
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                    }
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

        public static class FoxMurderPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
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
}