using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class Smasher
    {
        public static void resetCoolDown()
        {
            HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer = RoleClass.Smasher.KillCoolTime;
            HudManagerStartPatch.DoubleKillerMainKillButton.Timer = RoleClass.Smasher.KillCoolTime;
        }
        public static void resetSmashCoolDown()
        {
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = 0.1f;
            HudManagerStartPatch.DoubleKillerSubKillButton.Timer = 0.1f;
        }
        public static void EndMeeting()
        {
            resetCoolDown();
            resetSmashCoolDown();
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = 0.1f;
        }
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
    }
}
