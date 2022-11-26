namespace SuperNewRoles.Achievement
{
    //真ん中のやつを消すときは影響が出ないようにId指定してねー
    public enum AchievementType {
        StartSuperNewRoles = 0,
        BestFalseChargesExiled = 1,
        BestFalseChargesGuardExiled = 2,
        SheriffKill = 3,
        ChiefCreateSheriff = 4,
        ByChiefCreateSheriff = 5,
        ByChiefCreateSheriffNeutral = 6
    }
    public class AchievementData
    {
        public int Id; //Id
        public string NameKey; //名前
        public string Name; //名前
        public string Description; //説明
        public string Title; //称号
        public bool Complete; //クリアしてるか
        public AchievementType TypeData;
        public AchievementData(AchievementType type)
        {
            TypeData = type;
            Id = (int)type;
            NameKey = type.ToString();
            Name = ModTranslation.GetString($"{NameKey}Name");
            Description = ModTranslation.GetString($"{NameKey}Description");
            Title = ModTranslation.GetString($"{NameKey}Title");
            Complete = AchievementManagerSNR.currentData.Contains(Id+"\n");
            Logger.Info($"{Name}が生成されました。{Complete}です。{Title} : {Description}");
            AchievementManagerSNR.AllAchievementData.Add(this);
            if (Complete)
                AchievementManagerSNR.CompletedAchievement.Add(this);
        }
    }
}
