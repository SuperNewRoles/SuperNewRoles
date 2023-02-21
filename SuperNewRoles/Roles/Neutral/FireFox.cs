using System.Collections.Generic;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class FireFox
{
    private const int OptionId = 1182;// 設定のId
    public static CustomRoleOption FireFoxOption;
    public static CustomOption FireFoxPlayerCount;
    public static CustomOption FireFoxMaxKillCount;
    public static CustomOption FireFoxKillCool;
    public static CustomOption FireFoxCanKillCrewmate;
    public static CustomOption FireFoxCanKillImpostor;
    public static CustomOption FireFoxCanKillNeutral;
    public static CustomOption FireFoxCanKillLovers;
    public static CustomOption FireFoxIsCheckFox;
    public static CustomOption FireFoxIsUseVent;
    public static CustomOption FireFoxIsImpostorLight;
    public static CustomOption FireFoxReport;
    public static void SetupCustomOptions()
    {
        FireFoxOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.FireFox);
        FireFoxPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], FireFoxOption);
        FireFoxMaxKillCount = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, FireFoxOption);
        FireFoxKillCool = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "KillCooldown", 30f, 0f, 60f, 2.5f, FireFoxOption);
        FireFoxCanKillCrewmate = CustomOption.Create(OptionId + 4, false, CustomOptionType.Neutral, "FireFoxCanKillCrewmateSetting", false, FireFoxOption);
        FireFoxCanKillImpostor = CustomOption.Create(OptionId + 5, false, CustomOptionType.Neutral, "FireFoxCanKillImpostorSetting", true, FireFoxOption);
        FireFoxCanKillNeutral = CustomOption.Create(OptionId + 6, false, CustomOptionType.Neutral, "FireFoxCanKillNeutralSetting", false, FireFoxOption);
        FireFoxCanKillLovers = CustomOption.Create(OptionId + 7, false, CustomOptionType.Neutral, "FireFoxCanKillLoversSetting", false, FireFoxOption);
        FireFoxIsCheckFox = CustomOption.Create(OptionId + 8, false, CustomOptionType.Neutral, "FireFoxIsCheckFoxSetting", true, FireFoxOption);
        FireFoxIsUseVent = CustomOption.Create(OptionId + 9, false, CustomOptionType.Neutral, "MadmateUseVentSetting", false, FireFoxOption);
        FireFoxIsImpostorLight = CustomOption.Create(OptionId + 10, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, FireFoxOption);
        FireFoxReport = CustomOption.Create(OptionId + 11, false, CustomOptionType.Neutral, "MinimalistReportSetting", true, FireFoxOption);
    }

    public static List<PlayerControl> FireFoxPlayer;
    public static Color32 color = new(255, 127, 28, byte.MaxValue);
    public static int KillCount;
    public static void ClearAndReload()
    {
        FireFoxPlayer = new();
        KillCount = FireFoxMaxKillCount.GetInt();
    }

    public static CustomButton FireFoxKillButton;
    public static TMP_Text FireFoxKillNumText;
    public static void SetupCustomButtons(HudManager __instance)
    {
        FireFoxKillButton = new(
            () =>
            {
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!(target && PlayerControl.LocalPlayer.CanMove) || RoleHelpers.IsDead(PlayerControl.LocalPlayer) || KillCount <= 0) return;
                if (FireFoxCanKillCrewmate.GetBool() && target.IsCrew() && !(target.IsMadRoles() || target.IsFriendRoles() || target.IsRole(RoleId.MadKiller) || target.IsRole(RoleId.Dependents))) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FireFoxCanKillImpostor.GetBool() && (target.IsImpostor() || target.IsMadRoles() || target.IsRole(RoleId.MadKiller) || target.IsRole(RoleId.Dependents))) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FireFoxCanKillNeutral.GetBool() && (target.IsNeutral() || target.IsFriendRoles())) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FireFoxCanKillLovers.GetBool() && target.IsLovers()) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                KillCount--;
                FireFoxKillButton.Timer = FireFoxKillButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.FireFox; },
            () =>
            {
                FireFoxKillNumText.text = string.Format(ModTranslation.GetString("SheriffNumTextName"), KillCount);
                if (KillCount <= 0) return false;
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!(target && PlayerControl.LocalPlayer.CanMove)) return false;
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, color);
                if (target.IsRole(RoleId.FireFox) && !FireFoxCanKillLovers.GetBool()) return false;
                if (target.IsRole(RoleId.Fox) && !FireFoxCanKillLovers.GetBool() && FireFoxIsCheckFox.GetBool()) return false;
                return true;
            },
            () =>
            {
                FireFoxKillButton.MaxTimer = FireFoxKillCool.GetFloat();
                FireFoxKillButton.Timer = FireFoxKillButton.MaxTimer;
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
        FireFoxKillNumText = Object.Instantiate(FireFoxKillButton.actionButton.cooldownTimerText, FireFoxKillButton.actionButton.cooldownTimerText.transform.parent);
        FireFoxKillNumText.text = string.Format(ModTranslation.GetString("SheriffNumTextName"), FireFoxMaxKillCount.GetInt().ToString());
        FireFoxKillNumText.enableWordWrapping = false;
        FireFoxKillNumText.transform.localScale = Vector3.one * 0.5f;
        FireFoxKillNumText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }
}