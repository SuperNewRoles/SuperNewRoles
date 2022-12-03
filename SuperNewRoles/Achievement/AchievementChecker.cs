using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Achievement
{
    public static class AchievementChecker
    {
        public static DateTime GameStartTime;
        public static void OnEndGameCheck(List<WinningPlayerData> winners,)
        {
            if (PlayerControl.LocalPlayer.IsAlive())
            {
                if (RoleClass.Bestfalsecharge.IsMyBestFalseCharge) CompleteAchievement(AchievementType.BestFalseChargesGuardExiled);
            }
        }
        public static void OnUpdate()
        {
            if (!AllAchievementData[AchievementType.PersonalCombat].Complete && (PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.IsJackalTeamJackal())) if (PlayerControl.AllPlayerControls.FindAll((Il2CppSystem.Predicate<PlayerControl>)(x => x.IsAlive() && (x.IsImpostor() || x.IsJackalTeamJackal()))).Count == 2) CompleteAchievement(AchievementType.PersonalCombat);
        }
    }
}
