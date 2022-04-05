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
            if (Target.isRole(CustomRPC.RoleId.MadMate) && RoleClass.Sheriff.IsMadMateKill) return true;
            if (Target.isNeutral() && RoleClass.Sheriff.IsNeutralKill) return true;
            if (RoleClass.Sheriff.IsLoversKill && Target.IsLovers()) return true;
            return false;
        }
        public static bool IsSheriff(PlayerControl Player)
        {
            if (RoleClass.Sheriff.SheriffPlayer.IsCheckListPlayerControl(Player))
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
