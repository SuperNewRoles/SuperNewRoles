using HarmonyLib;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Intro
{
    public class IntroDate
    {
        public string NameKey;
        public Int16 TitleNum;
        public string TitleDesc;
        public Color color;
        public CustomRPC.RoleId RoleId;

        IntroDate(string NameKey, Color color , Int16 TitleNum ,CustomRPC.RoleId RoleId)
        {
            this.color = color;
            this.NameKey = NameKey;
            this.RoleId = RoleId;
            this.TitleNum = TitleNum;
            this.TitleDesc = Intro.IntroDate.GetTitle(NameKey, TitleNum);
        }
        public static IntroDate GetIntroDate(CustomRPC.RoleId RoleId)
        {
            switch (RoleId) {
                case (CustomRPC.RoleId.SoothSayer):
                    return SoothSayerIntro;
                case (CustomRPC.RoleId.Jester):
                    return JesterIntro;
                case (CustomRPC.RoleId.Lighter):
                    return LighterIntro;
                case (CustomRPC.RoleId.EvilLighter):
                    return EvilLighterIntro;
                case (CustomRPC.RoleId.EvilScientist):
                    return EvilScientist;
                case (CustomRPC.RoleId.Sheriff):
                    return SheriffIntro;
                case (CustomRPC.RoleId.MeetingSheriff):
                    return MeetingSheriffIntro;
                case (CustomRPC.RoleId.AllKiller):
                    return AllKillerIntro;
                case (CustomRPC.RoleId.Teleporter):
                    return TeleporterIntro;
                case (CustomRPC.RoleId.SpiritMedium):
                    return SpiritMediumIntro;
                case (CustomRPC.RoleId.SpeedBooster):
                    return SpeedBoosterIntro;
                case (CustomRPC.RoleId.EvilSpeedBooster):
                    return EvilSpeedBoosterIntro;
                case (CustomRPC.RoleId.Tasker):
                    return TaskerIntro;
                case (CustomRPC.RoleId.Doorr):
                    return DoorrIntro;
                case (CustomRPC.RoleId.EvilDoorr):
                    return EvilDoorrIntro;
                case (CustomRPC.RoleId.Sealdor):
                    return SealdorIntro;
                case (CustomRPC.RoleId.Clergyman):
                    return ClergymanIntro;
                case (CustomRPC.RoleId.MadMate):
                    return MadMateIntro;
                case (CustomRPC.RoleId.Bait):
                    return BaitIntro;
                case (CustomRPC.RoleId.HomeSecurityGuard):
                    return HomeSecurityGuardIntro;
                case (CustomRPC.RoleId.StuntMan):
                    return StuntManIntro;
                case (CustomRPC.RoleId.Moving):
                    return MovingIntro;
                case (CustomRPC.RoleId.Opportunist):
                    return OpportunistIntro;
                case (CustomRPC.RoleId.NiceGambler):
                    return NiceGamblerIntro;
                case (CustomRPC.RoleId.EvilGambler):
                    return EvilGamblerIntro;
            }

            return SheriffIntro;
        }
        public static string GetTitle(string name,Int16 num)
        {
            System.Random r1 = new System.Random();
            return ModTranslation.getString(name + "Title" + r1.Next(1, num + 1).ToString());
        }
        public static IntroDate SoothSayerIntro = new IntroDate("SoothSayer", RoleClass.SoothSayer.color, 1, CustomRPC.RoleId.SoothSayer);
        public static IntroDate JesterIntro = new IntroDate("Jester", RoleClass.Jester.color, 1, CustomRPC.RoleId.Jester);
        public static IntroDate LighterIntro = new IntroDate("Lighter",RoleClass.Lighter.color,1,CustomRPC.RoleId.Lighter);
        public static IntroDate EvilLighterIntro = new IntroDate("EvilLighter",RoleClass.EvilLighter.color,2,CustomRPC.RoleId.EvilLighter);
        public static IntroDate EvilScientist = new IntroDate("EvilScientist",RoleClass.EvilScientist.color,2,CustomRPC.RoleId.EvilScientist);
        public static IntroDate SheriffIntro = new IntroDate("Sheriff", RoleClass.Sheriff.color, 2, CustomRPC.RoleId.Sheriff);
        public static IntroDate MeetingSheriffIntro = new IntroDate("MeetingSheriff",RoleClass.MeetingSheriff.color,4,CustomRPC.RoleId.MeetingSheriff);
        public static IntroDate AllKillerIntro = new IntroDate("AllKiller",RoleClass.AllKiller.color,2,CustomRPC.RoleId.AllKiller);
        public static IntroDate TeleporterIntro = new IntroDate("Teleporter",RoleClass.Teleporter.color,2,CustomRPC.RoleId.Teleporter);
        public static IntroDate SpiritMediumIntro = new IntroDate("SpiritMedium",RoleClass.SpiritMedium.color,1,CustomRPC.RoleId.SpiritMedium);
        public static IntroDate SpeedBoosterIntro = new IntroDate("SpeedBooster",RoleClass.SpeedBooster.color,2,CustomRPC.RoleId.SpeedBooster);
        public static IntroDate EvilSpeedBoosterIntro = new IntroDate("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 3, CustomRPC.RoleId.EvilSpeedBooster);
        public static IntroDate TaskerIntro = new IntroDate("Tasker", RoleClass.Tasker.color, 2, CustomRPC.RoleId.Tasker);
        public static IntroDate DoorrIntro = new IntroDate("Doorr",RoleClass.Doorr.color,2,CustomRPC.RoleId.Doorr);
        public static IntroDate EvilDoorrIntro = new IntroDate("EvilDoorr", RoleClass.EvilDoorr.color, 3, CustomRPC.RoleId.EvilDoorr);
        public static IntroDate SealdorIntro = new IntroDate("Sealdor",RoleClass.Sealdor.color,3,CustomRPC.RoleId.Sealdor);
        public static IntroDate FreezerIntro = new IntroDate("Freezer", RoleClass.Freezer.color, 2, CustomRPC.RoleId.Freezer);
        public static IntroDate SpeederIntro = new IntroDate("Speeder", RoleClass.Speeder.color, 2, CustomRPC.RoleId.Speeder);
        public static IntroDate GuesserIntro = new IntroDate("Guesser", RoleClass.Guesser.color, 2, CustomRPC.RoleId.Guesser);
        public static IntroDate EvilGuesserIntro = new IntroDate("EvilGuesser", RoleClass.EvilGuesser.color, 1, CustomRPC.RoleId.EvilGuesser);
        public static IntroDate VultureIntro = new IntroDate("Vulture", RoleClass.Vulture.color, 1, CustomRPC.RoleId.Vulture);
        public static IntroDate NiceScientistIntro = new IntroDate("NiceScientist",RoleClass.NiceScientist.color,2,CustomRPC.RoleId.NiceScientist);
        public static IntroDate ClergymanIntro = new IntroDate("Clergyman", RoleClass.Clergyman.color, 2, CustomRPC.RoleId.Clergyman);
        public static IntroDate MadMateIntro = new IntroDate("MadMate", RoleClass.MadMate.color, 1, CustomRPC.RoleId.MadMate);
        public static IntroDate BaitIntro = new IntroDate("Bait", RoleClass.Bait.color, 1, CustomRPC.RoleId.Bait);
        public static IntroDate HomeSecurityGuardIntro = new IntroDate("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, CustomRPC.RoleId.HomeSecurityGuard);
        public static IntroDate StuntManIntro = new IntroDate("StuntMan", RoleClass.StuntMan.color, 1, CustomRPC.RoleId.StuntMan);
        public static IntroDate MovingIntro = new IntroDate("Moving", RoleClass.Moving.color, 1, CustomRPC.RoleId.Moving);
        public static IntroDate OpportunistIntro = new IntroDate("Opportunist",RoleClass.Opportunist.color,2,CustomRPC.RoleId.Opportunist);
        public static IntroDate NiceGamblerIntro = new IntroDate("NiceGambler", RoleClass.NiceGambler.color, 1, CustomRPC.RoleId.NiceGambler);
        public static IntroDate EvilGamblerIntro = new IntroDate("EvilGambler", RoleClass.EvilGambler.color, 1, CustomRPC.RoleId.EvilGambler);
    }
}
