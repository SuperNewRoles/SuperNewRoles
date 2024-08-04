using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;
using static SuperNewRoles.Mode.PlusMode.PlusGameOptions;

namespace SuperNewRoles.Patches;

public class Clairvoyant
{
    private static float count;
    //千里眼機能の中身
    public class FixedUpdate
    {
        public static void Postfix()
        {
            SuperNewRolesPlugin.Logger.LogInfo(count);
            SuperNewRolesPlugin.Logger.LogInfo(Timer);
            if (Timer >= 0.1 && !RoleClass.IsMeeting)
            {
                Camera.main.orthographicSize = MapOption.MapOption.CameraDefault * 3f;
                FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = MapOption.MapOption.Default * 3f;
                if (count == 0)
                {
                    count = 1;
                    Timer = 0;
                    return;
                }
            }
            else
            {
                Camera.main.orthographicSize = MapOption.MapOption.CameraDefault;
                FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = MapOption.MapOption.Default;
            }
        }
    }

    private static CustomButton ClairvoyantButton;

    public static void SetupCustomButtons(HudManager __instance)
    {
        ClairvoyantButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.CanMove)
                {
                    Timer = DurationTime;
                    ButtonTimer = DateTime.Now;
                    ClairvoyantButton.MaxTimer = CoolTime;
                    ClairvoyantButton.Timer = CoolTime;
                    IsZoomOn = true;
                }
            },
            (bool isAlive, RoleId role) => { return !PlayerControl.LocalPlayer.IsAlive() && IsClairvoyantZoom && ModeHandler.IsMode(ModeId.Default); },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                ClairvoyantButton.MaxTimer = CoolTime;
                ClairvoyantButton.Timer = CoolTime;
                IsZoomOn = false;
            },
            Roles.RoleClass.Hawk.GetButtonSprite(),
            new Vector3(-2.925f, -0.06f, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ClairvoyantButtonName"),
            showButtonText = true
        };
    }

    private static TimeSpan? ClairvoyantDurationSpanCached;
    private static float lastDurationTime;

    public static void ClairvoyantDuration()
    {
        if (Timer == 0 && PlayerControl.LocalPlayer.Data.IsDead && IsClairvoyantZoom) return;
        IsZoomOn = true;
        if (!ClairvoyantDurationSpanCached.HasValue || lastDurationTime != DurationTime)
        {
            ClairvoyantDurationSpanCached = new(0, 0, 0, (int)DurationTime);
            lastDurationTime = DurationTime;
        }
        Timer = (float)(ButtonTimer + ClairvoyantDurationSpanCached.Value - DateTime.Now).TotalSeconds;
        if (Timer <= 0f)
            Timer = 0f;
        IsZoomOn = false;

    }
}