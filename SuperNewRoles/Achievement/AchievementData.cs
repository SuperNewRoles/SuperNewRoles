namespace SuperNewRoles.Achievement
{
    //真ん中のやつを消すときは影響が出ないようにId指定してねー
    public enum AchievementType {
        BestFalseChargesExiled = 0,
        BestFalseChargesGuardExiled = 1,
        SheriffKill = 2,
        ChiefCreateSheriff = 3,
        ByChiefCreateSheriff = 4,
        ByChiefCreateSheriffNeutral = 5
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
            Complete = AchievementManager.currentData.Contains(Id+"\n");
            Logger.Info($"{Name}が生成されました。{Complete}です。{Title} : {Description}");
            AchievementManager.AllAchievementData.Add(this);
        }
    }
}
