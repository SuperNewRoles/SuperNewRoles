using System;
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles;

class Sheriff
{
    public static void ResetKillCooldown()
    {
        try
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.RemoteSheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = RoleClass.RemoteSheriff.CoolTime;
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
    public static bool IsSheriffKill(PlayerControl Target)
    {
        var roledata = CountChanger.GetRoleType(Target);
        RoleId role = Target.GetRole();

        if ((roledata == TeamRoleType.Impostor) || Target.IsRole(RoleId.HauntedWolf)) return CustomOptionHolder.SheriffCanKillImpostor.GetBool();//インポスター、狼付きは設定がimp設定が有効な時切れる
        if (RoleClass.Sheriff.IsLoversKill && Target.IsLovers()) return true;//ラバーズ
        if (CustomOptionHolder.SheriffQuarreledKill.GetBool() && Target.IsQuarreled()) return true;//クラード
        if (RoleClass.Sheriff.IsMadRoleKill && (Target.IsMadRoles() || Target.IsRole(RoleId.MadKiller) || Target.IsRole(RoleId.Dependents))) return true;
        if (CustomOptionHolder.SheriffFriendsRoleKill.GetBool() && Target.IsFriendRoles()) return true;
        if (RoleClass.Sheriff.IsNeutralKill && Target.IsNeutral()) return true;
        return false;
    }
    public static bool IsChiefSheriffKill(PlayerControl Target)
    {
        var roledata = CountChanger.GetRoleType(Target);
        return (roledata == TeamRoleType.Impostor)
        || ((Target.IsMadRoles() || Target.IsRole(RoleId.MadKiller) || Target.IsRole(RoleId.Dependents)) && RoleClass.Chief.IsMadRoleKill)
        || (Target.IsFriendRoles() && RoleClass.Chief.IsMadRoleKill)
        || (Target.IsNeutral() && RoleClass.Chief.IsNeutralKill)
        || (RoleClass.Chief.IsLoversKill && Target.IsLovers()) || Target.IsRole(RoleId.HauntedWolf);
    }
    public static bool IsRemoteSheriffKill(PlayerControl Target)
    {
        var roledata = CountChanger.GetRoleType(Target);
        return (roledata == TeamRoleType.Impostor)
        || ((Target.IsMadRoles() || Target.IsRole(RoleId.MadKiller) || Target.IsRole(RoleId.Dependents)) && RoleClass.RemoteSheriff.IsMadRoleKill)
        || (Target.IsFriendRoles() && RoleClass.RemoteSheriff.IsMadRoleKill)
        || (Target.IsNeutral() && RoleClass.RemoteSheriff.IsNeutralKill)
        || (RoleClass.RemoteSheriff.IsLoversKill && Target.IsLovers())
        || Target.IsRole(RoleId.HauntedWolf);
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
