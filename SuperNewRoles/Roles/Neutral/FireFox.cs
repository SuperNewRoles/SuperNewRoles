using System.Collections.Generic;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;
using SuperNewRoles.ReplayManager;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.RoleBases;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class FireFox : RoleBase<FireFox>
{
    public static Color32 color = new(255, 127, 28, byte.MaxValue);

    public FireFox()
    {
        RoleId = roleId = RoleId.FireFox;
        //以下いるもののみ変更
        OptionId = 1182;
        OptionType = CustomOptionType.Neutral;
        HasTask = false;
        CanUseVentOptionOn = true;
        CanUseVentOptionDefault = false;
        IsImpostorViewOptionOn = true;
        IsImpostorViewOptionDefault = false;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit()
    {
        KillCount = FireFoxMaxKillCount.GetInt();
        AbilityLimit = FireFoxMaxKillCount.GetInt();
    }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static CustomButton FireFoxKillButton;
    public static TMP_Text FireFoxKillNumText;
    public static void MakeButtons(HudManager hm)
    {
        FireFoxKillButton = new(
            () =>
            {
                FireFox role = local;
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!(target && PlayerControl.LocalPlayer.CanMove) || RoleHelpers.IsDead(PlayerControl.LocalPlayer) || role.AbilityLimit <= 0) return;
                if (FireFoxCanKillCrewmate.GetBool() && target.IsCrew() && !(target.IsMadRoles() || target.IsFriendRoles() || target.IsRole(RoleId.MadKiller) || target.IsRole(RoleId.Dependents))) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FireFoxCanKillImpostor.GetBool() && (target.IsImpostor() || target.IsMadRoles() || target.IsRole(RoleId.MadKiller) || target.IsRole(RoleId.Dependents))) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FireFoxCanKillNeutral.GetBool() && (target.IsNeutral() || target.IsFriendRoles())) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                if (FireFoxCanKillLovers.GetBool() && target.IsLovers()) ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                role.AbilityLimit--;
                role.KillCount--;
                FireFoxKillButton.Timer = FireFoxKillButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.FireFox; },
            () =>
            {
                FireFox role = local;
                FireFoxKillNumText.text = string.Format(ModTranslation.GetString("SheriffNumTextName"), role.AbilityLimit);
                if (role.AbilityLimit <= 0) return false;
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
            hm.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            hm,
            hm.KillButton,
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
    public static void SetButtonCooldowns() { }

    public int KillCount
    {
        get { return ReplayData.CanReplayCheckPlayerView ? (int)GetValueFloat("FireFoxKillCount") : _KillCount; }
        set { if (ReplayData.CanReplayCheckPlayerView) SetValueFloat("FireFoxKillCount", value); else _KillCount = value; }
    }
    public int _KillCount;

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

    public override void SetupMyOptions()
    {
        FireFoxReport = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "MinimalistReportSetting", true, RoleOption); OptionId++;
        FireFoxMaxKillCount = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, RoleOption); OptionId++;
        FireFoxKillCool = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "KillCooldown", 30f, 0f, 60f, 2.5f, RoleOption); OptionId++;
        FireFoxCanKillCrewmate = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "FireFoxCanKillCrewmateSetting", false, RoleOption); OptionId++;
        FireFoxCanKillImpostor = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "FireFoxCanKillImpostorSetting", true, RoleOption); OptionId++;
        FireFoxCanKillNeutral = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "FireFoxCanKillNeutralSetting", false, RoleOption); OptionId++;
        FireFoxCanKillLovers = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "FireFoxCanKillLoversSetting", false, RoleOption); OptionId++;
        FireFoxIsCheckFox = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "FireFoxIsCheckFoxSetting", true, RoleOption); OptionId++;
    }

    public static void Clear()
    {
        players = new();
    }
}