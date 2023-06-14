using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Roles.RoleClass;
namespace SuperNewRoles.Roles.Impostor.MadRole;

public static class Worshiper
{
    private const int optionId = 401100;// 設定のId

    // CustomOptionHolder
    public static CustomRoleOption WorshiperOption;
    public static CustomOption WorshiperPlayerCount;
    public static CustomOption WorshiperAbilitySuicideCoolTime;
    public static CustomOption WorshiperKillSuicideCoolTime;
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
        WorshiperAbilitySuicideCoolTime = CustomOption.Create(optionId + 2, false, CustomOptionType.Crewmate, "WorshiperAbilitySuicideCoolTime", 30f, 0f, 60f, 2.5f, WorshiperOption, format: "unitSeconds");
        WorshiperKillSuicideCoolTime = CustomOption.Create(optionId + 3, true, CustomOptionType.Crewmate, "WorshiperKillSuicideCoolTime", 30f, 2.5f, 60f, 2.5f, WorshiperOption, format: "unitSeconds");
        WorshiperIsUseVent = CustomOption.Create(optionId + 4, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, WorshiperOption);
        WorshiperIsImpostorLight = CustomOption.Create(optionId + 5, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, WorshiperOption);
        WorshiperIsCheckImpostor = CustomOption.Create(optionId + 6, false, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, WorshiperOption);
        var Worshiperoption = SelectTask.TaskSetting(optionId + 7, optionId + 8, optionId + 9, WorshiperIsCheckImpostor, CustomOptionType.Crewmate, true);
        WorshiperCommonTask = Worshiperoption.Item1;
        WorshiperShortTask = Worshiperoption.Item2;
        WorshiperLongTask = Worshiperoption.Item3;
        WorshiperCheckImpostorTask = CustomOption.Create(optionId + 10, false, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, WorshiperIsCheckImpostor);
    }

    // RoleClass
    public static List<PlayerControl> WorshiperPlayer;
    public static Color32 color = ImpostorRed;
    public static bool IsUseVent;
    public static bool IsImpostorLight;
    public static bool IsImpostorCheck;
    public static int ImpostorCheckTask;
    public static float AbilitySuicideCoolTime;
    public static float KillSuicideCoolTime;
    private static DateTime suicideButtonTimer;
    private static DateTime suicideKillButtonTimer;
    private static bool isfirstResetCool;// カスタムボタンの初手クールを10sにするメソッドができれば不要
    public static void ClearAndReload()
    {
        WorshiperPlayer = new();

        AbilitySuicideCoolTime = WorshiperAbilitySuicideCoolTime.GetFloat();
        KillSuicideCoolTime = WorshiperKillSuicideCoolTime.GetFloat();

        IsUseVent = WorshiperIsUseVent.GetBool();
        IsImpostorLight = WorshiperIsImpostorLight.GetBool();
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

        isfirstResetCool = true;
    }


    // Button
    private static CustomButton suicideButton;

    private static CustomButton suicideKillButton;

    public static void SetupCustomButtons(HudManager hm)
    {
        suicideButton = new(
            () =>
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    RoleHelpers.UseShapeshift();
                }
                else
                {
                    Suicide();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Worshiper; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { ResetSuicideButton(); },
            SuicideWisher.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            hm,
            hm.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("WorshiperSuicide"),
            showButtonText = true
        };

        suicideKillButton = new(
            () =>
            {
                Suicide();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Worshiper; },
            () =>
            {
                var Target = PlayerControlFixedUpdatePatch.SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () => { ResetSuicideKillButton(); },
            hm.KillButton.graphic.sprite,
            new Vector3(0f, 1f, 0f),
            hm,
            hm.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        );
        {
            suicideKillButton.buttonText = ModTranslation.GetString("WorshiperSuicide");
            suicideKillButton.showButtonText = true;
        }
    }

    private static void Suicide()
    {
        //自殺
        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
        PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
    }

    private static void ResetSuicideButton()
    {
        /*
            初手&会議終了後のクールが非導入者の場合0sになってしまう為、
            SHR時はアビリティ自決のクールを0s固定にして、導入者と非導入者の差異を無くした。
        */
        suicideButton.MaxTimer = !ModeHandler.IsMode(ModeId.SuperHostRoles) ? AbilitySuicideCoolTime : 0;
        suicideButton.Timer = !ModeHandler.IsMode(ModeId.SuperHostRoles) ? AbilitySuicideCoolTime : 0;
        suicideButtonTimer = DateTime.Now;
    }

    private static void ResetSuicideKillButton()
    {
        /*
            [isfirstResetCool(初回クールリセットか?)]がtrueの場合、クールを10sにしている。
            SHR時非導入者クール(10s)と導入者のクール(カスタムクール)が異なる事を、SNR時共通処理として修正している。
            初手カスタムボタンクールを10sにするメソッドがあれば不要な処理。
        */
        suicideKillButton.MaxTimer = !isfirstResetCool ? KillSuicideCoolTime : 10f;
        suicideKillButton.Timer = !isfirstResetCool ? KillSuicideCoolTime : 10f;
        suicideKillButtonTimer = DateTime.Now;
        Logger.Info($"「初回クールリセットか?」が{isfirstResetCool}の為、クールを[{suicideKillButton.MaxTimer}s]に設定しました。", "Worshiper");
        isfirstResetCool = false;
    }
}