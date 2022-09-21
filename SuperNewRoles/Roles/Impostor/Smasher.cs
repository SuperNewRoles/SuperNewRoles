using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles
{
    public static class Smasher
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer = RoleClass.Smasher.KillCoolTime;
            HudManagerStartPatch.DoubleKillerMainKillButton.Timer = RoleClass.Smasher.KillCoolTime;
        }
        public static void ResetSmashCoolDown()
        {
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = 0.1f;
            HudManagerStartPatch.DoubleKillerSubKillButton.Timer = 0.1f;
        }
        public static void EndMeeting()
        {
            ResetCoolDown();
            ResetSmashCoolDown();
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = 0.1f;
        }
    }
}