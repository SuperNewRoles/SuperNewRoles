using System;
using TMPro;
using System.Timers;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Mode;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Roles.Impostor.MadRole;

public static class MadRaccoon
{
    internal static class CustomOptionData
    {
        private static int optionId = 1321;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption IsCheckImpostor;
        public static CustomOption IsSettingNumberOfUniqueTasks;
        public static CustomOption CommonTask;
        public static CustomOption ShortTask;
        public static CustomOption LongTask;
        public static CustomOption IsParcentageForTaskTrigger;
        public static CustomOption ParcentageForTaskTriggerSetting;
        public static CustomOption IsUseVent;
        public static CustomOption IsImpostorLight;
        public static CustomOption ShapeshifterCooldown;
        public static CustomOption ShapeshifterDuration;

        public static void SetupCustomOptions()
        {
            Option = SetupCustomRoleOption(optionId, true, RoleId.MadRaccoon); optionId++;
            PlayerCount = Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option); optionId++;
            IsUseVent = Create(optionId, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, Option); optionId++;
            IsImpostorLight = Create(optionId, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, Option); optionId++;
            IsCheckImpostor = Create(optionId, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, Option); optionId++;
            IsSettingNumberOfUniqueTasks = Create(optionId, false, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, IsCheckImpostor); optionId++;
            var taskOption = SelectTask.TaskSetting(optionId, optionId + 1, optionId + 2, IsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true); optionId += 3;
            CommonTask = taskOption.Item1;
            ShortTask = taskOption.Item2;
            LongTask = taskOption.Item3;
            IsParcentageForTaskTrigger = Create(optionId, false, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, IsCheckImpostor); optionId++;
            ParcentageForTaskTriggerSetting = Create(optionId, false, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, IsParcentageForTaskTrigger); optionId++;
            ShapeshifterCooldown = Create(optionId, true, CustomOptionType.Crewmate, "DoppelgangerCooldownSetting", 5f, 5f, 60f, 2.5f, Option); optionId++;
            ShapeshifterDuration = Create(optionId, true, CustomOptionType.Crewmate, "DoppelgangerDurationTimeSetting", 90f, 0f, 250f, 5f, Option);
        }
    }

    internal static class RoleClass
    {
        public static List<PlayerControl> Player;
        public static Color32 color = Roles.RoleClass.ImpostorRed;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static float ShapeshifterCooldown;
        public static float ShapeshifterDuration;
        public static bool IsShapeNow;
        public static void ClearAndReload()
        {
            Player = new();

            IsUseVent = CustomOptionData.IsUseVent.GetBool();
            IsImpostorLight = CustomOptionData.IsImpostorLight.GetBool();
            IsImpostorCheck = CustomOptionData.IsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles);

            bool IsFullTask = !CustomOptionData.IsSettingNumberOfUniqueTasks.GetBool();
            int AllTask = SelectTask.GetTotalTasks(RoleId.MadRaccoon);
            ImpostorCheckTask = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0 : IsFullTask ? AllTask : (int)(AllTask * (int.Parse(CustomOptionData.ParcentageForTaskTriggerSetting.GetString().Replace("%", "")) / 100f));

            ShapeshifterCooldown = CustomOptionData.ShapeshifterCooldown.GetFloat();
            ShapeshifterDuration = CustomOptionData.ShapeshifterDuration.GetFloat();
            IsShapeNow = false;
        }
    }

    internal static class Button
    {
        private static CustomButton shapeshiftButton;
        private static Timer coolTimeTimer;
        private static Timer durationTimeTimer;
        internal static TextMeshPro shapeDurationText = null;
        private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MadRacoonButtonb.png", 115f);

        internal static void SetupCustomButtons(HudManager hm)
        {
            shapeshiftButton = new(
                () => { RoleHelpers.UseShapeshift(); },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.MadRaccoon; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { ResetShapeshiftCool(true); },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("DoppelgangerButtonName"),
                showButtonText = true
            };
            shapeDurationText = UnityEngine.Object.Instantiate(shapeshiftButton.actionButton.cooldownTimerText, shapeshiftButton.actionButton.cooldownTimerText.transform.parent);
            shapeDurationText.text = "";
            shapeDurationText.enableWordWrapping = false;
            shapeDurationText.transform.localScale = Vector3.one * 0.5f;
            shapeDurationText.transform.localPosition += new Vector3(0f, 0f, 0f);
        }

        /// <summary>
        /// シェイプの効果時間と能力持続時間の表示を制御するタイマー
        /// </summary>
        internal static void SetShapeDurationTimer()
        {
            coolTimeTimer = new Timer(RoleClass.ShapeshifterDuration * 1000);
            coolTimeTimer.Elapsed += (source, e) =>
            {
                ResetShapeDuration();
                RevertShapeshift();
            };
            coolTimeTimer.AutoReset = false;
            coolTimeTimer.Enabled = true;

            int num = (int)RoleClass.ShapeshifterDuration;
            shapeDurationText.text = $"<size=255%><color=#19fe19>{num}</color></size>";
            num--;

            durationTimeTimer = new Timer(1000);
            durationTimeTimer.Elapsed += (source, e) =>
            {
                if (num > 0)
                {
                    shapeDurationText.text = $"<size=255%><color=#19fe19>{num}</color></size>";
                    num--;
                }
            };
            durationTimeTimer.AutoReset = num >= 0;
            durationTimeTimer.Enabled = true;
        }

        /// <summary>
        /// 能力が解除された時の処理。
        /// 効果時間のタイマーを止める, クールタイムをリセットする
        /// </summary>
        internal static void ResetShapeDuration()
        {
            TimerStop();
            ResetShapeshiftCool(false);
        }

        private static void ResetShapeshiftCool(bool endMeeting)
        {
            float timerSet = !endMeeting ? RoleClass.ShapeshifterCooldown : 0f; // 会議終了時は能力クールを0sにする

            shapeshiftButton.MaxTimer = timerSet;
            shapeshiftButton.Timer = timerSet;
        }

        private static void TimerStop()
        {
            if (coolTimeTimer != null) coolTimeTimer.Stop();
            if (durationTimeTimer != null) durationTimeTimer.Stop();
            shapeDurationText.text = "";
        }

        private static void RevertShapeshift()
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
            PlayerControl.LocalPlayer.RpcRevertShapeshift(true);
        }
    }
}