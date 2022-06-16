using Hazel;
using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;

namespace SuperNewRoles.Roles
{
    class PositionSwapper
    {
        public static void ResetCoolDown(){
            HudManagerStartPatch.PositionSwapperButton.MaxTimer = RoleClass.PositionSwapper.CoolTime;
            RoleClass.PositionSwapper.ButtonTimer = DateTime.Now;
        }
        public static void EndMeeting(){
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Teleporter.CoolTime;
            RoleClass.Teleporter.ButtonTimer = DateTime.Now;
        }
        public static void SwapStart(){
            //
        }
    }
}