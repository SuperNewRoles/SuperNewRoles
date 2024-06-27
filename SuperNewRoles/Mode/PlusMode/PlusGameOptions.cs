using System;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Mode.PlusMode;

class PlusGameOptions
{
    public static CustomOption PlusGameOptionSetting;

    /// <summary>
    /// 設定 : "死亡時に他プレイヤーの役職を表示しない"
    /// </summary>
    /// <value>true : 表示しない / false : 表示する</value>
    public static CustomOption CanNotGhostSeeRole;

    /// <summary>
    /// 設定 : "インポスターは死亡時に他プレイヤーの役職を表示する
    /// (CanNotGhostSeeRole が有効時 インポスターは他プレイヤーの役職を表示するか)
    /// </summary>
    /// <value>true : 表示する / false : 表示しない</value>
    public static CustomOption OnlyImpostorGhostSeeRole;

    public static CustomOption CanGhostSeeVote;

    public static CustomOption CanNotGhostHaveHaunt;
    public static CustomOption ReleaseHauntAfterCompleteTasks;

    public static CustomOption LadderDead;
    public static CustomOption LadderDeadChance;

    public static CustomOption ReportDeadBodySetting;
    public static CustomOption HaveFirstEmergencyCooldownSetting;
    public static CustomOption FirstEmergencyCooldownSetting;
    public static CustomOption IsLimitEmergencyMeeting;
    public static CustomOption EmergencyMeetingLimitCount;
    public static CustomOption NotUseReportDeadBody;

    public static CustomOption NoTaskWinModeSetting;

    public static CustomOption ZoomOption;
    public static CustomOption ClairvoyantZoom;
    public static CustomOption MouseZoom;
    public static CustomOption ZoomCoolTime;
    public static CustomOption ZoomDurationTime;

    public static CustomOption NoSabotageModeSetting;

