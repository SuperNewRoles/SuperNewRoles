using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles;

class Sheriff
{
    public static void ResetKillCooldown()
    {
        try
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0f : RoleClass.RemoteSheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0f : RoleClass.RemoteSheriff.CoolTime;
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            else
            {
                if (HudManagerStartPatch.SheriffKillButton != null)
                    HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Chief.SheriffPlayer.Contains(CachedPlayer.LocalPlayer.PlayerId)
                        ? RoleClass.Chief.CoolTime
                        : RoleClass.Sheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = HudManagerStartPatch.SheriffKillButton.MaxTimer;
            }
        }
        catch { }
    }

    /// <summary>
    /// シェリフキルの判定及び, シェリフキルによる死者の死因判定を行う
    /// </summary>
    /// <param name="sheriff">キルを実行したシェリフ</param>
    /// <param name="target">シェリフキルのターゲット</param>
    /// <returns>
    /// true : シェリフキル成功 / false : シェリフキル失敗,
    /// FinalStatus : 死者の死因 (シェリフとターゲットどちらが死んだかは区別しないで返却)
    /// </returns>
    public static (bool, FinalStatus) IsSheriffRolesKill(PlayerControl sheriff, PlayerControl target)
    {
        var targetRoleData = CountChanger.GetRoleType(target);
        var isImpostorKill = true;
        var isMadRolesKill = false;
        var isNeutralKill = false;
        var isFriendRolesKill = false;
        var isLoversKill = false;
        var isQuarreledKill = false;

        var isHauntedWolfDecision = sheriff.IsHauntedWolf() && Attribute.HauntedWolf.CustomOptionData.IsReverseSheriffDecision.GetBool();
        FinalStatus statusSuccess = !isHauntedWolfDecision ? FinalStatus.SheriffKill : FinalStatus.HauntedSheriffKill;
        FinalStatus statusMisFire = !isHauntedWolfDecision ? FinalStatus.SheriffMisFire : FinalStatus.HauntedSheriffMisFire;

        RoleId role = sheriff.GetRole();

        switch (role)
        {
            case RoleId.Sheriff:
                // 通常Sheriffの場合
                if (!RoleClass.Chief.SheriffPlayer.Contains(sheriff.PlayerId))
                {
                    isImpostorKill = CustomOptionHolder.SheriffCanKillImpostor.GetBool();
                    isMadRolesKill = CustomOptionHolder.SheriffMadRoleKill.GetBool(); ;
                    isNeutralKill = CustomOptionHolder.SheriffNeutralKill.GetBool();
                    isFriendRolesKill = CustomOptionHolder.SheriffFriendsRoleKill.GetBool();
                    isLoversKill = CustomOptionHolder.SheriffLoversKill.GetBool();
                    isQuarreledKill = CustomOptionHolder.SheriffQuarreledKill.GetBool();
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
                isMadRolesKill = CustomOptionHolder.RemoteSheriffMadRoleKill.GetBool();
                isNeutralKill = CustomOptionHolder.RemoteSheriffNeutralKill.GetBool();
                isFriendRolesKill = CustomOptionHolder.RemoteSheriffFriendRolesKill.GetBool();
                isLoversKill = CustomOptionHolder.RemoteSheriffLoversKill.GetBool();
                isQuarreledKill = CustomOptionHolder.RemoteSheriffQuarreledKill.GetBool();
                break;
            case RoleId.MeetingSheriff:
                isMadRolesKill = CustomOptionHolder.MeetingSheriffMadRoleKill.GetBool();
                isNeutralKill = CustomOptionHolder.MeetingSheriffNeutralKill.GetBool();
                isFriendRolesKill = CustomOptionHolder.MeetingSheriffFriendsRoleKill.GetBool();
                isLoversKill = CustomOptionHolder.MeetingSheriffLoversKill.GetBool();
                isQuarreledKill = CustomOptionHolder.MeetingSheriffQuarreledKill.GetBool();
                break;
        }

        // シェリフが狼憑きであり設定が有効なら, キル判定を反転する
        if (isHauntedWolfDecision)
        {
            isImpostorKill ^= true;
            isMadRolesKill ^= true;
            isNeutralKill ^= true;
            isFriendRolesKill ^= true;
            isLoversKill ^= true;
            isQuarreledKill ^= true;
        }

        if (targetRoleData == TeamRoleType.Impostor) return (isImpostorKill, isImpostorKill ? statusSuccess : statusMisFire); // インポスター、狼付きは設定がimp設定が有効な時切れる
        if (target.IsHauntedWolf()) return (isImpostorKill, isImpostorKill ? FinalStatus.SheriffHauntedWolfKill : statusMisFire); // インポスター、狼付きは設定がimp設定が有効な時切れる
        if (target.IsMadRoles() || target.IsRole(RoleId.MadKiller) || target.IsRole(RoleId.Dependents)) return (isMadRolesKill, isImpostorKill ? statusSuccess : statusMisFire);
        if (target.IsNeutral()) return (isNeutralKill, isNeutralKill ? statusSuccess : statusMisFire);
        if (target.IsFriendRoles()) return (isFriendRolesKill, isFriendRolesKill ? statusSuccess : statusMisFire);
        if (target.IsLovers()) return (isLoversKill, isLoversKill ? statusSuccess : statusMisFire);
        if (target.IsQuarreled()) return (isQuarreledKill, isQuarreledKill ? statusSuccess : statusMisFire);
        if (isHauntedWolfDecision) return (true, statusSuccess); // シェリフの判定が反転している場合の デフォルト処理
        return (false, statusMisFire);
    }

    public static bool IsSheriff(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Sheriff) || Player.IsRole(RoleId.RemoteSheriff);
    }
    public static bool IsSheriffButton(PlayerControl Player)
    {
        if (Player.IsRole(RoleId.Sheriff))
        {
            if (RoleClass.Sheriff.KillMaxCount > 0)
            {
                return true;
            }
        }
        else if (Player.IsRole(RoleId.RemoteSheriff))
        {
            if (RoleClass.RemoteSheriff.KillMaxCount > 0)
            {
                return true;
            }
        }
        return false;
    }
    public static void EndMeeting()
    {
        ResetKillCooldown();
    }
}