using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Lighter
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.CoolTime;
            RoleClass.Lighter.ButtonTimer = DateTime.Now;
        }
        public static bool isLighter(PlayerControl Player)
        {
            if (Player.isRole(RoleId.Lighter))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void LightOnStart()
        {
            RoleClass.Lighter.IsLightOn = true;
        }
        public static void LightOutEnd()
        {
            if (!RoleClass.Lighter.IsLightOn) return;
            RoleClass.Lighter.IsLightOn = false;
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.CoolTime;
            RoleClass.Lighter.ButtonTimer = DateTime.Now;
            RoleClass.Lighter.IsLightOn = false;
        }
    }
}
