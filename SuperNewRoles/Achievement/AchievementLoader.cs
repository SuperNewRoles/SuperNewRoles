namespace SuperNewRoles.Achievement
{
    public static class AchievementLoader
    {
        public static void OnLoad()
        {
            ObjectCreate();
        }
        public static void ObjectCreate()
        {
            AchievementManager.AllAchievementData = new();
            new AchievementData(0, "BestFalseChargesExiled");
            new AchievementData(1, "BestFalseChargesGuardExiled");
            new AchievementData(2, "SheriffKill");
            new AchievementData(3, "ChiefCreateSheriff");
            new AchievementData(4, "ByChiefCreateSheriff");
            new AchievementData(5, "ByChiefCreateSheriffNeutral");
        }
    }
}
