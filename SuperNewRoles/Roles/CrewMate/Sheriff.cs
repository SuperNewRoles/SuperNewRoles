using System;
using SuperNewRoles.Buttons;



namespace SuperNewRoles.Roles
{
    class Sheriff
    {
        public static void ResetKillCoolDown()
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
        public static bool IsSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            RoleId role = Target.GetRole();

            if ((roledata == TeamRoleType.Impostor) || Target.IsRole(RoleId.HauntedWolf)) return CustomOptions.SheriffCanKillImpostor.GetBool();//インポスター、狼付きは設定がimp設定が有効な時切れる
            if (RoleClass.Sheriff.IsLoversKill && Target.IsLovers()) return true;//ラバーズ
            if (CustomOptions.SheriffQuarreledKill.GetBool() && Target.IsQuarreled()) return true;//クラード
            if (RoleClass.Sheriff.IsMadRoleKill && Target.IsMadRoles()) return true;
            if (CustomOptions.SheriffFriendsRoleKill.GetBool() && Target.IsFriendRoles()) return true;
            if (RoleClass.Sheriff.IsNeutralKill && Target.IsNeutral()) return true;
            return false;
        }
        public static bool IsChiefSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            return (roledata == TeamRoleType.Impostor)
            || (Target.IsMadRoles() && RoleClass.Chief.IsMadRoleKill)
            || (Target.IsFriendRoles() && RoleClass.Chief.IsMadRoleKill)
            || (Target.IsNeutral() && RoleClass.Chief.IsNeutralKill)
                ? true
                : (RoleClass.Chief.IsLoversKill && Target.IsLovers()) || Target.IsRole(RoleId.HauntedWolf);
        }
        public static bool IsRemoteSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            return (roledata == TeamRoleType.Impostor)
            || (Target.IsMadRoles() && RoleClass.RemoteSheriff.IsMadRoleKill)
            || (Target.IsFriendRoles() && RoleClass.RemoteSheriff.IsMadRoleKill)
            || (Target.IsNeutral() && RoleClass.RemoteSheriff.IsNeutralKill)
                ? true
                : (RoleClass.RemoteSheriff.IsLoversKill && Target.IsLovers()) || Target.IsRole(RoleId.HauntedWolf);
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
            ResetKillCoolDown();
        }
    }
}