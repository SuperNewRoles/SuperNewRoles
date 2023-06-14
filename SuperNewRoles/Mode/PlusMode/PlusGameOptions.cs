using System;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Mode.PlusMode;

class PlusGameOptions
{
    public static CustomOption PlusGameOptionSetting;

    public static CustomOption CanNotGhostSeeRole;
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
        PlusGameOptionSetting = Create(103500, true, CustomOptionType.Generic, Cs(new Color(168f / 187f, 191f / 255f, 147f / 255f, 1f), "PlusGameOptionSetting"), false, null, isHeader: true);

        CanNotGhostSeeRole = Create(103600, true, CustomOptionType.Generic, "CanNotGhostSeeRole", false, PlusGameOptionSetting, isHeader: true);
        OnlyImpostorGhostSeeRole = Create(103601, true, CustomOptionType.Generic, "OnlyImpostorGhostSeeRole", false, CanNotGhostSeeRole);

        CanGhostSeeVote = Create(103700, true, CustomOptionType.Generic, "CanGhostSeeVote", true, PlusGameOptionSetting, isHeader: true);

        IsAlwaysReduceCooldown = Create(103800, false, CustomOptionType.Generic, "IsAlwaysReduceCooldown", false, PlusGameOptionSetting, isHeader: true);
        IsAlwaysReduceCooldownExceptInVent = Create(103801, false, CustomOptionType.Generic, "IsAlwaysReduceCooldownExceptInVent", false, IsAlwaysReduceCooldown);
        IsAlwaysReduceCooldownExceptOnTask = Create(103802, false, CustomOptionType.Generic, "IsAlwaysReduceCooldownExceptOnTask", true, IsAlwaysReduceCooldown);

        LadderDead = Create(103900, true, CustomOptionType.Generic, "LadderDead", false, PlusGameOptionSetting, isHeader: true);
        LadderDeadChance = Create(103901, true, CustomOptionType.Generic, "LadderDeadChance", rates[1..], LadderDead);

        NoTaskWinModeSetting = Create(104000, true, CustomOptionType.Generic, "SettingNoTaskWinMode", false, PlusGameOptionSetting, isHeader: true);

        IsChangeTheWinCondition = Create(104100, true, CustomOptionType.Generic, "IsChangeTheWinCondition", false, PlusGameOptionSetting, isHeader: true);

        ZoomOption = Create(104200, false, CustomOptionType.Generic, Cs(Color.white, "Zoomafterdeath"), true, PlusGameOptionSetting, isHeader: true);
        MouseZoom = Create(104201, false, CustomOptionType.Generic, "mousemode", false, ZoomOption);
        ClairvoyantZoom = Create(104202, false, CustomOptionType.Generic, "clairvoyantmode", false, ZoomOption);
        ZoomCoolTime = Create(104203, false, CustomOptionType.Generic, "clairvoyantCoolTime", 15f, 0f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");
        ZoomDurationTime = Create(104204, false, CustomOptionType.Generic, "clairvoyantDurationTime", 5f, 0f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");

        NoSabotageModeSetting = Create(104300, true, CustomOptionType.Generic, "SettingNoSabotageMode", false, PlusGameOptionSetting, isHeader: true);
        NotUseReportDeadBody = Create(104301, true, CustomOptionType.Generic, "NotUseReportSetting", false, PlusGameOptionSetting);
        NotUseMeetingButton = Create(104302, true, CustomOptionType.Generic, "NotUseMeetingSetting", false, PlusGameOptionSetting);
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