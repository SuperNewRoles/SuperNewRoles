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
        ByChiefCreateSheriffNeutral = 6,

        ConcentratedEnergy = 7,
        PersonalCombat = 8,
        ImNotReady = 9,
        AlwaysLoveYou = 10,
        ParfectCrewGame = 11,
        BestHitman = 12,
        NeutralSheriff = 13,
        Hitting = 14,
        ForbiddenLove = 15,
        LoveDeliver = 16,
        FruitfulLove = 17,
        HelloFromBelly = 18,
        ThisIsTechnology = 19,
        JobAccomplishment = 20,
        ThisIsBasement = 21,
        ThankyouBasement = 22,
        ItsMagic = 23,
        LetsBattle = 24,
        ThisIsVictory = 25,
        ILoveToilet = 26,
        SoldOut = 27,
        HowItWorks = 28,
        InJeopardy = 29,
        TimeForRevolution = 30,
        TooManyKills = 31,
        Bomb = 32,
        NiceBomb = 33,
        GotItSideKiller = 34,
        ComeHere = 35,
        YoImpostor = 36,
        EvenWithoutTheseThings = 37,
        TheSameFace = 38,
        Preferably = 39,
        UsurpationOfVictory = 40,
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
            AchievementManagerSNR.AllAchievementData.Add(type, this);
            if (Complete)
                AchievementManagerSNR.CompletedAchievement.Add(this);
        }
    }
}
