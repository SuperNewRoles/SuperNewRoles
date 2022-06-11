using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;

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
            if (Target.isRole(CustomRPC.RoleId.MadMate) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMate) && RoleClass.Sheriff.MadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadJester) && RoleClass.Sheriff.MadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.Sheriff.IsNeutralKill) return true;
            if (RoleClass.Sheriff.IsLoversKill && Target.IsLovers()) return true;
            if (Target.isRole(CustomRPC.RoleId.MadStuntMan) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMayor) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadHawk) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadSeer) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMaker) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.JackalFriends) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.SeerFriends) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MayorFriends) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.HauntedWolf)) return true;
            //シェリフキルゥ
            return false;
        }
        public static bool IsRemoteSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            if (roledata == TeamRoleType.Impostor) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMate) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMate) && RoleClass.RemoteSheriff.MadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadJester) && RoleClass.RemoteSheriff.MadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.RemoteSheriff.IsNeutralKill) return true;
            if (RoleClass.RemoteSheriff.IsLoversKill && Target.IsLovers()) return true;
            if (Target.isRole(CustomRPC.RoleId.MadStuntMan) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMayor) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadHawk) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadSeer) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MadMaker) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.JackalFriends) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.SeerFriends) && RoleClass.Sheriff.IsMadRoleKill) return true;
            if (Target.isRole(CustomRPC.RoleId.MayorFriends) && RoleClass.RemoteSheriff.IsMadRoleKill) return true;
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