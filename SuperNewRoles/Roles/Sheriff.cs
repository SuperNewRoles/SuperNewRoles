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
            HudManagerStartPatch.SheriffKillButton.Timer = RoleClass.Sheriff.CoolTime;
        }
        public static bool IsSheriffKill(PlayerControl Target)
        {
            if (Target.Data.Role.IsImpostor)
            {
                return true;
            }
            else
            {
                return false;
            }
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
    }
}
