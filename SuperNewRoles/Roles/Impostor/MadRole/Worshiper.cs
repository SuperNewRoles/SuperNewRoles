using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Roles.RoleClass;
using SuperNewRoles.Patches;
using System;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles.Impostor.MadRole;

public static class Worshiper
{
    private const int optionId = 1143;// 設定のId

    // CustomOptionHolder
    public static CustomRoleOption WorshiperOption;
    public static CustomOption WorshiperPlayerCount;
    public static CustomOption WorshiperSuicideCoolTime;
    public static CustomOption WorshiperIsCheckImpostor;
    public static CustomOption WorshiperCommonTask;
    public static CustomOption WorshiperShortTask;
    public static CustomOption WorshiperLongTask;
    public static CustomOption WorshiperCheckImpostorTask;
    public static CustomOption WorshiperIsUseVent;
    public static CustomOption WorshiperIsImpostorLight;

    public static void SetupCustomOptions()
    {
        WorshiperOption = new(optionId, true, CustomOptionType.Crewmate, "WorshiperName", color, 1);
        WorshiperPlayerCount = CustomOption.Create(optionId + 1, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WorshiperOption);
        WorshiperSuicideCoolTime = CustomOption.Create(optionId + 2, true, CustomOptionType.Crewmate, "WorshiperSuicideCooldownSetting", 30f, 2.5f, 60f, 2.5f, WorshiperOption, format: "unitSeconds");
        WorshiperIsUseVent = CustomOption.Create(optionId + 3, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, WorshiperOption);
        WorshiperIsImpostorLight = CustomOption.Create(optionId + 4, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, WorshiperOption);
        WorshiperIsCheckImpostor = CustomOption.Create(optionId + 5, false, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, WorshiperOption);
        var Worshiperoption = SelectTask.TaskSetting(optionId + 6, optionId + 7, optionId + 8, WorshiperIsCheckImpostor, CustomOptionType.Crewmate, true);
        WorshiperCommonTask = Worshiperoption.Item1;
        WorshiperShortTask = Worshiperoption.Item2;
        WorshiperLongTask = Worshiperoption.Item3;
        WorshiperCheckImpostorTask = CustomOption.Create(optionId + 9, false, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, WorshiperIsCheckImpostor);
    }

    // RoleClass
    public static List<PlayerControl> WorshiperPlayer;
    public static Color32 color = ImpostorRed;
    public static List<byte> CheckedImpostor;
    public static bool IsUseVent;
    public static bool IsImpostorLight;
    public static bool IsImpostorCheck;
    public static int ImpostorCheckTask;
    public static float CoolTime;
    private static DateTime buttonTimer;
    public static void ClearAndReload()
    {
        WorshiperPlayer = new();

        CoolTime = WorshiperSuicideCoolTime.GetFloat();

        IsUseVent = WorshiperIsImpostorLight.GetBool();
        IsImpostorLight = WorshiperIsUseVent.GetBool();
        IsImpostorCheck = WorshiperIsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles);
        int Common = WorshiperCommonTask.GetInt();
        int Long = WorshiperLongTask.GetInt();
        int Short = WorshiperShortTask.GetInt();
        int AllTask = Common + Long + Short;
        if (AllTask == 0)
        {
            Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
            Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
            Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
        }
        ImpostorCheckTask = (int)(AllTask * (int.Parse(WorshiperCheckImpostorTask.GetString().Replace("%", "")) / 100f));
        CheckedImpostor = new();
    }

    public static void EndMeeting()
    {
        HudManagerStartPatch.WorshiperSuicideButton.MaxTimer = CoolTime;
        HudManagerStartPatch.WorshiperSuicideButton.Timer = CoolTime;
        HudManagerStartPatch.WorshiperSuicideKillButton.MaxTimer = CoolTime;
        HudManagerStartPatch.WorshiperSuicideKillButton.Timer = CoolTime;
        buttonTimer = DateTime.Now;
    }
}
