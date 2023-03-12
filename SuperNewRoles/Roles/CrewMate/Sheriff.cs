using System;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Sheriff : RoleBase<Sheriff>
{
    public static Color32 color = RoleClass.SheriffYellow;

    public Sheriff()
    {
        RoleId = roleId = RoleId.Sheriff;
        //以下いるもののみ変更
        OptionId = 700;
        IsSHRRole = true;
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
    public override void PostInit() { AbilityLimit = SheriffKillMaxCount.GetInt(); }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    public static CustomButton SheriffKillButton;
    public static TMPro.TMP_Text sheriffNumShotsText;

    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) {

        SheriffKillButton = new(
            () =>
            {
                if (local.CanUseAbility() && HudManagerStartPatch.SetTarget())
                {
                    var target = PlayerControlFixedUpdatePatch.SetTarget();
                    var localId = CachedPlayer.LocalPlayer.PlayerId;
                    var misfire = !Sheriff.IsSheriffRolesKill(CachedPlayer.LocalPlayer, target);
                    PlayerControlFixedUpdatePatch.SetPlayerOutline(target, color);
                    var alwaysKill = !IsSheriffRolesKill(CachedPlayer.LocalPlayer, target) && Sheriff.SheriffAlwaysKills.GetBool();
                    if (alwaysKill && target.IsRole(RoleId.Squid) && Squid.IsVigilance.ContainsKey(target.PlayerId) && Squid.IsVigilance[target.PlayerId])
                    {
                        alwaysKill = false;
                        Squid.SetVigilance(target, false);
                        Squid.SetSpeedBoost(target);
                        RPCHelper.StartRPC(CustomRPC.ShowFlash, target).EndRPC();
                    }
                    var targetId = target.PlayerId;

                    RPCProcedure.SheriffKill(localId, targetId, misfire, alwaysKill);
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, SendOption.Reliable, -1);
                    killWriter.Write(localId);
                    killWriter.Write(targetId);
                    killWriter.Write(misfire);
                    killWriter.Write(alwaysKill);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    FinalStatusClass.RpcSetFinalStatus(misfire ? CachedPlayer.LocalPlayer : target, misfire ? FinalStatus.SheriffMisFire : (target.IsRole(RoleId.HauntedWolf) ? FinalStatus.SheriffHauntedWolfKill : FinalStatus.SheriffKill));
                    if (alwaysKill) FinalStatusClass.RpcSetFinalStatus(target, FinalStatus.SheriffInvolvedOutburst);
                    ResetKillCooldown();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Sheriff; },
            () =>
            {
                float killCount = 0f;
                bool flag = false;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff))
                {
                    killCount = PlayerControl.LocalPlayer.GetRoleObject().AbilityLimit;
                    flag = PlayerControlFixedUpdatePatch.SetTarget() && PlayerControl.LocalPlayer.CanMove;
                }
                if (!PlayerControl.LocalPlayer.GetRoleObject().CanUseAbility()) flag = false;
                sheriffNumShotsText.text = killCount > 0 ? string.Format(ModTranslation.GetString("SheriffNumTextName"), killCount) : ModTranslation.GetString("CannotUse");
                return flag;
            },
            EndMeeting,
            GetButtonSprite(),
            new Vector3(0f, 1f, 0),
            hm,
            hm.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        );
        sheriffNumShotsText = GameObject.Instantiate(SheriffKillButton.actionButton.cooldownTimerText, SheriffKillButton.actionButton.cooldownTimerText.transform.parent);
        sheriffNumShotsText.text = "";
        sheriffNumShotsText.enableWordWrapping = false;
        sheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
        sheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        SheriffKillButton.buttonText = ModTranslation.GetString("SheriffKillButtonName");
        SheriffKillButton.showButtonText = true;
    }
    public static void SetButtonCooldowns() { }

    public static CustomOption SheriffKillMaxCount;
    public static CustomOption SheriffCanKillImpostor;
    public static CustomOption SheriffAlwaysKills;
    public static CustomOption SheriffMadRoleKill;
    public static CustomOption SheriffFriendsRoleKill;
    public static CustomOption SheriffNeutralKill;
    public static CustomOption SheriffLoversKill;
    public static CustomOption SheriffQuarreledKill;

    public override void SetupMyOptions() {
        SheriffKillMaxCount = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, RoleOption, format: "unitSeconds"); OptionId++;
        SheriffAlwaysKills = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffAlwaysKills", false, RoleOption); OptionId++;
        SheriffCanKillImpostor = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillImpostorSetting", true, RoleOption); OptionId++;
        SheriffMadRoleKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, RoleOption); OptionId++;
        SheriffNeutralKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, RoleOption); OptionId++;
        SheriffFriendsRoleKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, RoleOption); OptionId++;
        SheriffLoversKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, RoleOption); OptionId++;
        SheriffQuarreledKill = CustomOption.Create(OptionId, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, RoleOption); OptionId++;

    }

    public static void Clear()
    {
        players = new();
    }

    public static void ResetKillCooldown()
    {
        try
        {
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Chief.SheriffPlayer.Contains(CachedPlayer.LocalPlayer.PlayerId)
                ? RoleClass.Chief.CoolTime
                : Sheriff.CoolTimeS;
            HudManagerStartPatch.SheriffKillButton.Timer = HudManagerStartPatch.SheriffKillButton.MaxTimer;
        }
        catch { }
    }

    public static bool IsSheriffRolesKill(PlayerControl sheriff, PlayerControl target)
    {
        var targetRoleData = CountChanger.GetRoleType(target);
        var isImpostorKill = true;
        var isMadRolesKill = false;
        var isNeutralKill = false;
        var isFriendRolesKill = false;
        var isLoversKill = false;
        var isQuarreledKill = false;

        RoleId role = sheriff.GetRole();

        switch (role)
        {
            case RoleId.Sheriff:
                // 通常Sheriffの場合
                if (!RoleClass.Chief.SheriffPlayer.Contains(sheriff.PlayerId))
                {
                    isImpostorKill = SheriffCanKillImpostor.GetBool();
                    isMadRolesKill = SheriffMadRoleKill.GetBool(); ;
                    isNeutralKill = SheriffNeutralKill.GetBool();
                    isFriendRolesKill = SheriffFriendsRoleKill.GetBool();
                    isLoversKill = SheriffLoversKill.GetBool();
                    isQuarreledKill = SheriffQuarreledKill.GetBool();
                }
                else // 村長シェリフの場合
                {
                    isImpostorKill = CustomOptionHolder.ChiefSheriffCanKillImpostor.GetBool();
                    isMadRolesKill = CustomOptionHolder.ChiefSheriffCanKillMadRole.GetBool();
                    isNeutralKill = CustomOptionHolder.ChiefSheriffCanKillNeutral.GetBool();
                    isFriendRolesKill = CustomOptionHolder.ChiefSheriffFriendsRoleKill.GetBool();
                    isLoversKill = CustomOptionHolder.ChiefSheriffCanKillLovers.GetBool();
                    isQuarreledKill = CustomOptionHolder.ChiefSheriffQuarreledKill.GetBool();
                }
                break;
            case RoleId.RemoteSheriff:
                isMadRolesKill = RemoteSheriff.RemoteSheriffMadRoleKill.GetBool();
                isNeutralKill = RemoteSheriff.RemoteSheriffNeutralKill.GetBool();
                isFriendRolesKill = RemoteSheriff.RemoteSheriffFriendRolesKill.GetBool();
                isLoversKill = RemoteSheriff.RemoteSheriffLoversKill.GetBool();
                isQuarreledKill = RemoteSheriff.RemoteSheriffQuarreledKill.GetBool();
                break;
            case RoleId.MeetingSheriff:
                isMadRolesKill = CustomOptionHolder.MeetingSheriffMadRoleKill.GetBool();
                isNeutralKill = CustomOptionHolder.MeetingSheriffNeutralKill.GetBool();
                isFriendRolesKill = CustomOptionHolder.MeetingSheriffFriendsRoleKill.GetBool();
                isLoversKill = CustomOptionHolder.MeetingSheriffLoversKill.GetBool();
                isQuarreledKill = CustomOptionHolder.MeetingSheriffQuarreledKill.GetBool();
                break;
        }
        if ((targetRoleData == TeamRoleType.Impostor) || target.IsRole(RoleId.HauntedWolf)) return isImpostorKill;//インポスター、狼付きは設定がimp設定が有効な時切れる
        if (target.IsMadRoles()
            || target.IsRole(RoleId.MadKiller)
            || target.IsRole(RoleId.Dependents))
            return isMadRolesKill;
        if (target.IsNeutral()) return isNeutralKill;
        if (target.IsFriendRoles()) return isFriendRolesKill;
        if (target.IsLovers()) return isLoversKill;//ラバーズ
        if (target.IsQuarreled()) return isQuarreledKill;//クラード
        return false;
    }

    public static bool IsSheriff(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Sheriff) || Player.IsRole(RoleId.RemoteSheriff);
    }
    public static void EndMeeting()
    {
        ResetKillCooldown();
    }
}