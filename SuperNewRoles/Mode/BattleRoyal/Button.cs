/*
using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        public static void Postfix()
        {
            if (!ModeHandler.IsMode(ModeId.BattleRoyal)) return;
            HudManagerStartPatch.resetCoolDown();
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        public static CustomButton BattleRoyalKillButton;

        public static void resetCoolDown()
        {
            BattleRoyalKillButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
            BattleRoyalKillButton.Timer = PlayerControl.GameOptions.KillCooldown;
        }

        private static PlayerControl SheriffKillTarget;

        public static void Postfix(HudManager __instance)
        {
            BattleRoyalKillButton = new(
                () =>
                {
                    if (PlayerControlFixedUpdatePatch.SetTarget() && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && ModeHandler.IsMode(ModeId.BattleRoyal) && PlayerControl.LocalPlayer.CanMove)
                    {
                        ModHelpers.CheckMuderAttemptAndKill(PlayerControl.LocalPlayer, PlayerControlFixedUpdatePatch.SetTarget());
                        resetCoolDown();
                    }
                },
                () => { return RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && ModeHandler.IsMode(ModeId.BattleRoyal); },
                () =>
                {
                    return PlayerControlFixedUpdatePatch.SetTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => {  },
                __instance.KillButton.graphic.sprite,
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q,
                8
            );

            BattleRoyalKillButton.buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text;
            BattleRoyalKillButton.showButtonText = true;
        }
    }
}*/