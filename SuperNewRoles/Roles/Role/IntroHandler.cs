

namespace SuperNewRoles.Roles
{
    class IntroHandler
    {
        public static void Handler()
        {
            float time = 2f;
            if (PlayerControl.GameOptions.KillCooldown >= 10)
            {
                time = 7f;
            }
            new LateTask(() =>
            {
                RoleClass.IsStart = true;
            }, time, "IsStartOn");
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Pursuer))
            {
                RoleClass.Pursuer.arrow.arrow.SetActive(true);
            }
            if (Mode.ModeHandler.IsMode(Mode.ModeId.Zombie)) Mode.Zombie.Main.SetTimer();
            if (Mode.ModeHandler.IsMode(Mode.ModeId.BattleRoyal)) Mode.BattleRoyal.Main.IsCountOK = true;
            if (Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles)) Mode.SuperHostRoles.FixedUpdate.SetRoleNames();
            if (Mode.ModeHandler.IsMode(Mode.ModeId.CopsRobbers)) Mode.CopsRobbers.Main.IsStart = true;
        }
    }
}