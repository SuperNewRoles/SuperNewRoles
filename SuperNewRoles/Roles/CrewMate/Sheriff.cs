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
                HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Chief.SheriffPlayer.Contains(CachedPlayer.LocalPlayer.PlayerId)
                    ? RoleClass.Chief.CoolTime
                    : RoleClass.Sheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = HudManagerStartPatch.SheriffKillButton.MaxTimer;
            }
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