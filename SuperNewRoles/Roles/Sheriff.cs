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
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Sheriff.CoolTime;
            RoleClass.Sheriff.ButtonTimer = DateTime.Now;
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
            return false;
        }
        public static bool IsSheriff(PlayerControl Player)
        {
            if (Player.isRole(CustomRPC.RoleId.Sheriff))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Sheriff.CoolTime;
            RoleClass.Sheriff.ButtonTimer = DateTime.Now;
        }
    }
}
