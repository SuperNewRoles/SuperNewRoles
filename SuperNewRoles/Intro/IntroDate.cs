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
        public string Name;
        public Int16 TitleNum;
        public string TitleDesc;
        public Color color;
        public CustomRPC.RoleId RoleId;
        public string Description;
        public TeamRoleType Team;
        IntroDate(string NameKey, Color color , Int16 TitleNum ,CustomRPC.RoleId RoleId,TeamRoleType team = TeamRoleType.Crewmate)
        {
            this.color = color;
            this.NameKey = NameKey;
            this.Name = ModTranslation.getString(NameKey+"Name");
            this.RoleId = RoleId;
            this.TitleNum = TitleNum;
            this.TitleDesc = Intro.IntroDate.GetTitle(NameKey, TitleNum);
            this.Description = ModTranslation.getString(NameKey+"Description");
            this.Team = team;
        }
        public static IntroDate GetIntroDate(CustomRPC.RoleId RoleId,PlayerControl p = null)
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
                case (CustomRPC.RoleId.Jackal):
                    return JackalIntro;
                case (CustomRPC.RoleId.Sidekick):
                    return SidekickIntro;
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
                case (CustomRPC.RoleId.Bestfalsecharge):
                    return BestfalsechargeIntro;
                case (CustomRPC.RoleId.Researcher):
                    return ResearcherIntro;
                case (CustomRPC.RoleId.SelfBomber):
                    return SelfBomberIntro;
                case (CustomRPC.RoleId.God):
                    return GodIntro;
                case (CustomRPC.RoleId.AllCleaner):
                    return AllCleanerIntro;
                case (CustomRPC.RoleId.NiceNekomata):
                    return NiceNekomataIntro;
                case (CustomRPC.RoleId.EvilNekomata):
                    return EvilNekomataIntro;
                case (CustomRPC.RoleId.JackalFriends):
                    return JackalFriendsIntro;
                case (CustomRPC.RoleId.Doctor):
                    return DoctorIntro;
                case (CustomRPC.RoleId.CountChanger):
                    return CountChangerIntro;
                case (CustomRPC.RoleId.Pursuer):
                    return PursuerIntro;
                case (CustomRPC.RoleId.Minimalist):
                    return MinimalistIntro;
                case (CustomRPC.RoleId.Hawk):
                    return HawkIntro;
                case (CustomRPC.RoleId.Egoist):
                    return EgoistIntro;
                case (CustomRPC.RoleId.NiceRedRidingHood):
                    return NiceRedRidingHoodIntro;
                case (CustomRPC.RoleId.EvilEraser):
                    return EvilEraserIntro;
                case (CustomRPC.RoleId.Workperson):
                    return WorkpersonIntro;
                case (CustomRPC.RoleId.Magaziner):
                    return MagazinerIntro;
                case (CustomRPC.RoleId.Mayor):
                    return MayorIntro;
                case (CustomRPC.RoleId.truelover):
                    return trueloverIntro;
                case (CustomRPC.RoleId.Technician):
                    return TechnicianIntro;
                case (CustomRPC.RoleId.SerialKiller):
                    return SerialKillerIntro;
                //イントロ検知
                case (CustomRPC.RoleId.DefaultRole):
                    if (p != null && p.Data.Role.IsImpostor) {
                        return ImpostorIntro;
                    } else
                    {
                        return CrewmateIntro;
                    }
                    
            }

            return SheriffIntro;
        }
        public static string GetTitle(string name,Int16 num)
        {
            System.Random r1 = new System.Random();
            return ModTranslation.getString(name + "Title" + r1.Next(1, num + 1).ToString());
        }
        public static IntroDate CrewmateIntro = new IntroDate("CrewMate", Color.white, 1, CustomRPC.RoleId.DefaultRole);
        public static IntroDate ImpostorIntro = new IntroDate("Impostor", RoleClass.ImpostorRed, 1, CustomRPC.RoleId.DefaultRole,TeamRoleType.Impostor);
        public static IntroDate SoothSayerIntro = new IntroDate("SoothSayer", RoleClass.SoothSayer.color, 1, CustomRPC.RoleId.SoothSayer);
        public static IntroDate JesterIntro = new IntroDate("Jester", RoleClass.Jester.color, 1, CustomRPC.RoleId.Jester, TeamRoleType.Neutral);
        public static IntroDate LighterIntro = new IntroDate("Lighter",RoleClass.Lighter.color,1,CustomRPC.RoleId.Lighter);
        public static IntroDate EvilLighterIntro = new IntroDate("EvilLighter",RoleClass.EvilLighter.color,2,CustomRPC.RoleId.EvilLighter, TeamRoleType.Impostor);
        public static IntroDate EvilScientist = new IntroDate("EvilScientist",RoleClass.EvilScientist.color,2,CustomRPC.RoleId.EvilScientist, TeamRoleType.Impostor);
        public static IntroDate SheriffIntro = new IntroDate("Sheriff", RoleClass.Sheriff.color, 2, CustomRPC.RoleId.Sheriff);
        public static IntroDate MeetingSheriffIntro = new IntroDate("MeetingSheriff",RoleClass.MeetingSheriff.color,4,CustomRPC.RoleId.MeetingSheriff);
        public static IntroDate JackalIntro = new IntroDate("Jackal",RoleClass.Jackal.color,3,CustomRPC.RoleId.Jackal, TeamRoleType.Neutral);
        public static IntroDate SidekickIntro = new IntroDate("Sidekick", RoleClass.Jackal.color, 1, CustomRPC.RoleId.Sidekick, TeamRoleType.Neutral);
        public static IntroDate TeleporterIntro = new IntroDate("Teleporter",RoleClass.Teleporter.color,2,CustomRPC.RoleId.Teleporter, TeamRoleType.Impostor);
        public static IntroDate SpiritMediumIntro = new IntroDate("SpiritMedium",RoleClass.SpiritMedium.color,1,CustomRPC.RoleId.SpiritMedium);
        public static IntroDate SpeedBoosterIntro = new IntroDate("SpeedBooster",RoleClass.SpeedBooster.color,2,CustomRPC.RoleId.SpeedBooster);
        public static IntroDate EvilSpeedBoosterIntro = new IntroDate("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 4, CustomRPC.RoleId.EvilSpeedBooster, TeamRoleType.Impostor);
        public static IntroDate TaskerIntro = new IntroDate("Tasker", RoleClass.Tasker.color, 2, CustomRPC.RoleId.Tasker, TeamRoleType.Impostor);
        public static IntroDate DoorrIntro = new IntroDate("Doorr",RoleClass.Doorr.color,2,CustomRPC.RoleId.Doorr);
        public static IntroDate EvilDoorrIntro = new IntroDate("EvilDoorr", RoleClass.EvilDoorr.color, 3, CustomRPC.RoleId.EvilDoorr, TeamRoleType.Impostor);
        public static IntroDate SealdorIntro = new IntroDate("Sealdor",RoleClass.Sealdor.color,3,CustomRPC.RoleId.Sealdor);
        public static IntroDate FreezerIntro = new IntroDate("Freezer", RoleClass.Freezer.color, 3, CustomRPC.RoleId.Freezer, TeamRoleType.Impostor);
        public static IntroDate SpeederIntro = new IntroDate("Speeder", RoleClass.Speeder.color, 2, CustomRPC.RoleId.Speeder, TeamRoleType.Impostor);
        public static IntroDate GuesserIntro = new IntroDate("Guesser", RoleClass.Guesser.color, 2, CustomRPC.RoleId.Guesser);
        public static IntroDate EvilGuesserIntro = new IntroDate("EvilGuesser", RoleClass.EvilGuesser.color, 1, CustomRPC.RoleId.EvilGuesser, TeamRoleType.Impostor);
        public static IntroDate VultureIntro = new IntroDate("Vulture", RoleClass.Vulture.color, 1, CustomRPC.RoleId.Vulture, TeamRoleType.Neutral);
        public static IntroDate NiceScientistIntro = new IntroDate("NiceScientist",RoleClass.NiceScientist.color,2,CustomRPC.RoleId.NiceScientist);
        public static IntroDate ClergymanIntro = new IntroDate("Clergyman", RoleClass.Clergyman.color, 2, CustomRPC.RoleId.Clergyman);
        public static IntroDate MadMateIntro = new IntroDate("MadMate", RoleClass.MadMate.color, 1, CustomRPC.RoleId.MadMate, TeamRoleType.Impostor);
        public static IntroDate BaitIntro = new IntroDate("Bait", RoleClass.Bait.color, 1, CustomRPC.RoleId.Bait);
        public static IntroDate HomeSecurityGuardIntro = new IntroDate("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, CustomRPC.RoleId.HomeSecurityGuard);
        public static IntroDate StuntManIntro = new IntroDate("StuntMan", RoleClass.StuntMan.color, 1, CustomRPC.RoleId.StuntMan);
        public static IntroDate MovingIntro = new IntroDate("Moving", RoleClass.Moving.color, 1, CustomRPC.RoleId.Moving);
        public static IntroDate OpportunistIntro = new IntroDate("Opportunist",RoleClass.Opportunist.color,2,CustomRPC.RoleId.Opportunist, TeamRoleType.Neutral);
        public static IntroDate NiceGamblerIntro = new IntroDate("NiceGambler", RoleClass.NiceGambler.color, 1, CustomRPC.RoleId.NiceGambler);
        public static IntroDate EvilGamblerIntro = new IntroDate("EvilGambler", RoleClass.EvilGambler.color, 1, CustomRPC.RoleId.EvilGambler, TeamRoleType.Impostor);
        public static IntroDate BestfalsechargeIntro = new IntroDate("Bestfalsecharge", RoleClass.Bestfalsecharge.color, 1, CustomRPC.RoleId.Bestfalsecharge);
        public static IntroDate ResearcherIntro = new IntroDate("Researcher", RoleClass.Researcher.color, 1, CustomRPC.RoleId.Researcher, TeamRoleType.Neutral);
        public static IntroDate SelfBomberIntro = new IntroDate("SelfBomber", RoleClass.SelfBomber.color, 1, CustomRPC.RoleId.SelfBomber, TeamRoleType.Impostor);
        public static IntroDate GodIntro = new IntroDate("God", RoleClass.God.color, 1, CustomRPC.RoleId.God, TeamRoleType.Neutral);
        public static IntroDate AllCleanerIntro = new IntroDate("AllCleaner", RoleClass.AllCleaner.color, 1, CustomRPC.RoleId.AllCleaner, TeamRoleType.Impostor);
        public static IntroDate NiceNekomataIntro = new IntroDate("NiceNekomata", RoleClass.NiceNekomata.color, 3, CustomRPC.RoleId.NiceNekomata);
        public static IntroDate EvilNekomataIntro = new IntroDate("EvilNekomata", RoleClass.EvilNekomata.color, 1, CustomRPC.RoleId.EvilNekomata, TeamRoleType.Impostor);
        public static IntroDate JackalFriendsIntro = new IntroDate("JackalFriends", RoleClass.JackalFriends.color, 2, CustomRPC.RoleId.JackalFriends, TeamRoleType.Neutral);
        public static IntroDate DoctorIntro = new IntroDate("Doctor", RoleClass.Doctor.color, 1, CustomRPC.RoleId.Doctor);
        public static IntroDate CountChangerIntro = new IntroDate("CountChanger", RoleClass.CountChanger.color, 2, CustomRPC.RoleId.CountChanger, TeamRoleType.Impostor);
        public static IntroDate PursuerIntro = new IntroDate("Pursuer", RoleClass.Pursuer.color, 3, CustomRPC.RoleId.Pursuer, TeamRoleType.Impostor);
        public static IntroDate MinimalistIntro = new IntroDate("Minimalist", RoleClass.Minimalist.color, 2, CustomRPC.RoleId.Minimalist, TeamRoleType.Impostor);
        public static IntroDate HawkIntro = new IntroDate("Hawk", RoleClass.Hawk.color, 1, CustomRPC.RoleId.Hawk, TeamRoleType.Impostor);
        public static IntroDate EgoistIntro = new IntroDate("Egoist", RoleClass.Egoist.color, 1, CustomRPC.RoleId.Egoist, TeamRoleType.Neutral);
        public static IntroDate NiceRedRidingHoodIntro = new IntroDate("NiceRedRidingHood", RoleClass.NiceRedRidingHood.color, 1, CustomRPC.RoleId.NiceRedRidingHood);
        public static IntroDate EvilEraserIntro = new IntroDate("EvilEraser", RoleClass.EvilEraser.color, 1, CustomRPC.RoleId.EvilEraser, TeamRoleType.Impostor);
        public static IntroDate WorkpersonIntro = new IntroDate("Workperson", RoleClass.Workperson.color, 1, CustomRPC.RoleId.Workperson, TeamRoleType.Neutral);
        public static IntroDate MagazinerIntro = new IntroDate("Magaziner", RoleClass.Magaziner.color, 1, CustomRPC.RoleId.Magaziner, TeamRoleType.Impostor);
        public static IntroDate MayorIntro = new IntroDate("Mayor", RoleClass.Mayor.color, 1, CustomRPC.RoleId.Mayor);
        public static IntroDate trueloverIntro = new IntroDate("truelover", RoleClass.truelover.color, 1, CustomRPC.RoleId.truelover, TeamRoleType.Neutral);
        public static IntroDate TechnicianIntro = new IntroDate("Technician", RoleClass.Technician.color, 1, CustomRPC.RoleId.Technician);
        public static IntroDate SerialKillerIntro = new IntroDate("SerialKiller", RoleClass.SerialKiller.color, 1, CustomRPC.RoleId.SerialKiller);
        //イントロオブジェ
    }
}
