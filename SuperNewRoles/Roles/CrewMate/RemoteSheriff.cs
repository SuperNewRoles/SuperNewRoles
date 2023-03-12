using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class RemoteSheriff : RoleBase<RemoteSheriff>
{
    public static Color color = RoleClass.SheriffYellow;

    public RemoteSheriff()
    {
        RoleId = roleId = RoleId.RemoteSheriff;
        //以下いるもののみ変更
        HasTask = AmongUsClient.Instance is not null ? !ModeHandler.IsMode(ModeId.SuperHostRoles) : true;
        IsAssignRoleFirst = true;
        OptionId = 711;
        IsSHRRole = true;
        OptionType = CustomOptionType.Crewmate;
        IsChangeOutfitRole = true;
        CoolTimeOptionOn = true;
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
    public override void PostInit() { AbilityLimit = RemoteSheriffKillMaxCount.GetInt(); }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    public static CustomButton RemoteSheriffKillButton;
    public static TMPro.TMP_Text sheriffNumShotsText;

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm)
    {
        RemoteSheriffKillButton = new(
            RoleHelpers.UseShapeshift,
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.RemoteSheriff; },
            () =>
            {
                float killCount = 0f;
                bool flag = false;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
                {
                    killCount = local.AbilityLimit;
                    flag = true;
                }
                if (!PlayerControl.LocalPlayer.GetRoleObject().CanUseAbility()) flag = false;
                sheriffNumShotsText.text = killCount > 0 ? string.Format(ModTranslation.GetString("SheriffNumTextName"), killCount) : ModTranslation.GetString("CannotUse");
                return flag;
            },
            ResetKillCooldown,
            Sheriff.GetButtonSprite(),
            new Vector3(0f, 1f, 0),
            hm,
            hm.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        );
        sheriffNumShotsText = GameObject.Instantiate(RemoteSheriffKillButton.actionButton.cooldownTimerText, RemoteSheriffKillButton.actionButton.cooldownTimerText.transform.parent);
        sheriffNumShotsText.text = "";
        sheriffNumShotsText.enableWordWrapping = false;
        sheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
        sheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        RemoteSheriffKillButton.buttonText = ModTranslation.GetString("SheriffKillButtonName");
        RemoteSheriffKillButton.showButtonText = true;
    }

    public static void ResetKillCooldown()
    {
        try
        {
            HudManagerStartPatch.SheriffKillButton.MaxTimer = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0f : CoolTimeS;
            HudManagerStartPatch.SheriffKillButton.Timer = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0f : CoolTimeS;
        }
        catch { }
    }
    public static void SetButtonCooldowns() { }

    public static CustomOption RemoteSheriffAlwaysKills;
    public static CustomOption RemoteSheriffMadRoleKill;
    public static CustomOption RemoteSheriffNeutralKill;
    public static CustomOption RemoteSheriffFriendRolesKill;
    public static CustomOption RemoteSheriffLoversKill;
    public static CustomOption RemoteSheriffQuarreledKill;
    public static CustomOption RemoteSheriffKillMaxCount;
    public static CustomOption RemoteSheriffIsKillTeleportSetting;

    public override void SetupMyOptions() {
        RemoteSheriffKillMaxCount = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, RoleOption, format: "unitSeconds"); OptionId++;
        RemoteSheriffIsKillTeleportSetting = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "RemoteSheriffIsKillTeleportSetting", false, RoleOption); OptionId++;
        RemoteSheriffAlwaysKills = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffAlwaysKills", false, RoleOption); OptionId++;
        RemoteSheriffMadRoleKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, RoleOption); OptionId++;
        RemoteSheriffNeutralKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, RoleOption); OptionId++;
        RemoteSheriffFriendRolesKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, RoleOption); OptionId++;
        RemoteSheriffLoversKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, RoleOption); OptionId++;
        RemoteSheriffQuarreledKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, RoleOption); OptionId++;
    }

    public static void Clear()
    {
        players = new();
    }
}