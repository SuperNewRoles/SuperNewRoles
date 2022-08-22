using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.CustomOption;

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
            if (Target.IsMadRoles() && RoleClass.Sheriff.IsMadRoleKill && !CustomOptions.SheriffMadRoleKillIndividualSettings.GetBool()) return true;//マッドを切れるが有効 かつ 個別設定が無効
            if (RoleClass.Sheriff.IsMadRoleKill && CustomOptions.SheriffMadRoleKillIndividualSettings.GetBool())//マッドを切れるが有効 かつ 個別設定が有効
            {
                return role switch
                {
                    RoleId.MadMate => CustomOptions.SheriffCanKillMadMate.GetBool(),
                    RoleId.MadMayor => CustomOptions.SheriffCanKillMadMayor.GetBool(),
                    RoleId.MadStuntMan => CustomOptions.SheriffCanKillMadStuntMan.GetBool(),
                    RoleId.MadHawk => CustomOptions.SheriffCanKillMadHawk.GetBool(),
                    RoleId.MadJester => CustomOptions.SheriffCanKillMadJester.GetBool(),
                    RoleId.MadSeer => CustomOptions.SheriffCanKillMadSeer.GetBool(),
                    RoleId.BlackCat => CustomOptions.SheriffCanKillBlackCat.GetBool(),
                    RoleId.MadMaker => CustomOptions.SheriffCanKillMadMaker.GetBool(),
                    //シェリフがマッドを切れる
                    _ => false,
                };
            }
            if (Target.IsFriendRoles() && RoleClass.Sheriff.IsFriendsRoleKill && !CustomOptions.SheriffFriendsRoleKillIndividualSettings.GetBool()) return true;//フレンズを切れるが有効 かつ 個別設定が無効
            if (RoleClass.Sheriff.IsFriendsRoleKill && CustomOptions.SheriffFriendsRoleKillIndividualSettings.GetBool())//フレンズを切れるが有効 かつ 個別設定が有効
            {
                return role switch
                {
                    RoleId.JackalFriends => CustomOptions.SheriffCanKillJackalFriends.GetBool(),
                    RoleId.SeerFriends => CustomOptions.SheriffCanKillSeerFriends.GetBool(),
                    RoleId.MayorFriends => CustomOptions.SheriffCanKillMayorFriends.GetBool(),
                    //シェリフがフレンズを切れる
                    _ => false,
                };
            }
            if (Target.IsNeutral() && RoleClass.Sheriff.IsNeutralKill && !CustomOptions.SheriffNeutralKillIndividualSettings.GetBool()) return true;//第三陣営を切れるが有効 かつ 個別設定が無効
            if (RoleClass.Sheriff.IsNeutralKill && CustomOptions.SheriffNeutralKillIndividualSettings.GetBool())//第三陣営を切れるが有効 かつ 個別設定が有効
            {
                return role switch
                {
                    RoleId.Jester => CustomOptions.SheriffCanKillJester.GetBool(),
                    RoleId.Jackal => CustomOptions.SheriffCanKillJackal.GetBool(),
                    RoleId.Sidekick => CustomOptions.SheriffCanKillSidekick.GetBool(),
                    RoleId.Vulture => CustomOptions.SheriffCanKillVulture.GetBool(),
                    RoleId.Opportunist => CustomOptions.SheriffCanKillOpportunist.GetBool(),
                    //RoleId.Researcher => CustomOptions.SheriffCanKillResearcher.GetBool(),
                    RoleId.God => CustomOptions.SheriffCanKillGod.GetBool(),
                    RoleId.Egoist => CustomOptions.SheriffCanKillEgoist.GetBool(),
                    RoleId.Workperson => CustomOptions.SheriffCanKillWorkperson.GetBool(),
                    RoleId.truelover => CustomOptions.SheriffCanKilltruelover.GetBool(),
                    RoleId.Amnesiac => CustomOptions.SheriffCanKillAmnesiac.GetBool(),
                    RoleId.FalseCharges => CustomOptions.SheriffCanKillFalseCharges.GetBool(),
                    RoleId.Fox => CustomOptions.SheriffCanKillFox.GetBool(),
                    RoleId.TeleportingJackal => CustomOptions.SheriffCanKillTeleportingJackal.GetBool(),
                    RoleId.Demon => CustomOptions.SheriffCanKillDemon.GetBool(),
                    RoleId.JackalSeer => CustomOptions.SheriffCanKillJackalSeer.GetBool(),
                    RoleId.SidekickSeer => CustomOptions.SheriffCanKillSidekickSeer.GetBool(),
                    RoleId.Arsonist => CustomOptions.SheriffCanKillArsonist.GetBool(),
                    RoleId.MayorFriends => CustomOptions.SheriffCanKillMayorFriends.GetBool(),
                    RoleId.Tuna => CustomOptions.SheriffCanKillTuna.GetBool(),
                    RoleId.Neet => CustomOptions.SheriffCanKillNeet.GetBool(),
                    RoleId.Revolutionist => CustomOptions.SheriffCanKillRevolutionist.GetBool(),
                    RoleId.Stefinder => CustomOptions.SheriffCanKillStefinder.GetBool(),
                    //シェリフが第3陣営を切れる
                    _ => false,
                };
            }
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