    public static void Load()
    {
        PlusGameOptionSetting = Create(103500, true, CustomOptionType.Generic, Cs(new Color(168f / 187f, 191f / 255f, 147f / 255f, 1f), "PlusGameOptionSetting"), false, null, isHeader: true, withHeader: true);

        CanNotGhostSeeRole = Create(103600, true, CustomOptionType.Generic, "CanNotGhostSeeRole", false, PlusGameOptionSetting, isHeader: true);
        OnlyImpostorGhostSeeRole = Create(103601, true, CustomOptionType.Generic, "OnlyImpostorGhostSeeRole", false, CanNotGhostSeeRole);

        CanGhostSeeVote = Create(103700, true, CustomOptionType.Generic, "CanGhostSeeVote", true, PlusGameOptionSetting, isHeader: true);

        CanNotGhostHaveHaunt = Create(104700, true, CustomOptionType.Generic, "CanNotGhostHaveHaunt", false, PlusGameOptionSetting, isHeader: true);
        ReleaseHauntAfterCompleteTasks = Create(104701, true, CustomOptionType.Generic, "ReleaseHauntAfterCompleteTasks", false, CanNotGhostHaveHaunt);

        LadderDead = Create(103900, true, CustomOptionType.Generic, "LadderDead", false, PlusGameOptionSetting, isHeader: true);
        LadderDeadChance = Create(103901, true, CustomOptionType.Generic, "LadderDeadChance", rates[1..], LadderDead);

        NoTaskWinModeSetting = Create(104000, true, CustomOptionType.Generic, "SettingNoTaskWinMode", false, PlusGameOptionSetting, isHeader: true);

        ReportDeadBodySetting = Create(105300, true, CustomOptionType.Generic, "ReportDeadBodySetting", false, PlusGameOptionSetting, isHeader: true);
        HaveFirstEmergencyCooldownSetting = Create(105304, true, CustomOptionType.Generic, "HaveFirstEmergencyCooldown", false, ReportDeadBodySetting);
        FirstEmergencyCooldownSetting = Create(105305, true, CustomOptionType.Generic, "FirstEmergencyCooldownSetting", 30, 0, 120, 5, HaveFirstEmergencyCooldownSetting);
        IsLimitEmergencyMeeting = Create(105301, true, CustomOptionType.Generic, "IsLimitEmergencyMeeting", false, ReportDeadBodySetting);
        EmergencyMeetingLimitCount = Create(105302, true, CustomOptionType.Generic, "EmergencyMeetingLimitCount", 10, 0, 20, 1, IsLimitEmergencyMeeting);
        NotUseReportDeadBody = Create(105303, true, CustomOptionType.Generic, "NotUseReportSetting", false, ReportDeadBodySetting);

        ZoomOption = Create(104200, false, CustomOptionType.Generic, Cs(Color.white, "Zoomafterdeath"), true, PlusGameOptionSetting, isHeader: true);
        MouseZoom = Create(104201, false, CustomOptionType.Generic, "mousemode", false, ZoomOption);
        ClairvoyantZoom = Create(104202, false, CustomOptionType.Generic, "clairvoyantmode", false, ZoomOption);
        ZoomCoolTime = Create(104203, false, CustomOptionType.Generic, "clairvoyantCoolTime", 15f, 0f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");
        ZoomDurationTime = Create(104204, false, CustomOptionType.Generic, "clairvoyantDurationTime", 5f, 0f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");

        NoSabotageModeSetting = Create(104300, true, CustomOptionType.Generic, "SettingNoSabotageMode", false, PlusGameOptionSetting, isHeader: true);
    }

    public static bool UseDeadBodyReport;

    public static bool IsGhostSeeVote;
    public static bool IsNotGhostHaveHaunt;
    public static bool IsReleasingHauntAfterCompleteTasks;

    // 会議関連
    /// <summary>会議回数制限</summary>
    /// <param name="enabledSetting">緊急招集を使用可能か</param>
    /// <param name="maxCount">最大緊急招集可能回数</param>
    public static (bool enabledSetting, byte maxCount) EmergencyMeetingsCallstate { get; private set; }

    /// <summary> 設定 : "死者が出るまで緊急ボタンクールダウンを変更する" が有効か</summary>
    public static bool EnableFirstEmergencyCooldown =>
        PlusGameOptionSetting.GetBool() &&
        ReportDeadBodySetting.GetBool() && HaveFirstEmergencyCooldownSetting.GetBool();

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
            if (ReportDeadBodySetting.GetBool())
            {
                bool enabledSetting = !(IsLimitEmergencyMeeting.GetBool() && EmergencyMeetingLimitCount.GetInt() == 0);
                byte maxCount = !enabledSetting ? byte.MinValue : !IsLimitEmergencyMeeting.GetBool() ? byte.MaxValue : (byte)EmergencyMeetingLimitCount.GetInt();

                EmergencyMeetingsCallstate = (enabledSetting, maxCount);
                UseDeadBodyReport = !NotUseReportDeadBody.GetBool();
            }
            else
            {
                EmergencyMeetingsCallstate = (true, byte.MaxValue);
                UseDeadBodyReport = true;
            }
            IsGhostSeeVote = CanGhostSeeVote.GetBool();

            IsNotGhostHaveHaunt = CanNotGhostHaveHaunt.GetBool();
            IsReleasingHauntAfterCompleteTasks = IsNotGhostHaveHaunt && ReleaseHauntAfterCompleteTasks.GetBool();

            //千里眼・ズーム関連
            IsClairvoyantZoom = ZoomOption.GetBool() && ClairvoyantZoom.GetBool();
            IsMouseZoom = ZoomOption.GetBool() && MouseZoom.GetBool();
        }
        else
        {
            EmergencyMeetingsCallstate = (true, byte.MaxValue);
            UseDeadBodyReport = true;

            IsGhostSeeVote = true;

            IsNotGhostHaveHaunt = false;
            IsReleasingHauntAfterCompleteTasks = false;

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