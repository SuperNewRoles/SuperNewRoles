using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Roles.RoleClass;
namespace SuperNewRoles.Roles.Impostor.MadRole;

public static class Worshiper
{
    internal class CustomOptionData
    {
        private static int optionId = 401100;// 設定のId

        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption AbilitySuicideCoolTime;
        public static CustomOption KillSuicideCoolTime;
        public static CustomOption IsCheckImpostor;
        public static CustomOption IsSettingNumberOfUniqueTasks;
        public static CustomOption CommonTask;
        public static CustomOption ShortTask;
        public static CustomOption LongTask;
        public static CustomOption IsParcentageForTaskTrigger;
        public static CustomOption ParcentageForTaskTriggerSetting;
        public static CustomOption IsUseVent;
        public static CustomOption IsImpostorLight;

        public static void SetupCustomOptions()
        {
            Option = new(optionId, true, CustomOptionType.Crewmate, "WorshiperName", RoleData.color, 1); optionId++;
            PlayerCount = Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option); optionId++;
            AbilitySuicideCoolTime = Create(optionId, false, CustomOptionType.Crewmate, "WorshiperAbilitySuicideCoolTime", 30f, 0f, 60f, 2.5f, Option, format: "unitSeconds"); optionId++;
            KillSuicideCoolTime = Create(optionId, true, CustomOptionType.Crewmate, "WorshiperKillSuicideCoolTime", 30f, 2.5f, 60f, 2.5f, Option, format: "unitSeconds"); optionId++;
            IsUseVent = Create(optionId, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, Option); optionId++;
            IsImpostorLight = Create(optionId, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, Option); optionId++;
            IsCheckImpostor = Create(optionId, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, Option); optionId++;
            IsSettingNumberOfUniqueTasks = Create(optionId, false, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, IsCheckImpostor); optionId++;
            var taskOption = SelectTask.TaskSetting(optionId, optionId + 1, optionId + 2, IsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true); optionId += 3;
            CommonTask = taskOption.Item1;
            ShortTask = taskOption.Item2;
            LongTask = taskOption.Item3;
            IsParcentageForTaskTrigger = Create(optionId, false, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, IsCheckImpostor); optionId++;
            ParcentageForTaskTriggerSetting = Create(optionId, false, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, IsParcentageForTaskTrigger);
        }
    }

    internal class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = ImpostorRed;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static float AbilitySuicideCoolTime;
        public static float KillSuicideCoolTime;
        internal static DateTime suicideButtonTimer;
        internal static DateTime suicideKillButtonTimer;
        public static void ClearAndReload()
        {
            Player = new();

            AbilitySuicideCoolTime = CustomOptionData.AbilitySuicideCoolTime.GetFloat();
            KillSuicideCoolTime = CustomOptionData.KillSuicideCoolTime.GetFloat();

            IsUseVent = CustomOptionData.IsUseVent.GetBool();
            IsImpostorLight = CustomOptionData.IsImpostorLight.GetBool();
            IsImpostorCheck = CustomOptionData.IsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles);

            bool IsFullTask = !CustomOptionData.IsSettingNumberOfUniqueTasks.GetBool();
            int AllTask = SelectTask.GetTotalTasks(RoleId.Worshiper);

            ImpostorCheckTask = IsFullTask ? AllTask : (int)(AllTask * (int.Parse(CustomOptionData.ParcentageForTaskTriggerSetting.GetString().Replace("%", "")) / 100f));
        }
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
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleData.color);
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
        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
        PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
    }

    private static void ResetSuicideButton()
    {
        /*
            初手&会議終了後のクールが非導入者の場合0sになってしまう為、
            SHR時はアビリティ自決のクールを0s固定にして、導入者と非導入者の差異を無くした。
        */
        suicideButton.MaxTimer = !ModeHandler.IsMode(ModeId.SuperHostRoles) ? RoleData.AbilitySuicideCoolTime : 0;
        suicideButton.Timer = !ModeHandler.IsMode(ModeId.SuperHostRoles) ? RoleData.AbilitySuicideCoolTime : 0;
        RoleData.suicideButtonTimer = DateTime.Now;
    }

    private static void ResetSuicideKillButton()
    {
        var cooldown = IsfirstResetCool ? 10f : RoleData.KillSuicideCoolTime;
        suicideKillButton.MaxTimer = cooldown;
        suicideKillButton.Timer = cooldown;
        RoleData.suicideKillButtonTimer = DateTime.Now;
        IsfirstResetCool = false;
    }
}