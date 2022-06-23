using System;
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles
{
    class Sheriff
    {
        public static void ResetKillCoolDown()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.RemoteSheriff))
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.RemoteSheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = RoleClass.RemoteSheriff.CoolTime;
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            else
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Sheriff.CoolTime;
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            SuperNewRolesPlugin.Logger.LogInfo("リセット！！！");
        }
        public static bool IsSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            if (roledata == TeamRoleType.Impostor) return true;
            if (Target.isMadRole() && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isFriendRole() && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.Sheriff.IsNeutralKill) return true;
            if (RoleClass.Sheriff.IsLoversKill && Target.IsLovers()) return true;
            if (Target.isRole(CustomRPC.RoleId.HauntedWolf)) return true;
            //シェリフキルゥ
            return false;
        }
        public static bool IsRemoteSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            if (roledata == TeamRoleType.Impostor) return true;
            if (Target.isMadRole() && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isFriendRole() && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.RemoteSheriff.IsNeutralKill) return true;
            if (RoleClass.RemoteSheriff.IsLoversKill && Target.IsLovers()) return true;
            if (Target.isRole(CustomRPC.RoleId.HauntedWolf)) return true;
            //リモシェリフキルゥ
            return false;
        }
        public static bool IsSheriff(PlayerControl Player)
        {
            if (Player.isRole(CustomRPC.RoleId.Sheriff) || Player.isRole(CustomRPC.RoleId.RemoteSheriff))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsSheriffButton(PlayerControl Player)
        {
            if (Player.isRole(CustomRPC.RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillMaxCount > 0)
                {
                    return true;
                }
            }
            else if (Player.isRole(CustomRPC.RoleId.RemoteSheriff))
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
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.RemoteSheriff))
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.RemoteSheriff.CoolTime;
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            else
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Sheriff.CoolTime;
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
        }
    }
}
