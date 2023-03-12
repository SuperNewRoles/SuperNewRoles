using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor.MadRole;

public class Worshiper : RoleBase<Worshiper>
{
    public static Color color = ImpostorRed;

    public Worshiper()
    {
        RoleId = roleId = RoleId.Worshiper;
        //以下いるもののみ変更
        OptionId = 1161;
        IsSHRRole = true;
        OptionType = CustomOptionType.Crewmate;
        CanUseVentOptionOn = true;
        CanUseVentOptionDefault = false;
        IsImpostorViewOptionOn = true;
        IsImpostorViewOptionDefault = false;
        CoolTimeOptionOn = true;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp()
    {
        SetButtonCooldowns();
    }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() { }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm)
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
        suicideButton.MaxTimer = !ModeHandler.IsMode(ModeId.SuperHostRoles) ? WorshiperAbilitySuicideCoolTime.GetFloat() : 0;
        suicideButton.Timer = !ModeHandler.IsMode(ModeId.SuperHostRoles) ? WorshiperAbilitySuicideCoolTime.GetFloat() : 0;
        suicideButtonTimer = DateTime.Now;
    }
    private static void ResetSuicideKillButton()
    {
        /*
            [isfirstResetCool(初回クールリセットか?)]がtrueの場合、クールを10sにしている。
            SHR時非導入者クール(10s)と導入者のクール(カスタムクール)が異なる事を、SNR時共通処理として修正している。
            初手カスタムボタンクールを10sにするメソッドがあれば不要な処理。
        */
        suicideKillButton.MaxTimer = !isfirstResetCool ? WorshiperKillSuicideCoolTime.GetFloat() : 10f;
        suicideKillButton.Timer = !isfirstResetCool ? WorshiperKillSuicideCoolTime.GetFloat() : 10f;
        suicideKillButtonTimer = DateTime.Now;
        Logger.Info($"「初回クールリセットか?」が{isfirstResetCool}の為、クールを[{suicideKillButton.MaxTimer}s]に設定しました。", "Worshiper");
        isfirstResetCool = false;
    }
    public static void SetButtonCooldowns()
    {
        ResetSuicideButton();
        ResetSuicideKillButton();
    }

    public static CustomOption WorshiperAbilitySuicideCoolTime;
    public static CustomOption WorshiperKillSuicideCoolTime;
    public static CustomOption WorshiperIsCheckImpostor;
    public static CustomOption WorshiperCommonTask;
    public static CustomOption WorshiperShortTask;
    public static CustomOption WorshiperLongTask;
    public static CustomOption WorshiperCheckImpostorTask;
    public override void SetupMyOptions()
    {
        WorshiperAbilitySuicideCoolTime = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "WorshiperAbilitySuicideCoolTime", 30f, 0f, 60f, 2.5f, RoleOption, format: "unitSeconds"); OptionId++;
        WorshiperKillSuicideCoolTime = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "WorshiperKillSuicideCoolTime", 30f, 2.5f, 60f, 2.5f, RoleOption, format: "unitSeconds"); OptionId++;
        WorshiperIsCheckImpostor = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, RoleOption); OptionId++;
        var Worshiperoption = SelectTask.TaskSetting(OptionId, OptionId + 1, OptionId + 2, WorshiperIsCheckImpostor, CustomOptionType.Crewmate, true); OptionId += 3;
        WorshiperCommonTask = Worshiperoption.Item1;
        WorshiperShortTask = Worshiperoption.Item2;
        WorshiperLongTask = Worshiperoption.Item3;
        WorshiperCheckImpostorTask = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, WorshiperIsCheckImpostor);
    }

    // RoleClass
    public static bool IsImpostorCheck;
    public static int ImpostorCheckTask;
    private static CustomButton suicideButton;

    private static CustomButton suicideKillButton;
    private static DateTime suicideButtonTimer;
    private static DateTime suicideKillButtonTimer;
    private static bool isfirstResetCool;

    public static void Clear()
    {
        players = new();

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
}