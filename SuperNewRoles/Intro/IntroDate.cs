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
        public static List<IntroDate> IntroDatas = new List<IntroDate>();
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
            IntroDatas.Add(this);
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
                case (CustomRPC.RoleId.NiceScientist):
                    return NiceScientistIntro;
                case (CustomRPC.RoleId.truelover):
                    return trueloverIntro;
                case (CustomRPC.RoleId.Technician):
                    return TechnicianIntro;
                case (CustomRPC.RoleId.SerialKiller):
                    return SerialKillerIntro;
                case (CustomRPC.RoleId.OverKiller):
                    return OverKillerIntro;
                case (CustomRPC.RoleId.Levelinger):
                    return LevelingerIntro;
                case (CustomRPC.RoleId.EvilMoving):
                    return EvilMovingIntro;
                case (CustomRPC.RoleId.Amnesiac):
                    return AmnesiacIntro;
                case (CustomRPC.RoleId.SideKiller):
                    return SideKillerIntro;
                case (CustomRPC.RoleId.MadKiller):
                    return MadKillerIntro;
                case (CustomRPC.RoleId.Survivor):
                    return SurvivorIntro;
                case (CustomRPC.RoleId.MadMayor):
                    return MadMayorIntro;
                case (CustomRPC.RoleId.NiceHawk):
                    return NiceHawkIntro;
                case (CustomRPC.RoleId.Bakery):
                    return BakeryIntro;
                case (CustomRPC.RoleId.MadStuntMan):
                    return MadStuntManIntro;
                case (CustomRPC.RoleId.MadHawk):
                    return MadHawkIntro;
                case (CustomRPC.RoleId.MadJester):
                    return MadJesterIntro;
                case (CustomRPC.RoleId.FalseCharges):
                    return FalseChargesIntro;
                case (CustomRPC.RoleId.NiceTeleporter):
                    return NiceTeleporterIntro;
                case (CustomRPC.RoleId.Celebrity):
                    return CelebrityIntro;
                case (CustomRPC.RoleId.Nocturnality):
                    return NocturnalityIntro;
                case (CustomRPC.RoleId.Observer):
                    return ObserverIntro;
                case (CustomRPC.RoleId.Vampire):
                    return VampireIntro;
                case (CustomRPC.RoleId.Fox):
                    return FoxIntro;
                case (CustomRPC.RoleId.DarkKiller):
                    return DarkKillerIntro;
                case (CustomRPC.RoleId.Seer):
                    return SeerIntro;
                case (CustomRPC.RoleId.MadSeer):
                    return MadSeerIntro;
                case (CustomRPC.RoleId.EvilSeer):
                    return EvilSeerIntro;
                case (CustomRPC.RoleId.RemoteSheriff):
                    return RemoteSheriffIntro;
                case (CustomRPC.RoleId.TeleportingJackal):
                    return TeleportingJackalIntro;
                case (CustomRPC.RoleId.MadMaker):
                    return MadMakerIntro;
                case (CustomRPC.RoleId.Demon):
                    return DemonIntro;
                case (CustomRPC.RoleId.TaskManager):
                    return TaskManagerIntro;
                case (CustomRPC.RoleId.SeerFriends):
                    return SeerFriendsIntro;
                case (CustomRPC.RoleId.JackalSeer):
                    return JackalSeerIntro;
                case (CustomRPC.RoleId.SidekickSeer):
                    return SidekickSeerIntro;
                case (CustomRPC.RoleId.Arsonist):
                    return ArsonistIntro;
                case (CustomRPC.RoleId.Speeder):
                    return SpeederIntro;
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
        public static IntroDate MadMateIntro = new IntroDate("MadMate", RoleClass.MadMate.color, 1, CustomRPC.RoleId.MadMate);
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
        public static IntroDate OverKillerIntro = new IntroDate("OverKiller", RoleClass.OverKiller.color, 1, CustomRPC.RoleId.OverKiller);
        public static IntroDate LevelingerIntro = new IntroDate("Levelinger", RoleClass.Levelinger.color, 1, CustomRPC.RoleId.Levelinger);
        public static IntroDate EvilMovingIntro = new IntroDate("EvilMoving", RoleClass.EvilMoving.color, 1, CustomRPC.RoleId.EvilMoving);
        public static IntroDate AmnesiacIntro = new IntroDate("Amnesiac", RoleClass.Amnesiac.color, 1, CustomRPC.RoleId.Amnesiac);
        public static IntroDate SideKillerIntro = new IntroDate("SideKiller", RoleClass.SideKiller.color, 1, CustomRPC.RoleId.SideKiller);
        public static IntroDate MadKillerIntro = new IntroDate("MadKiller", RoleClass.SideKiller.color, 1, CustomRPC.RoleId.MadKiller);
        public static IntroDate SurvivorIntro = new IntroDate("Survivor", RoleClass.Survivor.color, 1, CustomRPC.RoleId.Survivor);
        public static IntroDate MadMayorIntro = new IntroDate("MadMayor", RoleClass.MadMayor.color, 1, CustomRPC.RoleId.MadMayor);
        public static IntroDate NiceHawkIntro = new IntroDate("NiceHawk", RoleClass.NiceHawk.color, 2, CustomRPC.RoleId.NiceHawk);
        public static IntroDate BakeryIntro = new IntroDate("Bakery", RoleClass.Bakery.color, 1, CustomRPC.RoleId.Bakery);
        public static IntroDate MadStuntManIntro = new IntroDate("MadStuntMan", RoleClass.MadStuntMan.color, 1, CustomRPC.RoleId.MadStuntMan, TeamRoleType.Impostor);
        public static IntroDate MadHawkIntro = new IntroDate("MadHawk", RoleClass.MadHawk.color, 1, CustomRPC.RoleId.MadHawk);
        public static IntroDate MadJesterIntro = new IntroDate("MadJester", RoleClass.MadJester.color, 1, CustomRPC.RoleId.MadJester);
        public static IntroDate FalseChargesIntro = new IntroDate("FalseCharges", RoleClass.FalseCharges.color, 1, CustomRPC.RoleId.FalseCharges);
        public static IntroDate NiceTeleporterIntro = new IntroDate("NiceTeleporter", RoleClass.NiceTeleporter.color, 1, CustomRPC.RoleId.NiceTeleporter);
        public static IntroDate CelebrityIntro = new IntroDate("Celebrity", RoleClass.Celebrity.color, 1, CustomRPC.RoleId.Celebrity);
        public static IntroDate NocturnalityIntro = new IntroDate("Nocturnality", RoleClass.Nocturnality.color, 1, CustomRPC.RoleId.Nocturnality);
        public static IntroDate ObserverIntro = new IntroDate("Observer", RoleClass.Observer.color, 1, CustomRPC.RoleId.Observer);
        public static IntroDate VampireIntro = new IntroDate("Vampire", RoleClass.Vampire.color, 1, CustomRPC.RoleId.Vampire);
        public static IntroDate FoxIntro = new IntroDate("Fox", RoleClass.Fox.color, 1, CustomRPC.RoleId.Fox);
        public static IntroDate DarkKillerIntro = new IntroDate("DarkKiller", RoleClass.DarkKiller.color, 1, CustomRPC.RoleId.DarkKiller);
        public static IntroDate SeerIntro = new IntroDate("Seer", RoleClass.Seer.color, 1, CustomRPC.RoleId.Seer);
        public static IntroDate MadSeerIntro = new IntroDate("MadSeer", RoleClass.MadSeer.color, 1, CustomRPC.RoleId.MadSeer);
        public static IntroDate EvilSeerIntro = new IntroDate("EvilSeer", RoleClass.EvilSeer.color, 1, CustomRPC.RoleId.EvilSeer);
        public static IntroDate RemoteSheriffIntro = new IntroDate("RemoteSheriff", RoleClass.RemoteSheriff.color, 1, CustomRPC.RoleId.RemoteSheriff);
        public static IntroDate TeleportingJackalIntro = new IntroDate("TeleportingJackal", RoleClass.TeleportingJackal.color, 1, CustomRPC.RoleId.TeleportingJackal);
        public static IntroDate MadMakerIntro = new IntroDate("MadMaker", RoleClass.MadMaker.color, 1, CustomRPC.RoleId.MadMaker);
        public static IntroDate DemonIntro = new IntroDate("Demon", RoleClass.Demon.color, 1, CustomRPC.RoleId.Demon);
        public static IntroDate TaskManagerIntro = new IntroDate("TaskManager", RoleClass.TaskManager.color, 1, CustomRPC.RoleId.TaskManager);
        public static IntroDate SeerFriendsIntro = new IntroDate("SeerFriends", RoleClass.SeerFriends.color, 1, CustomRPC.RoleId.SeerFriends, TeamRoleType.Neutral);
        public static IntroDate JackalSeerIntro = new IntroDate("JackalSeer", RoleClass.JackalSeer.color, 1, CustomRPC.RoleId.JackalSeer, TeamRoleType.Neutral);
        public static IntroDate SidekickSeerIntro = new IntroDate("SidekickSeer", RoleClass.JackalSeer.color, 1, CustomRPC.RoleId.SidekickSeer, TeamRoleType.Neutral);
        public static IntroDate ArsonistIntro = new IntroDate("Arsonist", RoleClass.Arsonist.color, 1, CustomRPC.RoleId.Arsonist);
        //イントロオブジェ
    }
}
