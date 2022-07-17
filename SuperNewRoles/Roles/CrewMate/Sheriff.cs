using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Sheriff
    {
        public static void ResetKillCoolDown()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff))
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
            if (roledata == TeamRoleType.Impostor) return true;
            if (Target.isMadRole() && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isFriendRole() && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.Sheriff.IsNeutralKill) return true;
            return RoleClass.Sheriff.IsLoversKill && Target.IsLovers() ? true : Target.isRole(RoleId.HauntedWolf);
        }
        public static bool IsChiefSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            if (roledata == TeamRoleType.Impostor) return true;
            if (Target.isMadRole() && RoleClass.Chief.IsMadRoleKill) return true;
            if (Target.isFriendRole() && RoleClass.Chief.IsMadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.Chief.IsNeutralKill) return true;
            return RoleClass.Chief.IsLoversKill && Target.IsLovers() ? true : Target.isRole(RoleId.HauntedWolf);
        }
        public static bool IsRemoteSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            if (roledata == TeamRoleType.Impostor) return true;
            if (Target.isMadRole() && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isFriendRole() && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.RemoteSheriff.IsNeutralKill) return true;
            return RoleClass.RemoteSheriff.IsLoversKill && Target.IsLovers() ? true : Target.isRole(RoleId.HauntedWolf);
        }
        public static bool IsSheriff(PlayerControl Player)
        {
            return Player.isRole(RoleId.Sheriff) || Player.isRole(RoleId.RemoteSheriff);
        }
        public static bool IsSheriffButton(PlayerControl Player)
        {
            if (Player.isRole(RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillMaxCount > 0)
                {
                    return true;
                }
            }
            else if (Player.isRole(RoleId.RemoteSheriff))
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