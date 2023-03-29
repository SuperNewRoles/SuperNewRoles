using System;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Mode.PlusMode;

class PlusGameOptions
{
    public static CustomOption PlusGameOptionSetting;

    public static CustomOption CanGhostSeeRole;
    public static CustomOption OnlyImpostorGhostSeeRole;

    public static CustomOption CanGhostSeeVote;

    public static CustomOption IsAlwaysReduceCooldown;
    public static CustomOption IsAlwaysReduceCooldownExceptInVent;
    public static CustomOption IsAlwaysReduceCooldownExceptOnTask;

    public static CustomOption LadderDead;
    public static CustomOption LadderDeadChance;

    public static CustomOption NoTaskWinModeSetting;

    public static CustomOption IsChangeTheWinCondition;

    public static CustomOption ZoomOption;
    public static CustomOption ClairvoyantZoom;
    public static CustomOption MouseZoom;
    public static CustomOption ZoomCoolTime;
    public static CustomOption ZoomDurationTime;

    public static CustomOption NoSabotageModeSetting;
    public static CustomOption NotUseReportDeadBody;
    public static CustomOption NotUseMeetingButton;

    public static void Load()
    {
        PlusGameOptionSetting = Create(508, true, CustomOptionType.Generic, Cs(new Color(168f / 187f, 191f / 255f, 147f / 255f, 1f), "PlusGameOptionSetting"), false, null, isHeader: true);

        CanGhostSeeRole = Create(1100, true, CustomOptionType.Generic, "CanGhostSeeRole", true, PlusGameOptionSetting, isHeader: true);
        OnlyImpostorGhostSeeRole = Create(1101, true, CustomOptionType.Generic, "OnlyImpostorGhostSeeRole", false, CanGhostSeeRole);

        CanGhostSeeVote = Create(1144, true, CustomOptionType.Generic, "CanGhostSeeVote", true, PlusGameOptionSetting, isHeader: true);

        IsAlwaysReduceCooldown = Create(682, false, CustomOptionType.Generic, "IsAlwaysReduceCooldown", false, PlusGameOptionSetting, isHeader: true);
        IsAlwaysReduceCooldownExceptInVent = Create(954, false, CustomOptionType.Generic, "IsAlwaysReduceCooldownExceptInVent", false, IsAlwaysReduceCooldown);
        IsAlwaysReduceCooldownExceptOnTask = Create(684, false, CustomOptionType.Generic, "IsAlwaysReduceCooldownExceptOnTask", true, IsAlwaysReduceCooldown);

        LadderDead = Create(637, true, CustomOptionType.Generic, "LadderDead", false, PlusGameOptionSetting, isHeader: true);
        LadderDeadChance = Create(625, true, CustomOptionType.Generic, "LadderDeadChance", rates[1..], LadderDead);

        NoTaskWinModeSetting = Create(510, true, CustomOptionType.Generic, "SettingNoTaskWinMode", false, PlusGameOptionSetting, isHeader: true);

        IsChangeTheWinCondition = Create(1005, true, CustomOptionType.Generic, "IsChangeTheWinCondition", false, PlusGameOptionSetting, isHeader: true);

        ZoomOption = Create(618, false, CustomOptionType.Generic, Cs(Color.white, "Zoomafterdeath"), true, PlusGameOptionSetting, isHeader: true);
        MouseZoom = Create(619, false, CustomOptionType.Generic, "mousemode", false, ZoomOption);
        ClairvoyantZoom = Create(620, false, CustomOptionType.Generic, "clairvoyantmode", false, ZoomOption);
        ZoomCoolTime = Create(621, false, CustomOptionType.Generic, "clairvoyantCoolTime", 15f, 0f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");
        ZoomDurationTime = Create(622, false, CustomOptionType.Generic, "clairvoyantDurationTime", 5f, 0f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");

        NoSabotageModeSetting = Create(509, true, CustomOptionType.Generic, "SettingNoSabotageMode", false, PlusGameOptionSetting, isHeader: true);
        NotUseReportDeadBody = Create(452, true, CustomOptionType.Generic, "NotUseReportSetting", false, PlusGameOptionSetting);
        NotUseMeetingButton = Create(453, true, CustomOptionType.Generic, "NotUseMeetingSetting", false, PlusGameOptionSetting);
    }

    public static bool UseDeadBodyReport;
    public static bool UseMeetingButton;

    public static bool IsGhostSeeVote;

    //千里眼・ズーム関連
    public static bool IsMouseZoom;
    public static bool IsClairvoyantZoom;
    public static float CoolTime;
    public static float DurationTime;
    public static bool IsZoomOn;
    public static float Timer;
    public static DateTime ButtonTimer;

    public static void ClearAndReload()
    {
        if (PlusGameOptionSetting.GetBool())
        {
            UseDeadBodyReport = !NotUseReportDeadBody.GetBool();
            UseMeetingButton = !NotUseMeetingButton.GetBool();

            IsGhostSeeVote = CanGhostSeeVote.GetBool();

            //千里眼・ズーム関連
            IsClairvoyantZoom = ZoomOption.GetBool() && ClairvoyantZoom.GetBool();
            IsMouseZoom = ZoomOption.GetBool() && MouseZoom.GetBool();
        }
        else
        {
            UseDeadBodyReport = true;
            UseMeetingButton = true;

            IsGhostSeeVote = true;

            //千里眼・ズーム関連
            IsClairvoyantZoom = false;
            IsMouseZoom = false;
        }
        //千里眼・ズーム関連
        CoolTime = ZoomCoolTime.GetFloat();
        DurationTime = ZoomDurationTime.GetFloat();
        IsZoomOn = false;
        Timer = 0;
        ButtonTimer = DateTime.Now;
    }
}