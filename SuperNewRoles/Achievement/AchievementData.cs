namespace SuperNewRoles.Achievement
{
    public class AchievementData
    {
        public int Id; //Id
        public string NameKey; //名前
        public string Name; //名前
        public string Description; //説明
        public string Title; //称号
        public bool Complete; //クリアしてるか
        public AchievementData(int _id, string _nameKey)
        {
            Id = _id;
            NameKey = _nameKey;
            Name = ModTranslation.GetString($"{NameKey}Name");
            Description = ModTranslation.GetString($"{NameKey}Description");
            Title = ModTranslation.GetString($"{NameKey}Title");
            AchievementManager.AllAchievementData.Add(this);
        }
    }
}
