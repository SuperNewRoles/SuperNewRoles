using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Modules
{
    public class TeamDate
    {
        public string NameKey;
        public Color color;
        public Color BackGround;
        public List<RoleId> RoleIds;

        TeamDate(string NameKey, Color color, Color BackGround, List<RoleId> RoleId)
        {
            this.color = color;
            this.BackGround = BackGround;
            this.NameKey = NameKey;
            RoleIds = RoleId;
        }
        public static TeamDate VultureTeam = new("Test", Color.black, Color.yellow, new List<RoleId> { RoleId.Sheriff });
    }
    public class IntroDate
    {
        public static List<IntroDate> IntroDatas = new();
        public static Dictionary<RoleId, IntroDate> IntroDatasCache = new();
        public static List<IntroDate> GhostRoleDatas = new();
        public string NameKey;
        public string Name;
        public Int16 TitleNum;
        public string TitleDesc;
        public Color color;
        public RoleId RoleId;
        public string Description;
        public TeamRoleType Team;
        public bool IsGhostRole;
        IntroDate(string NameKey, Color color, Int16 TitleNum, RoleId RoleId, TeamRoleType team = TeamRoleType.Crewmate, bool IsGhostRole = false)
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
        public static IntroDate GetIntroDate(RoleId RoleId, PlayerControl p = null)
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

        public static IntroDate CrewmateIntro = new("CrewMate", Color.white, 1, RoleId.DefaultRole);
        public static IntroDate ImpostorIntro = new("Impostor", RoleClass.ImpostorRed, 1, RoleId.DefaultRole, TeamRoleType.Impostor);
        public static IntroDate SoothSayerIntro = new("SoothSayer", RoleClass.SoothSayer.color, 1, RoleId.SoothSayer);
        public static IntroDate JesterIntro = new("Jester", RoleClass.Jester.color, 1, RoleId.Jester, TeamRoleType.Neutral);
        public static IntroDate LighterIntro = new("Lighter", RoleClass.Lighter.color, 1, RoleId.Lighter);
        public static IntroDate EvilLighterIntro = new("EvilLighter", RoleClass.EvilLighter.color, 2, RoleId.EvilLighter, TeamRoleType.Impostor);
        public static IntroDate EvilScientist = new("EvilScientist", RoleClass.EvilScientist.color, 2, RoleId.EvilScientist, TeamRoleType.Impostor);
        public static IntroDate SheriffIntro = new("Sheriff", RoleClass.Sheriff.color, 2, RoleId.Sheriff);
        public static IntroDate MeetingSheriffIntro = new("MeetingSheriff", RoleClass.MeetingSheriff.color, 4, RoleId.MeetingSheriff);
        public static IntroDate JackalIntro = new("Jackal", RoleClass.Jackal.color, 3, RoleId.Jackal, TeamRoleType.Neutral);
        public static IntroDate SidekickIntro = new("Sidekick", RoleClass.Jackal.color, 1, RoleId.Sidekick, TeamRoleType.Neutral);
        public static IntroDate TeleporterIntro = new("Teleporter", RoleClass.Teleporter.color, 2, RoleId.Teleporter, TeamRoleType.Impostor);
        public static IntroDate SpiritMediumIntro = new("SpiritMedium", RoleClass.SpiritMedium.color, 1, RoleId.SpiritMedium);
        public static IntroDate SpeedBoosterIntro = new("SpeedBooster", RoleClass.SpeedBooster.color, 2, RoleId.SpeedBooster);
        public static IntroDate EvilSpeedBoosterIntro = new("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 4, RoleId.EvilSpeedBooster, TeamRoleType.Impostor);
        public static IntroDate TaskerIntro = new("Tasker", RoleClass.Tasker.color, 2, RoleId.Tasker, TeamRoleType.Impostor);
        public static IntroDate DoorrIntro = new("Doorr", RoleClass.Doorr.color, 2, RoleId.Doorr);
        public static IntroDate EvilDoorrIntro = new("EvilDoorr", RoleClass.EvilDoorr.color, 3, RoleId.EvilDoorr, TeamRoleType.Impostor);
        public static IntroDate ShielderIntro = new("Shielder", RoleClass.Shielder.color, 3, RoleId.Shielder);
        public static IntroDate FreezerIntro = new("Freezer", RoleClass.Freezer.color, 3, RoleId.Freezer, TeamRoleType.Impostor);
        public static IntroDate SpeederIntro = new("Speeder", RoleClass.Speeder.color, 2, RoleId.Speeder, TeamRoleType.Impostor);
        public static IntroDate NiceGuesserIntro = new("NiceGuesser", RoleClass.NiceGuesser.color, 1, RoleId.NiceGuesser, TeamRoleType.Crewmate);
        public static IntroDate EvilGuesserIntro = new("EvilGuesser", RoleClass.EvilGuesser.color, 1, RoleId.EvilGuesser, TeamRoleType.Impostor);
        public static IntroDate VultureIntro = new("Vulture", RoleClass.Vulture.color, 1, RoleId.Vulture, TeamRoleType.Neutral);
        public static IntroDate NiceScientistIntro = new("NiceScientist", RoleClass.NiceScientist.color, 2, RoleId.NiceScientist);
        public static IntroDate ClergymanIntro = new("Clergyman", RoleClass.Clergyman.color, 2, RoleId.Clergyman);
        public static IntroDate MadMateIntro = new("MadMate", RoleClass.MadMate.color, 1, RoleId.MadMate);
        public static IntroDate BaitIntro = new("Bait", RoleClass.Bait.color, 1, RoleId.Bait);
        public static IntroDate HomeSecurityGuardIntro = new("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, RoleId.HomeSecurityGuard);
        public static IntroDate StuntManIntro = new("StuntMan", RoleClass.StuntMan.color, 1, RoleId.StuntMan);
        public static IntroDate MovingIntro = new("Moving", RoleClass.Moving.color, 1, RoleId.Moving);
        public static IntroDate OpportunistIntro = new("Opportunist", RoleClass.Opportunist.color, 2, RoleId.Opportunist, TeamRoleType.Neutral);
        public static IntroDate NiceGamblerIntro = new("NiceGambler", RoleClass.NiceGambler.color, 1, RoleId.NiceGambler);
        public static IntroDate EvilGamblerIntro = new("EvilGambler", RoleClass.EvilGambler.color, 1, RoleId.EvilGambler, TeamRoleType.Impostor);
        public static IntroDate BestfalsechargeIntro = new("Bestfalsecharge", RoleClass.Bestfalsecharge.color, 1, RoleId.Bestfalsecharge);
        public static IntroDate ResearcherIntro = new("Researcher", RoleClass.Researcher.color, 1, RoleId.Researcher, TeamRoleType.Neutral);
        public static IntroDate SelfBomberIntro = new("SelfBomber", RoleClass.SelfBomber.color, 1, RoleId.SelfBomber, TeamRoleType.Impostor);
        public static IntroDate GodIntro = new("God", RoleClass.God.color, 1, RoleId.God, TeamRoleType.Neutral);
        public static IntroDate AllCleanerIntro = new("AllCleaner", RoleClass.AllCleaner.color, 1, RoleId.AllCleaner, TeamRoleType.Impostor);
        public static IntroDate NiceNekomataIntro = new("NiceNekomata", RoleClass.NiceNekomata.color, 3, RoleId.NiceNekomata);
        public static IntroDate EvilNekomataIntro = new("EvilNekomata", RoleClass.EvilNekomata.color, 1, RoleId.EvilNekomata, TeamRoleType.Impostor);
        public static IntroDate JackalFriendsIntro = new("JackalFriends", RoleClass.JackalFriends.color, 2, RoleId.JackalFriends);
        public static IntroDate DoctorIntro = new("Doctor", RoleClass.Doctor.color, 1, RoleId.Doctor);
        public static IntroDate CountChangerIntro = new("CountChanger", RoleClass.CountChanger.color, 2, RoleId.CountChanger, TeamRoleType.Impostor);
        public static IntroDate PursuerIntro = new("Pursuer", RoleClass.Pursuer.color, 3, RoleId.Pursuer, TeamRoleType.Impostor);
        public static IntroDate MinimalistIntro = new("Minimalist", RoleClass.Minimalist.color, 2, RoleId.Minimalist, TeamRoleType.Impostor);
        public static IntroDate HawkIntro = new("Hawk", RoleClass.Hawk.color, 1, RoleId.Hawk, TeamRoleType.Impostor);
        public static IntroDate EgoistIntro = new("Egoist", RoleClass.Egoist.color, 1, RoleId.Egoist, TeamRoleType.Neutral);
        public static IntroDate NiceRedRidingHoodIntro = new("NiceRedRidingHood", RoleClass.NiceRedRidingHood.color, 1, RoleId.NiceRedRidingHood);
        public static IntroDate EvilEraserIntro = new("EvilEraser", RoleClass.EvilEraser.color, 1, RoleId.EvilEraser, TeamRoleType.Impostor);
        public static IntroDate WorkpersonIntro = new("Workperson", RoleClass.Workperson.color, 1, RoleId.Workperson, TeamRoleType.Neutral);
        public static IntroDate MagazinerIntro = new("Magaziner", RoleClass.Magaziner.color, 1, RoleId.Magaziner, TeamRoleType.Impostor);
        public static IntroDate MayorIntro = new("Mayor", RoleClass.Mayor.color, 1, RoleId.Mayor);
        public static IntroDate trueloverIntro = new("truelover", RoleClass.Truelover.color, 1, RoleId.truelover, TeamRoleType.Neutral);
        public static IntroDate TechnicianIntro = new("Technician", RoleClass.Technician.color, 1, RoleId.Technician);
        public static IntroDate SerialKillerIntro = new("SerialKiller", RoleClass.SerialKiller.color, 1, RoleId.SerialKiller, TeamRoleType.Impostor);
        public static IntroDate OverKillerIntro = new("OverKiller", RoleClass.OverKiller.color, 1, RoleId.OverKiller, TeamRoleType.Impostor);
        public static IntroDate LevelingerIntro = new("Levelinger", RoleClass.Levelinger.color, 1, RoleId.Levelinger, TeamRoleType.Impostor);
        public static IntroDate EvilMovingIntro = new("EvilMoving", RoleClass.EvilMoving.color, 1, RoleId.EvilMoving, TeamRoleType.Impostor);
        public static IntroDate AmnesiacIntro = new("Amnesiac", RoleClass.Amnesiac.color, 1, RoleId.Amnesiac, TeamRoleType.Neutral);
        public static IntroDate SideKillerIntro = new("SideKiller", RoleClass.SideKiller.color, 1, RoleId.SideKiller, TeamRoleType.Impostor);
        public static IntroDate MadKillerIntro = new("MadKiller", RoleClass.SideKiller.color, 1, RoleId.MadKiller, TeamRoleType.Impostor);
        public static IntroDate SurvivorIntro = new("Survivor", RoleClass.Survivor.color, 1, RoleId.Survivor, TeamRoleType.Impostor);
        public static IntroDate MadMayorIntro = new("MadMayor", RoleClass.MadMayor.color, 1, RoleId.MadMayor);
        public static IntroDate NiceHawkIntro = new("NiceHawk", RoleClass.NiceHawk.color, 2, RoleId.NiceHawk);
        public static IntroDate BakeryIntro = new("Bakery", RoleClass.Bakery.color, 1, RoleId.Bakery);
        public static IntroDate MadStuntManIntro = new("MadStuntMan", RoleClass.MadStuntMan.color, 1, RoleId.MadStuntMan, TeamRoleType.Impostor);
        public static IntroDate MadHawkIntro = new("MadHawk", RoleClass.MadHawk.color, 1, RoleId.MadHawk);
        public static IntroDate MadJesterIntro = new("MadJester", RoleClass.MadJester.color, 1, RoleId.MadJester);
        public static IntroDate FalseChargesIntro = new("FalseCharges", RoleClass.FalseCharges.color, 1, RoleId.FalseCharges, TeamRoleType.Neutral);
        public static IntroDate NiceTeleporterIntro = new("NiceTeleporter", RoleClass.NiceTeleporter.color, 1, RoleId.NiceTeleporter);
        public static IntroDate CelebrityIntro = new("Celebrity", RoleClass.Celebrity.color, 1, RoleId.Celebrity);
        public static IntroDate NocturnalityIntro = new("Nocturnality", RoleClass.Nocturnality.color, 1, RoleId.Nocturnality);
        public static IntroDate ObserverIntro = new("Observer", RoleClass.Observer.color, 1, RoleId.Observer);
        public static IntroDate VampireIntro = new("Vampire", RoleClass.Vampire.color, 1, RoleId.Vampire, TeamRoleType.Impostor);
        public static IntroDate FoxIntro = new("Fox", RoleClass.Fox.color, 1, RoleId.Fox, TeamRoleType.Neutral);
        public static IntroDate DarkKillerIntro = new("DarkKiller", RoleClass.DarkKiller.color, 1, RoleId.DarkKiller, TeamRoleType.Impostor);
        public static IntroDate SeerIntro = new("Seer", RoleClass.Seer.color, 1, RoleId.Seer);
        public static IntroDate MadSeerIntro = new("MadSeer", RoleClass.MadSeer.color, 1, RoleId.MadSeer);
        public static IntroDate EvilSeerIntro = new("EvilSeer", RoleClass.EvilSeer.color, 1, RoleId.EvilSeer, TeamRoleType.Impostor);
        public static IntroDate RemoteSheriffIntro = new("RemoteSheriff", RoleClass.RemoteSheriff.color, 1, RoleId.RemoteSheriff);
        public static IntroDate TeleportingJackalIntro = new("TeleportingJackal", RoleClass.TeleportingJackal.color, 1, RoleId.TeleportingJackal);
        public static IntroDate MadMakerIntro = new("MadMaker", RoleClass.MadMaker.color, 1, RoleId.MadMaker);
        public static IntroDate DemonIntro = new("Demon", RoleClass.Demon.color, 1, RoleId.Demon, TeamRoleType.Neutral);
        public static IntroDate TaskManagerIntro = new("TaskManager", RoleClass.TaskManager.color, 1, RoleId.TaskManager);
        public static IntroDate SeerFriendsIntro = new("SeerFriends", RoleClass.SeerFriends.color, 1, RoleId.SeerFriends);
        public static IntroDate JackalSeerIntro = new("JackalSeer", RoleClass.JackalSeer.color, 1, RoleId.JackalSeer, TeamRoleType.Neutral);
        public static IntroDate SidekickSeerIntro = new("SidekickSeer", RoleClass.JackalSeer.color, 1, RoleId.SidekickSeer, TeamRoleType.Neutral);
        public static IntroDate AssassinIntro = new("Assassin", RoleClass.Assassin.color, 1, RoleId.Assassin);
        public static IntroDate MarineIntro = new("Marine", RoleClass.Marine.color, 1, RoleId.Marine);
        public static IntroDate ArsonistIntro = new("Arsonist", RoleClass.Arsonist.color, 1, RoleId.Arsonist, TeamRoleType.Neutral);
        public static IntroDate ChiefIntro = new("Chief", RoleClass.Chief.color, 1, RoleId.Chief);
        public static IntroDate CleanerIntro = new("Cleaner", RoleClass.Cleaner.color, 1, RoleId.Cleaner, TeamRoleType.Impostor);
        public static IntroDate MadCleanerIntro = new("MadCleaner", RoleClass.MadCleaner.color, 1, RoleId.MadCleaner);
        public static IntroDate SamuraiIntro = new("Samurai", RoleClass.Samurai.color, 1, RoleId.Samurai, TeamRoleType.Impostor);
        public static IntroDate MayorFriendsIntro = new("MayorFriends", RoleClass.MayorFriends.color, 1, RoleId.MayorFriends);
        public static IntroDate VentMakerIntro = new("VentMaker", RoleClass.VentMaker.color, 1, RoleId.VentMaker, TeamRoleType.Impostor);
        public static IntroDate GhostMechanicIntro = new("GhostMechanic", RoleClass.GhostMechanic.color, 1, RoleId.GhostMechanic, TeamRoleType.Crewmate, true);
        public static IntroDate EvilHackerIntro = new("EvilHacker", RoleClass.EvilHacker.color, 1, RoleId.EvilHacker, TeamRoleType.Impostor);
        public static IntroDate HauntedWolfIntro = new("HauntedWolf", RoleClass.HauntedWolf.color, 1, RoleId.HauntedWolf, TeamRoleType.Crewmate);
        public static IntroDate PositionSwapperIntro = new("PositionSwapper", RoleClass.PositionSwapper.color, 1, RoleId.PositionSwapper, TeamRoleType.Impostor);
        public static IntroDate TunaIntro = new("Tuna", RoleClass.Tuna.color, 1, RoleId.Tuna, TeamRoleType.Neutral);
        public static IntroDate MafiaIntro = new("Mafia", RoleClass.Mafia.color, 1, RoleId.Mafia, TeamRoleType.Impostor);
        public static IntroDate BlackCatIntro = new("BlackCat", RoleClass.BlackCat.color, 1, RoleId.BlackCat);
        public static IntroDate SecretlyKillerIntro = new("SecretlyKiller", RoleClass.SecretlyKiller.color, 1, RoleId.SecretlyKiller, TeamRoleType.Impostor);
        public static IntroDate SpyIntro = new("Spy", RoleClass.Spy.color, 1, RoleId.Spy, TeamRoleType.Crewmate);
        public static IntroDate KunoichiIntro = new("Kunoichi", RoleClass.Kunoichi.color, 1, RoleId.Kunoichi, TeamRoleType.Impostor);
        public static IntroDate DoubleKillerIntro = new("DoubleKiller", RoleClass.DoubleKiller.color, 1, RoleId.DoubleKiller, TeamRoleType.Impostor);
        public static IntroDate SmasherIntro = new("Smasher", RoleClass.Smasher.color, 1, RoleId.Smasher, TeamRoleType.Impostor);
        public static IntroDate SuicideWisherIntro = new("SuicideWisher", RoleClass.SuicideWisher.color, 1, RoleId.SuicideWisher, TeamRoleType.Impostor);
        public static IntroDate NeetIntro = new("Neet", RoleClass.Neet.color, 1, RoleId.Neet, TeamRoleType.Neutral);
        public static IntroDate FastMakerIntro = new("FastMaker", RoleClass.FastMaker.color, 1, RoleId.FastMaker, TeamRoleType.Impostor);
        public static IntroDate ToiletFanIntro = new("ToiletFan", RoleClass.ToiletFan.color, 1, RoleId.ToiletFan, TeamRoleType.Crewmate);
        public static IntroDate SatsumaAndImoIntro = new("SatsumaAndImo", RoleClass.SatsumaAndImo.color, 1, RoleId.SatsumaAndImo, TeamRoleType.Crewmate);
        public static IntroDate EvilButtonerIntro = new("EvilButtoner", RoleClass.EvilButtoner.color, 1, RoleId.EvilButtoner, TeamRoleType.Impostor);
        public static IntroDate NiceButtonerIntro = new("NiceButtoner", RoleClass.NiceButtoner.color, 1, RoleId.NiceButtoner, TeamRoleType.Crewmate);
        public static IntroDate FinderIntro = new("Finder", RoleClass.Finder.color, 1, RoleId.Finder, TeamRoleType.Impostor);
        public static IntroDate RevolutionistIntro = new("Revolutionist", RoleClass.Revolutionist.color, 1, RoleId.Revolutionist, TeamRoleType.Neutral);
        public static IntroDate DictatorIntro = new("Dictator", RoleClass.Dictator.color, 1, RoleId.Dictator, TeamRoleType.Crewmate);
        public static IntroDate SpelunkerIntro = new("Spelunker", RoleClass.Spelunker.color, 1, RoleId.Spelunker, TeamRoleType.Neutral);
        public static IntroDate SuicidalIdeationIntro = new("SuicidalIdeation", RoleClass.SuicidalIdeation.color, 1, RoleId.SuicidalIdeation, TeamRoleType.Neutral);
        public static IntroDate HitmanIntro = new("Hitman", RoleClass.Hitman.color, 1, RoleId.Hitman, TeamRoleType.Neutral);
        public static IntroDate MatryoshkaIntro = new("Matryoshka", RoleClass.Matryoshka.color, 1, RoleId.Matryoshka, TeamRoleType.Impostor);
        public static IntroDate NunIntro = new("Nun", RoleClass.Nun.color, 1, RoleId.Nun, TeamRoleType.Impostor);
        public static IntroDate PsychometristIntro = new("Psychometrist", RoleClass.Psychometrist.color, 1, RoleId.Psychometrist, TeamRoleType.Crewmate);
        public static IntroDate SeeThroughPersonIntro = new("SeeThroughPerson", RoleClass.SeeThroughPerson.color, 1, RoleId.SeeThroughPerson, TeamRoleType.Crewmate);
        public static IntroDate PartTimerIntro = new("PartTimer", RoleClass.PartTimer.color, 1, RoleId.PartTimer, TeamRoleType.Neutral);
        public static IntroDate PainterIntro = new("Painter", RoleClass.Painter.color, 1, RoleId.Painter, TeamRoleType.Crewmate);
        public static IntroDate PhotographerIntro = new("Photographer", RoleClass.Photographer.color, 1, RoleId.Photographer, TeamRoleType.Neutral);
        public static IntroDate StefinderIntro = new("Stefinder", RoleClass.Stefinder.color, 1, RoleId.Stefinder, TeamRoleType.Neutral);
        public static IntroDate StefinderIntro1 = new("Stefinder", RoleClass.ImpostorRed, 1, RoleId.Stefinder1, TeamRoleType.Neutral);
        public static IntroDate SluggerIntro = new("Slugger", RoleClass.Slugger.color, 1, RoleId.Slugger, TeamRoleType.Impostor);
        public static IntroDate ShiftActorIntro = new("ShiftActor", ShiftActor.color, 1, RoleId.ShiftActor, TeamRoleType.Impostor);
        public static IntroDate ConnectKillerIntro = new("ConnectKiller", RoleClass.ConnectKiller.color, 1, RoleId.ConnectKiller, TeamRoleType.Impostor);
        public static IntroDate GMIntro = new("GM", RoleClass.GM.color, 1, RoleId.GM, TeamRoleType.Neutral);
        public static IntroDate CrackerIntro = new("Cracker", RoleClass.Cracker.color, 1, RoleId.Cracker, TeamRoleType.Impostor);
        public static IntroDate NekoKabochaIntro = new("NekoKabocha", NekoKabocha.color, 1, RoleId.NekoKabocha, TeamRoleType.Impostor);
        public static IntroDate WaveCannonIntro = new("WaveCannon", RoleClass.WaveCannon.color, 1, RoleId.WaveCannon, TeamRoleType.Impostor);
        public static IntroDate DoppelgangerIntro = new("Doppelganger", RoleClass.Doppelganger.color, 1, RoleId.Doppelganger, TeamRoleType.Impostor);
        public static IntroDate WerewolfIntro = new("Werewolf", RoleClass.Werewolf.color, 1, RoleId.Werewolf, TeamRoleType.Impostor);
        public static IntroDate KnightIntro = new("Knight", Roles.CrewMate.Knight.color, 1, RoleId.Knight, TeamRoleType.Crewmate);
        public static IntroDate PavlovsdogsIntro = new("Pavlovsdogs", RoleClass.Pavlovsdogs.color, 1, RoleId.Pavlovsdogs, TeamRoleType.Neutral);
        public static IntroDate PavlovsownerIntro = new("Pavlovsowner", RoleClass.Pavlovsowner.color, 1, RoleId.Pavlovsowner, TeamRoleType.Neutral);
        public static IntroDate WaveCannonJackalIntro = new("WaveCannonJackal", RoleClass.WaveCannonJackal.color, 1, RoleId.WaveCannonJackal, TeamRoleType.Neutral);
        public static IntroDate ConjurerIntro = new("Conjurer", Conjurer.color, 1, RoleId.Conjurer, TeamRoleType.Impostor);
        public static IntroDate CamouflagerIntro = new("Camouflager", RoleClass.Camouflager.color, 1, RoleId.Camouflager, TeamRoleType.Impostor);
        //イントロオブジェ
    }
}