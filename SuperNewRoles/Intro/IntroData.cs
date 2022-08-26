using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Intro
{
    public class IntroData
    {
        public static List<IntroData> IntroDatas = new();
        public static Dictionary<RoleId, IntroData> IntroDatasCache = new();
        public static List<IntroData> GhostRoleDatas = new();
        public string NameKey;
        public string Name;
        public Int16 TitleNum;
        public string TitleDesc;
        public Color color;
        public RoleId RoleId;
        public string Description;
        public TeamRoleType Team;
        public bool IsGhostRole;
        IntroData(string NameKey, Color color, Int16 TitleNum, RoleId RoleId, TeamRoleType team = TeamRoleType.Crewmate, bool IsGhostRole = false)
        {
            this.color = color;
            this.NameKey = NameKey;
            this.Name = ModTranslation.GetString(NameKey + "Name");
            this.RoleId = RoleId;
            this.TitleNum = TitleNum;
            this.TitleDesc = GetTitle(NameKey, TitleNum);
            this.Description = ModTranslation.GetString(NameKey + "Description");
            this.Team = team;
            this.IsGhostRole = IsGhostRole;

            if (IsGhostRole)
            {
                GhostRoleDatas.Add(this);
            }
            IntroDatas.Add(this);
        }
        public static IntroData GetIntroDate(RoleId RoleId, PlayerControl p = null)
        {
            if (RoleId == RoleId.DefaultRole)
            {
                return p != null && p.IsImpostor() ? ImpostorIntro : CrewmateIntro;
            }
            try
            {
                return IntroDatasCache[RoleId];
            }
            catch
            {
                var data = IntroDatas.FirstOrDefault((_) => _.RoleId == RoleId);
                if (data == null) data = CrewmateIntro;
                IntroDatasCache[RoleId] = data;
                return data;
            }
        }
        public static CustomRoleOption GetOption(RoleId roleId)
        {
            var option = CustomRoleOption.RoleOptions.FirstOrDefault((_) => _.RoleId == roleId);
            return option;
        }
        public static string GetTitle(string name, Int16 num)
        {
            System.Random r1 = new();
            return ModTranslation.GetString(name + "Title" + r1.Next(1, num + 1).ToString());
        }

        public static IntroData CrewmateIntro = new("CrewMate", Color.white, 1, RoleId.DefaultRole);
        public static IntroData ImpostorIntro = new("Impostor", RoleClass.ImpostorRed, 1, RoleId.DefaultRole, TeamRoleType.Impostor);
        public static IntroData SoothSayerIntro = new("SoothSayer", RoleClass.SoothSayer.color, 1, RoleId.SoothSayer);
        public static IntroData JesterIntro = new("Jester", RoleClass.Jester.color, 1, RoleId.Jester, TeamRoleType.Neutral);
        public static IntroData LighterIntro = new("Lighter", RoleClass.Lighter.color, 1, RoleId.Lighter);
        public static IntroData EvilLighterIntro = new("EvilLighter", RoleClass.EvilLighter.color, 2, RoleId.EvilLighter, TeamRoleType.Impostor);
        public static IntroData EvilScientist = new("EvilScientist", RoleClass.EvilScientist.color, 2, RoleId.EvilScientist, TeamRoleType.Impostor);
        public static IntroData SheriffIntro = new("Sheriff", RoleClass.Sheriff.color, 2, RoleId.Sheriff);
        public static IntroData MeetingSheriffIntro = new("MeetingSheriff", RoleClass.MeetingSheriff.color, 4, RoleId.MeetingSheriff);
        public static IntroData JackalIntro = new("Jackal", RoleClass.Jackal.color, 3, RoleId.Jackal, TeamRoleType.Neutral);
        public static IntroData SidekickIntro = new("Sidekick", RoleClass.Jackal.color, 1, RoleId.Sidekick, TeamRoleType.Neutral);
        public static IntroData TeleporterIntro = new("Teleporter", RoleClass.Teleporter.color, 2, RoleId.Teleporter, TeamRoleType.Impostor);
        public static IntroData SpiritMediumIntro = new("SpiritMedium", RoleClass.SpiritMedium.color, 1, RoleId.SpiritMedium);
        public static IntroData SpeedBoosterIntro = new("SpeedBooster", RoleClass.SpeedBooster.color, 2, RoleId.SpeedBooster);
        public static IntroData EvilSpeedBoosterIntro = new("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 4, RoleId.EvilSpeedBooster, TeamRoleType.Impostor);
        public static IntroData TaskerIntro = new("Tasker", RoleClass.Tasker.color, 2, RoleId.Tasker, TeamRoleType.Impostor);
        public static IntroData DoorrIntro = new("Doorr", RoleClass.Doorr.color, 2, RoleId.Doorr);
        public static IntroData EvilDoorrIntro = new("EvilDoorr", RoleClass.EvilDoorr.color, 3, RoleId.EvilDoorr, TeamRoleType.Impostor);
        public static IntroData ShielderIntro = new("Shielder", RoleClass.Shielder.color, 3, RoleId.Shielder);
        public static IntroData FreezerIntro = new("Freezer", RoleClass.Freezer.color, 3, RoleId.Freezer, TeamRoleType.Impostor);
        public static IntroData SpeederIntro = new("Speeder", RoleClass.Speeder.color, 2, RoleId.Speeder, TeamRoleType.Impostor);
        public static IntroData GuesserIntro = new("Guesser", RoleClass.Guesser.color, 2, RoleId.Guesser);
        public static IntroData EvilGuesserIntro = new("EvilGuesser", RoleClass.EvilGuesser.color, 1, RoleId.EvilGuesser, TeamRoleType.Impostor);
        public static IntroData VultureIntro = new("Vulture", RoleClass.Vulture.color, 1, RoleId.Vulture, TeamRoleType.Neutral);
        public static IntroData NiceScientistIntro = new("NiceScientist", RoleClass.NiceScientist.color, 2, RoleId.NiceScientist);
        public static IntroData ClergymanIntro = new("Clergyman", RoleClass.Clergyman.color, 2, RoleId.Clergyman);
        public static IntroData MadMateIntro = new("MadMate", RoleClass.MadMate.color, 1, RoleId.MadMate);
        public static IntroData BaitIntro = new("Bait", RoleClass.Bait.color, 1, RoleId.Bait);
        public static IntroData HomeSecurityGuardIntro = new("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, RoleId.HomeSecurityGuard);
        public static IntroData StuntManIntro = new("StuntMan", RoleClass.StuntMan.color, 1, RoleId.StuntMan);
        public static IntroData MovingIntro = new("Moving", RoleClass.Moving.color, 1, RoleId.Moving);
        public static IntroData OpportunistIntro = new("Opportunist", RoleClass.Opportunist.color, 2, RoleId.Opportunist, TeamRoleType.Neutral);
        public static IntroData NiceGamblerIntro = new("NiceGambler", RoleClass.NiceGambler.color, 1, RoleId.NiceGambler);
        public static IntroData EvilGamblerIntro = new("EvilGambler", RoleClass.EvilGambler.color, 1, RoleId.EvilGambler, TeamRoleType.Impostor);
        public static IntroData BestfalsechargeIntro = new("Bestfalsecharge", RoleClass.Bestfalsecharge.color, 1, RoleId.Bestfalsecharge);
        public static IntroData ResearcherIntro = new("Researcher", RoleClass.Researcher.color, 1, RoleId.Researcher, TeamRoleType.Neutral);
        public static IntroData SelfBomberIntro = new("SelfBomber", RoleClass.SelfBomber.color, 1, RoleId.SelfBomber, TeamRoleType.Impostor);
        public static IntroData GodIntro = new("God", RoleClass.God.color, 1, RoleId.God, TeamRoleType.Neutral);
        public static IntroData AllCleanerIntro = new("AllCleaner", RoleClass.AllCleaner.color, 1, RoleId.AllCleaner, TeamRoleType.Impostor);
        public static IntroData NiceNekomataIntro = new("NiceNekomata", RoleClass.NiceNekomata.color, 3, RoleId.NiceNekomata);
        public static IntroData EvilNekomataIntro = new("EvilNekomata", RoleClass.EvilNekomata.color, 1, RoleId.EvilNekomata, TeamRoleType.Impostor);
        public static IntroData JackalFriendsIntro = new("JackalFriends", RoleClass.JackalFriends.color, 2, RoleId.JackalFriends);
        public static IntroData DoctorIntro = new("Doctor", RoleClass.Doctor.color, 1, RoleId.Doctor);
        public static IntroData CountChangerIntro = new("CountChanger", RoleClass.CountChanger.color, 2, RoleId.CountChanger, TeamRoleType.Impostor);
        public static IntroData PursuerIntro = new("Pursuer", RoleClass.Pursuer.color, 3, RoleId.Pursuer, TeamRoleType.Impostor);
        public static IntroData MinimalistIntro = new("Minimalist", RoleClass.Minimalist.color, 2, RoleId.Minimalist, TeamRoleType.Impostor);
        public static IntroData HawkIntro = new("Hawk", RoleClass.Hawk.color, 1, RoleId.Hawk, TeamRoleType.Impostor);
        public static IntroData EgoistIntro = new("Egoist", RoleClass.Egoist.color, 1, RoleId.Egoist, TeamRoleType.Neutral);
        public static IntroData NiceRedRidingHoodIntro = new("NiceRedRidingHood", RoleClass.NiceRedRidingHood.color, 1, RoleId.NiceRedRidingHood);
        public static IntroData EvilEraserIntro = new("EvilEraser", RoleClass.EvilEraser.color, 1, RoleId.EvilEraser, TeamRoleType.Impostor);
        public static IntroData WorkpersonIntro = new("Workperson", RoleClass.Workperson.color, 1, RoleId.Workperson, TeamRoleType.Neutral);
        public static IntroData MagazinerIntro = new("Magaziner", RoleClass.Magaziner.color, 1, RoleId.Magaziner, TeamRoleType.Impostor);
        public static IntroData MayorIntro = new("Mayor", RoleClass.Mayor.color, 1, RoleId.Mayor);
        public static IntroData trueloverIntro = new("truelover", RoleClass.Truelover.color, 1, RoleId.truelover, TeamRoleType.Neutral);
        public static IntroData TechnicianIntro = new("Technician", RoleClass.Technician.color, 1, RoleId.Technician);
        public static IntroData SerialKillerIntro = new("SerialKiller", RoleClass.SerialKiller.color, 1, RoleId.SerialKiller, TeamRoleType.Impostor);
        public static IntroData OverKillerIntro = new("OverKiller", RoleClass.OverKiller.color, 1, RoleId.OverKiller, TeamRoleType.Impostor);
        public static IntroData LevelingerIntro = new("Levelinger", RoleClass.Levelinger.color, 1, RoleId.Levelinger, TeamRoleType.Impostor);
        public static IntroData EvilMovingIntro = new("EvilMoving", RoleClass.EvilMoving.color, 1, RoleId.EvilMoving, TeamRoleType.Impostor);
        public static IntroData AmnesiacIntro = new("Amnesiac", RoleClass.Amnesiac.color, 1, RoleId.Amnesiac, TeamRoleType.Neutral);
        public static IntroData SideKillerIntro = new("SideKiller", RoleClass.SideKiller.color, 1, RoleId.SideKiller, TeamRoleType.Impostor);
        public static IntroData MadKillerIntro = new("MadKiller", RoleClass.SideKiller.color, 1, RoleId.MadKiller, TeamRoleType.Impostor);
        public static IntroData SurvivorIntro = new("Survivor", RoleClass.Survivor.color, 1, RoleId.Survivor, TeamRoleType.Impostor);
        public static IntroData MadMayorIntro = new("MadMayor", RoleClass.MadMayor.color, 1, RoleId.MadMayor);
        public static IntroData NiceHawkIntro = new("NiceHawk", RoleClass.NiceHawk.color, 2, RoleId.NiceHawk);
        public static IntroData BakeryIntro = new("Bakery", RoleClass.Bakery.color, 1, RoleId.Bakery);
        public static IntroData MadStuntManIntro = new("MadStuntMan", RoleClass.MadStuntMan.color, 1, RoleId.MadStuntMan, TeamRoleType.Impostor);
        public static IntroData MadHawkIntro = new("MadHawk", RoleClass.MadHawk.color, 1, RoleId.MadHawk);
        public static IntroData MadJesterIntro = new("MadJester", RoleClass.MadJester.color, 1, RoleId.MadJester);
        public static IntroData FalseChargesIntro = new("FalseCharges", RoleClass.FalseCharges.color, 1, RoleId.FalseCharges, TeamRoleType.Neutral);
        public static IntroData NiceTeleporterIntro = new("NiceTeleporter", RoleClass.NiceTeleporter.color, 1, RoleId.NiceTeleporter);
        public static IntroData CelebrityIntro = new("Celebrity", RoleClass.Celebrity.color, 1, RoleId.Celebrity);
        public static IntroData NocturnalityIntro = new("Nocturnality", RoleClass.Nocturnality.color, 1, RoleId.Nocturnality);
        public static IntroData ObserverIntro = new("Observer", RoleClass.Observer.color, 1, RoleId.Observer);
        public static IntroData VampireIntro = new("Vampire", RoleClass.Vampire.color, 1, RoleId.Vampire, TeamRoleType.Impostor);
        public static IntroData FoxIntro = new("Fox", RoleClass.Fox.color, 1, RoleId.Fox, TeamRoleType.Neutral);
        public static IntroData DarkKillerIntro = new("DarkKiller", RoleClass.DarkKiller.color, 1, RoleId.DarkKiller, TeamRoleType.Impostor);
        public static IntroData SeerIntro = new("Seer", RoleClass.Seer.color, 1, RoleId.Seer);
        public static IntroData MadSeerIntro = new("MadSeer", RoleClass.MadSeer.color, 1, RoleId.MadSeer);
        public static IntroData EvilSeerIntro = new("EvilSeer", RoleClass.EvilSeer.color, 1, RoleId.EvilSeer, TeamRoleType.Impostor);
        public static IntroData RemoteSheriffIntro = new("RemoteSheriff", RoleClass.RemoteSheriff.color, 1, RoleId.RemoteSheriff);
        public static IntroData TeleportingJackalIntro = new("TeleportingJackal", RoleClass.TeleportingJackal.color, 1, RoleId.TeleportingJackal);
        public static IntroData MadMakerIntro = new("MadMaker", RoleClass.MadMaker.color, 1, RoleId.MadMaker);
        public static IntroData DemonIntro = new("Demon", RoleClass.Demon.color, 1, RoleId.Demon, TeamRoleType.Neutral);
        public static IntroData TaskManagerIntro = new("TaskManager", RoleClass.TaskManager.color, 1, RoleId.TaskManager);
        public static IntroData SeerFriendsIntro = new("SeerFriends", RoleClass.SeerFriends.color, 1, RoleId.SeerFriends);
        public static IntroData JackalSeerIntro = new("JackalSeer", RoleClass.JackalSeer.color, 1, RoleId.JackalSeer, TeamRoleType.Neutral);
        public static IntroData SidekickSeerIntro = new("SidekickSeer", RoleClass.JackalSeer.color, 1, RoleId.SidekickSeer, TeamRoleType.Neutral);
        public static IntroData AssassinIntro = new("Assassin", RoleClass.Assassin.color, 1, RoleId.Assassin);
        public static IntroData MarineIntro = new("Marine", RoleClass.Marine.color, 1, RoleId.Marine);
        public static IntroData ArsonistIntro = new("Arsonist", RoleClass.Arsonist.color, 1, RoleId.Arsonist, TeamRoleType.Neutral);
        public static IntroData ChiefIntro = new("Chief", RoleClass.Chief.color, 1, RoleId.Chief);
        public static IntroData CleanerIntro = new("Cleaner", RoleClass.Cleaner.color, 1, RoleId.Cleaner, TeamRoleType.Impostor);
        public static IntroData MadCleanerIntro = new("MadCleaner", RoleClass.MadCleaner.color, 1, RoleId.MadCleaner);
        public static IntroData SamuraiIntro = new("Samurai", RoleClass.Samurai.color, 1, RoleId.Samurai, TeamRoleType.Impostor);
        public static IntroData MayorFriendsIntro = new("MayorFriends", RoleClass.MayorFriends.color, 1, RoleId.MayorFriends);
        public static IntroData VentMakerIntro = new("VentMaker", RoleClass.VentMaker.color, 1, RoleId.VentMaker, TeamRoleType.Impostor);
        public static IntroData GhostMechanicIntro = new("GhostMechanic", RoleClass.GhostMechanic.color, 1, RoleId.GhostMechanic, TeamRoleType.Crewmate, true);
        public static IntroData EvilHackerIntro = new("EvilHacker", RoleClass.EvilHacker.color, 1, RoleId.EvilHacker, TeamRoleType.Impostor);
        public static IntroData HauntedWolfIntro = new("HauntedWolf", RoleClass.HauntedWolf.color, 1, RoleId.HauntedWolf, TeamRoleType.Crewmate);
        public static IntroData PositionSwapperIntro = new("PositionSwapper", RoleClass.PositionSwapper.color, 1, RoleId.PositionSwapper, TeamRoleType.Impostor);
        public static IntroData TunaIntro = new("Tuna", RoleClass.Tuna.color, 1, RoleId.Tuna, TeamRoleType.Neutral);
        public static IntroData MafiaIntro = new("Mafia", RoleClass.Mafia.color, 1, RoleId.Mafia, TeamRoleType.Impostor);
        public static IntroData BlackCatIntro = new("BlackCat", RoleClass.BlackCat.color, 1, RoleId.BlackCat);
        public static IntroData SecretlyKillerIntro = new("SecretlyKiller", RoleClass.SecretlyKiller.color, 1, RoleId.SecretlyKiller, TeamRoleType.Impostor);
        public static IntroData SpyIntro = new("Spy", RoleClass.Spy.color, 1, RoleId.Spy, TeamRoleType.Crewmate);
        public static IntroData KunoichiIntro = new("Kunoichi", RoleClass.Kunoichi.color, 1, RoleId.Kunoichi, TeamRoleType.Impostor);
        public static IntroData DoubleKillerIntro = new("DoubleKiller", RoleClass.DoubleKiller.color, 1, RoleId.DoubleKiller, TeamRoleType.Impostor);
        public static IntroData SmasherIntro = new("Smasher", RoleClass.Smasher.color, 1, RoleId.Smasher, TeamRoleType.Impostor);
        public static IntroData SuicideWisherIntro = new("SuicideWisher", RoleClass.SuicideWisher.color, 1, RoleId.SuicideWisher, TeamRoleType.Impostor);
        public static IntroData NeetIntro = new("Neet", RoleClass.Neet.color, 1, RoleId.Neet, TeamRoleType.Neutral);
        public static IntroData FastMakerIntro = new("FastMaker", RoleClass.FastMaker.color, 1, RoleId.FastMaker, TeamRoleType.Impostor);
        public static IntroData ToiletFanIntro = new("ToiletFan", RoleClass.ToiletFan.color, 1, RoleId.ToiletFan, TeamRoleType.Crewmate);
        public static IntroData SatsumaAndImoIntro = new("SatsumaAndImo", RoleClass.SatsumaAndImo.color, 1, RoleId.SatsumaAndImo, TeamRoleType.Crewmate);
        public static IntroData EvilButtonerIntro = new("EvilButtoner", RoleClass.EvilButtoner.color, 1, RoleId.EvilButtoner, TeamRoleType.Impostor);
        public static IntroData NiceButtonerIntro = new("NiceButtoner", RoleClass.NiceButtoner.color, 1, RoleId.NiceButtoner, TeamRoleType.Crewmate);
        public static IntroData FinderIntro = new("Finder", RoleClass.Finder.color, 1, RoleId.Finder, TeamRoleType.Impostor);
        public static IntroData RevolutionistIntro = new("Revolutionist", RoleClass.Revolutionist.color, 1, RoleId.Revolutionist, TeamRoleType.Neutral);
        public static IntroData DictatorIntro = new("Dictator", RoleClass.Dictator.color, 1, RoleId.Dictator, TeamRoleType.Crewmate);
        public static IntroData SpelunkerIntro = new("Spelunker", RoleClass.Spelunker.color, 1, RoleId.Spelunker, TeamRoleType.Neutral);
        public static IntroData SuicidalIdeationIntro = new("SuicidalIdeation", RoleClass.SuicidalIdeation.color, 1, RoleId.SuicidalIdeation, TeamRoleType.Neutral);
        public static IntroData HitmanIntro = new("Hitman", RoleClass.Hitman.color, 1, RoleId.Hitman, TeamRoleType.Neutral);
        public static IntroData MatryoshkaIntro = new("Matryoshka", RoleClass.Matryoshka.color, 1, RoleId.Matryoshka, TeamRoleType.Impostor);
        public static IntroData NunIntro = new("Nun", RoleClass.Nun.color, 1, RoleId.Nun, TeamRoleType.Impostor);
        public static IntroData PsychometristIntro = new("Psychometrist", RoleClass.Psychometrist.color, 1, RoleId.Psychometrist, TeamRoleType.Crewmate);
        public static IntroData SeeThroughPersonIntro = new("SeeThroughPerson", RoleClass.SeeThroughPerson.color, 1, RoleId.SeeThroughPerson, TeamRoleType.Crewmate);
        public static IntroData PartTimerIntro = new("PartTimer", RoleClass.PartTimer.color, 1, RoleId.PartTimer, TeamRoleType.Neutral);
        public static IntroData PainterIntro = new("Painter", RoleClass.Painter.color, 1, RoleId.Painter, TeamRoleType.Crewmate);
        public static IntroData PhotographerIntro = new("Photographer", RoleClass.Photographer.color, 1, RoleId.Photographer, TeamRoleType.Neutral);
        public static IntroData StefinderIntro = new("Stefinder", RoleClass.Stefinder.color, 1, RoleId.Stefinder, TeamRoleType.Neutral);
        public static IntroData StefinderIntro1 = new("Stefinder", RoleClass.ImpostorRed, 1, RoleId.Stefinder1, TeamRoleType.Neutral);
        public static IntroData SluggerIntro = new("Slugger", RoleClass.Slugger.color, 1, RoleId.Slugger, TeamRoleType.Impostor);
        //イントロオブジェ
    }
}