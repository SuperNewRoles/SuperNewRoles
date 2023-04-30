using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Buttons;

public static class OldModeButtons
{
    public static bool IsOldMode => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && CustomOptionHolder.IsOldMode.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles);
    public static bool CanUseKeyboard => IsOldMode && false;
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    static class MeetingHudStart
    {
        public static void Postfix(MeetingHud __instance)
        {
            MeetingHudUpdate.IsEnd = false;
            MeetingHudUpdate.time = 6.6f;
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    static class MeetingHudUpdate
    {
        public static bool IsEnd = false;
        public static float time;
        public static void Postfix(MeetingHud __instance)
        {
            if (!IsOldMode) return;
            time -= Time.deltaTime;
            if (time <= 0)
            {
                if (!IsEnd)
                {/*
                        Vector3 pos = __instance.transform.FindChild("OverlayParent/PlayerVoteArea(Clone)").localPosition;
                        pos.z = 10;
                        __instance.transform.FindChild("OverlayParent/PlayerVoteArea(Clone)").localPosition = pos;*/
                    FastDestroyableSingleton<HudManager>.Instance.discussEmblem.gameObject.SetActive(true);
                    time = 3f;
                    IsEnd = true;
                    return;
                }
                else
                {
                    FastDestroyableSingleton<HudManager>.Instance.discussEmblem.gameObject.SetActive(false);
                    time = 99999999;
                }
                return;
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new System.Type[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    class HudManagerSetHudActivePatch
    {
        public static void Postfix() => OldModeUpdate();
    }
    [HarmonyPatch(typeof(ConsoleJoystick), nameof(ConsoleJoystick.HandleHUD))]
    class ConsoleJoystickHandleHUDPatch
    {
        public static bool Prefix() => !CanUseKeyboard;
    }
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.HandleHud))]
    class KeyboardJoystickHandleHUDPatch
    {
        public static bool Prefix()
        {
            if (!CanUseKeyboard) return true;
            if (!DestroyableSingleton<HudManager>.InstanceExists)
            {
                return false;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
                {
                    Mode = MapOptions.Modes.Sabotage,
                    AllowMovementWhileMapOpen = true
                });
            }
            return false;
        }
    }
    public static void OldModeUpdate()
    {
        if (!IsOldMode) return;
        HudManager Hud = FastDestroyableSingleton<HudManager>.Instance;
        //キルボタン
        Hud.KillButton.transform.localPosition = new(3, -2.1f, 1);
        Hud.KillButton.transform.localScale = new(1.2f, 1.2f, 1.2f);
        Hud.KillButton.buttonLabelText.transform.localPosition = new(0, 0.2f, 0);
        Hud.KillButton.buttonLabelText.transform.localScale = new(1.7f, 1.7f, 1.7f);
        //サボボタン
        Hud.SabotageButton.transform.localPosition = new(4.45f, -2.1f, -9);
        Hud.SabotageButton.transform.localScale = new(1.2f, 1.2f, 1.2f);
        Hud.SabotageButton.buttonLabelText.transform.localPosition = new(0, -0.35f, 0);
        Hud.SabotageButton.buttonLabelText.transform.localScale = new(0.9f, 0.9f, 0.9f);
        //使用ボタン
        Hud.UseButton.transform.localPosition = new(4.45f, -2.1f, -9);
        Hud.UseButton.transform.localScale = new(1.2f, 1.2f, 1.2f);
        if (Hud.UseButton.graphic.sprite == Hud.UseButton.fastUseSettings[ImageNames.UseButton].Image)
        {
            Hud.UseButton.buttonLabelText.transform.localPosition = new();
            Hud.UseButton.buttonLabelText.transform.localScale = new(1.7f, 1.7f, 1.7f);
        }
        else
        {
            Hud.UseButton.buttonLabelText.transform.localPosition = new(0, -0.45f, 0);
            Hud.UseButton.buttonLabelText.transform.localScale = new(1.1f, 1.1f, 1.1f);
        }
        Hud.AdminButton.gameObject.SetActive(false);
        //ベント
        Hud.ImpostorVentButton.transform.localPosition = new(4.45f, -2.1f, -9);
        Hud.ImpostorVentButton.transform.localScale = new(1.3f, 1.3f, 1.3f);
        Hud.ImpostorVentButton.buttonLabelText.transform.localPosition = new(-0.05f, -0.4f, 0);
        Hud.ImpostorVentButton.buttonLabelText.transform.localScale = new(1f, 1f, 1f);
        //通報
        Hud.ReportButton.transform.localPosition = new(4.425f, -0.5f, -9);
        Hud.ReportButton.transform.localScale = new(1.2f, 1.2f, 1.2f);

        if (Hud.AbilityButton != null)
        {
            Hud.AbilityButton.transform.localPosition = new(3f, Hud.KillButton.gameObject.active ? -0.5f : -2.1f, -9);
            Hud.AbilityButton.transform.localScale = new(1.2f, 1.2f, 1.2f);
            Hud.AbilityButton.buttonLabelText.transform.localPosition = new(-0.01f, -0.325f, 0);
        }

        if (Hud.UseButton.currentTarget == null || (Hud.ImpostorVentButton.gameObject.active && Hud.ImpostorVentButton.currentTarget != null))
        {
            bool IsViewUseButton = true;
            if (Hud.ImpostorVentButton.gameObject.active && Hud.ImpostorVentButton.currentTarget != null)
            {
                Hud.SabotageButton.transform.localScale = new();
                IsViewUseButton = false;
            }
            else if (Hud.SabotageButton.gameObject.active && PlayerControl.LocalPlayer.CanMove)
            {
                Hud.ImpostorVentButton.transform.localScale = new();
                IsViewUseButton = false;
            }
            if (!IsViewUseButton)
            {
                Hud.UseButton.transform.localScale = new();
            }
            else
            {
                Hud.UseButton.transform.localScale = new(1.2f, 1.2f, 1.2f);
                Hud.SabotageButton.transform.localScale = new();
                Hud.ImpostorVentButton.transform.localScale = new();
            }
        }
        else
        {
            Hud.UseButton.transform.localScale = new(1.2f, 1.2f, 1.2f);
            Hud.SabotageButton.transform.localScale = new();
            Hud.ImpostorVentButton.transform.localScale = new();
        }
    }
}