using System.Collections.Generic;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class FierFox
{
    private const int OptionId = 1168;// 設定のId
    public static CustomRoleOption FierFoxOption;
    public static CustomOption FierFoxPlayerCount;
    public static CustomOption FierFoxMaxKillCount;
    public static CustomOption FierFoxKillCool;
    public static CustomOption FierFoxCanKillCrewmate;
    public static CustomOption FierFoxCanKillImpostor;
    public static CustomOption FierFoxCanKillNeutral;
    public static CustomOption FierFoxCanKillLovers;
    public static CustomOption FierFoxIsCheckFox;
    public static CustomOption FierFoxIsUseVent;
    public static CustomOption FierFoxIsImpostorLight;
    public static CustomOption FierFoxReport;
    public static void SetupCustomOptions()
    {
        FierFoxOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.FierFox);
        FierFoxPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], FierFoxOption);
        FierFoxMaxKillCount = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, FierFoxOption);
        FierFoxKillCool = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "KillCooldown", 30f, 0f, 60f, 2.5f, FierFoxOption);
        FierFoxCanKillCrewmate = CustomOption.Create(OptionId + 4, false, CustomOptionType.Neutral, "FierFoxCanKillCrewmateSetting", false, FierFoxOption);
        FierFoxCanKillImpostor = CustomOption.Create(OptionId + 5, false, CustomOptionType.Neutral, "FierFoxCanKillImpostorSetting", true, FierFoxOption);
        FierFoxCanKillNeutral = CustomOption.Create(OptionId + 6, false, CustomOptionType.Neutral, "FierFoxCanKillNeutralSetting", false, FierFoxOption);
        FierFoxCanKillLovers = CustomOption.Create(OptionId + 7, false, CustomOptionType.Neutral, "FierFoxCanKillLoversSetting", false, FierFoxOption);
        FierFoxIsCheckFox = CustomOption.Create(OptionId + 8, false, CustomOptionType.Neutral, "FierFoxIsCheckFoxSetting", true, FierFoxOption);
        FierFoxIsUseVent = CustomOption.Create(OptionId + 9, false, CustomOptionType.Neutral, "MadmateUseVentSetting", false, FierFoxOption);
        FierFoxIsImpostorLight = CustomOption.Create(OptionId + 10, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, FierFoxOption);
        FierFoxReport = CustomOption.Create(OptionId + 11, false, CustomOptionType.Neutral, "MinimalistReportSetting", true, FierFoxOption);
    }

    public static List<PlayerControl> FierFoxPlayer;
    public static Color32 color = new(255, 127, 28, byte.MaxValue);
    public static int KillCount;
    public static void ClearAndReload()
    {
        FierFoxPlayer = new();
        KillCount = FierFoxMaxKillCount.GetInt();
    }

    public static CustomButton FierFoxKillButton;
    public static TMP_Text FierFoxKillNumText;
    public static void SetupCustomButtons(HudManager __instance)
    {
        FierFoxKillButton = new(
            () =>
            {
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!(target && PlayerControl.LocalPlayer.CanMove) || RoleHelpers.IsDead(PlayerControl.LocalPlayer) || KillCount <= 0) return;
                if (FierFoxCanKillCrewmate.GetBool() && target.IsCrew() && !(target.IsMadRoles() || target.IsFriendRoles())) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FierFoxCanKillImpostor.GetBool() && (target.IsImpostor() || target.IsMadRoles())) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FierFoxCanKillNeutral.GetBool() && (target.IsNeutral() || target.IsFriendRoles())) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FierFoxCanKillLovers.GetBool() && target.IsLovers()) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                KillCount--;
                FierFoxKillButton.Timer = FierFoxKillButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.FierFox; },
            () =>
            {
                FierFoxKillNumText.text = string.Format(ModTranslation.GetString("SheriffNumTextName"), KillCount);
                if (KillCount <= 0) return false;
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!(target && PlayerControl.LocalPlayer.CanMove)) return false;
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, color);
                if (target.IsRole(RoleId.FierFox) && !FierFoxCanKillLovers.GetBool()) return false;
                if (target.IsRole(RoleId.Fox) && !FierFoxCanKillLovers.GetBool() && FierFoxIsCheckFox.GetBool()) return false;
                return true;
            },
            () =>
            {
                FierFoxKillButton.MaxTimer = FierFoxKillCool.GetFloat();
                FierFoxKillButton.Timer = FierFoxKillButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };
        FierFoxKillNumText = Object.Instantiate(FierFoxKillButton.actionButton.cooldownTimerText, FierFoxKillButton.actionButton.cooldownTimerText.transform.parent);
        FierFoxKillNumText.text = string.Format(ModTranslation.GetString("SheriffNumTextName"), FierFoxMaxKillCount.GetInt().ToString());
        FierFoxKillNumText.enableWordWrapping = false;
        FierFoxKillNumText.transform.localScale = Vector3.one * 0.5f;
        FierFoxKillNumText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }
}
