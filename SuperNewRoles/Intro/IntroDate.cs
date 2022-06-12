using HarmonyLib;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Intro
{
    public class IntroDate
    {
        public static List<IntroDate> IntroDatas = new List<IntroDate>();
        public static Dictionary<RoleId, IntroDate> IntroDatasCache = new Dictionary<RoleId, IntroDate>();
        public static List<IntroDate> GhostRoleDatas = new List<IntroDate>();
        public string NameKey;
        public string Name;
        public Int16 TitleNum;
        public string TitleDesc;
        public Color color;
        public CustomRPC.RoleId RoleId;
        public string Description;
        public TeamRoleType Team;
        public bool IsGhostRole;
        IntroDate(string NameKey, Color color, Int16 TitleNum, CustomRPC.RoleId RoleId, TeamRoleType team = TeamRoleType.Crewmate, bool IsGhostRole = false)
        {
            this.color = color;
            this.NameKey = NameKey;
            this.Name = ModTranslation.getString(NameKey + "Name");
            this.RoleId = RoleId;
            this.TitleNum = TitleNum;
            this.TitleDesc = GetTitle(NameKey, TitleNum);
            this.Description = ModTranslation.getString(NameKey + "Description");
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
                if (p != null && p.isImpostor())
                {
                    return ImpostorIntro;
                }
                else
                {
                    return CrewmateIntro;
                }

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
            System.Random r1 = new System.Random();
            return ModTranslation.getString(name + "Title" + r1.Next(1, num + 1).ToString());
        }
        public static IntroDate CrewmateIntro = new IntroDate("CrewMate", Color.white, 1, CustomRPC.RoleId.DefaultRole);
        public static IntroDate ImpostorIntro = new IntroDate("Impostor", RoleClass.ImpostorRed, 1, CustomRPC.RoleId.DefaultRole, TeamRoleType.Impostor);
        public static IntroDate SoothSayerIntro = new IntroDate("SoothSayer", RoleClass.SoothSayer.color, 1, CustomRPC.RoleId.SoothSayer);
        public static IntroDate JesterIntro = new IntroDate("Jester", RoleClass.Jester.color, 1, CustomRPC.RoleId.Jester, TeamRoleType.Neutral);
        public static IntroDate LighterIntro = new IntroDate("Lighter", RoleClass.Lighter.color, 1, CustomRPC.RoleId.Lighter);
        public static IntroDate EvilLighterIntro = new IntroDate("EvilLighter", RoleClass.EvilLighter.color, 2, CustomRPC.RoleId.EvilLighter, TeamRoleType.Impostor);
        public static IntroDate EvilScientist = new IntroDate("EvilScientist", RoleClass.EvilScientist.color, 2, CustomRPC.RoleId.EvilScientist, TeamRoleType.Impostor);
        public static IntroDate SheriffIntro = new IntroDate("Sheriff", RoleClass.Sheriff.color, 2, CustomRPC.RoleId.Sheriff);
        public static IntroDate MeetingSheriffIntro = new IntroDate("MeetingSheriff", RoleClass.MeetingSheriff.color, 4, CustomRPC.RoleId.MeetingSheriff);
        public static IntroDate JackalIntro = new IntroDate("Jackal", RoleClass.Jackal.color, 3, CustomRPC.RoleId.Jackal, TeamRoleType.Neutral);
        public static IntroDate SidekickIntro = new IntroDate("Sidekick", RoleClass.Jackal.color, 1, CustomRPC.RoleId.Sidekick, TeamRoleType.Neutral);
        public static IntroDate TeleporterIntro = new IntroDate("Teleporter", RoleClass.Teleporter.color, 2, CustomRPC.RoleId.Teleporter, TeamRoleType.Impostor);
        public static IntroDate SpiritMediumIntro = new IntroDate("SpiritMedium", RoleClass.SpiritMedium.color, 1, CustomRPC.RoleId.SpiritMedium);
        public static IntroDate SpeedBoosterIntro = new IntroDate("SpeedBooster", RoleClass.SpeedBooster.color, 2, CustomRPC.RoleId.SpeedBooster);
        public static IntroDate EvilSpeedBoosterIntro = new IntroDate("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 4, CustomRPC.RoleId.EvilSpeedBooster, TeamRoleType.Impostor);
        public static IntroDate TaskerIntro = new IntroDate("Tasker", RoleClass.Tasker.color, 2, CustomRPC.RoleId.Tasker, TeamRoleType.Impostor);
        public static IntroDate DoorrIntro = new IntroDate("Doorr", RoleClass.Doorr.color, 2, CustomRPC.RoleId.Doorr);
        public static IntroDate EvilDoorrIntro = new IntroDate("EvilDoorr", RoleClass.EvilDoorr.color, 3, CustomRPC.RoleId.EvilDoorr, TeamRoleType.Impostor);
        public static IntroDate ShielderIntro = new IntroDate("Shielder", RoleClass.Shielder.color, 3, CustomRPC.RoleId.Shielder);
        public static IntroDate FreezerIntro = new IntroDate("Freezer", RoleClass.Freezer.color, 3, CustomRPC.RoleId.Freezer, TeamRoleType.Impostor);
        public static IntroDate SpeederIntro = new IntroDate("Speeder", RoleClass.Speeder.color, 2, CustomRPC.RoleId.Speeder, TeamRoleType.Impostor);
        public static IntroDate GuesserIntro = new IntroDate("Guesser", RoleClass.Guesser.color, 2, CustomRPC.RoleId.Guesser);
        public static IntroDate EvilGuesserIntro = new IntroDate("EvilGuesser", RoleClass.EvilGuesser.color, 1, CustomRPC.RoleId.EvilGuesser, TeamRoleType.Impostor);
        public static IntroDate VultureIntro = new IntroDate("Vulture", RoleClass.Vulture.color, 1, CustomRPC.RoleId.Vulture, TeamRoleType.Neutral);
        public static IntroDate NiceScientistIntro = new IntroDate("NiceScientist", RoleClass.NiceScientist.color, 2, CustomRPC.RoleId.NiceScientist);
        public static IntroDate ClergymanIntro = new IntroDate("Clergyman", RoleClass.Clergyman.color, 2, CustomRPC.RoleId.Clergyman);
        public static IntroDate MadMateIntro = new IntroDate("MadMate", RoleClass.MadMate.color, 1, CustomRPC.RoleId.MadMate);
        public static IntroDate BaitIntro = new IntroDate("Bait", RoleClass.Bait.color, 1, CustomRPC.RoleId.Bait);
        public static IntroDate HomeSecurityGuardIntro = new IntroDate("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, CustomRPC.RoleId.HomeSecurityGuard);
        public static IntroDate StuntManIntro = new IntroDate("StuntMan", RoleClass.StuntMan.color, 1, CustomRPC.RoleId.StuntMan);
        public static IntroDate MovingIntro = new IntroDate("Moving", RoleClass.Moving.color, 1, CustomRPC.RoleId.Moving);
        public static IntroDate OpportunistIntro = new IntroDate("Opportunist", RoleClass.Opportunist.color, 2, CustomRPC.RoleId.Opportunist, TeamRoleType.Neutral);
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
        public static IntroDate SerialKillerIntro = new IntroDate("SerialKiller", RoleClass.SerialKiller.color, 1, CustomRPC.RoleId.SerialKiller, TeamRoleType.Impostor);
        public static IntroDate OverKillerIntro = new IntroDate("OverKiller", RoleClass.OverKiller.color, 1, CustomRPC.RoleId.OverKiller, TeamRoleType.Impostor);
        public static IntroDate LevelingerIntro = new IntroDate("Levelinger", RoleClass.Levelinger.color, 1, CustomRPC.RoleId.Levelinger, TeamRoleType.Impostor);
        public static IntroDate EvilMovingIntro = new IntroDate("EvilMoving", RoleClass.EvilMoving.color, 1, CustomRPC.RoleId.EvilMoving, TeamRoleType.Impostor);
        public static IntroDate AmnesiacIntro = new IntroDate("Amnesiac", RoleClass.Amnesiac.color, 1, CustomRPC.RoleId.Amnesiac, TeamRoleType.Neutral);
        public static IntroDate SideKillerIntro = new IntroDate("SideKiller", RoleClass.SideKiller.color, 1, CustomRPC.RoleId.SideKiller, TeamRoleType.Impostor);
        public static IntroDate MadKillerIntro = new IntroDate("MadKiller", RoleClass.SideKiller.color, 1, CustomRPC.RoleId.MadKiller, TeamRoleType.Impostor);
        public static IntroDate SurvivorIntro = new IntroDate("Survivor", RoleClass.Survivor.color, 1, CustomRPC.RoleId.Survivor, TeamRoleType.Impostor);
        public static IntroDate MadMayorIntro = new IntroDate("MadMayor", RoleClass.MadMayor.color, 1, CustomRPC.RoleId.MadMayor);
        public static IntroDate NiceHawkIntro = new IntroDate("NiceHawk", RoleClass.NiceHawk.color, 2, CustomRPC.RoleId.NiceHawk);
        public static IntroDate BakeryIntro = new IntroDate("Bakery", RoleClass.Bakery.color, 1, CustomRPC.RoleId.Bakery);
        public static IntroDate MadStuntManIntro = new IntroDate("MadStuntMan", RoleClass.MadStuntMan.color, 1, CustomRPC.RoleId.MadStuntMan, TeamRoleType.Impostor);
        public static IntroDate MadHawkIntro = new IntroDate("MadHawk", RoleClass.MadHawk.color, 1, CustomRPC.RoleId.MadHawk);
        public static IntroDate MadJesterIntro = new IntroDate("MadJester", RoleClass.MadJester.color, 1, CustomRPC.RoleId.MadJester);
        public static IntroDate FalseChargesIntro = new IntroDate("FalseCharges", RoleClass.FalseCharges.color, 1, CustomRPC.RoleId.FalseCharges, TeamRoleType.Neutral);
        public static IntroDate NiceTeleporterIntro = new IntroDate("NiceTeleporter", RoleClass.NiceTeleporter.color, 1, CustomRPC.RoleId.NiceTeleporter);
        public static IntroDate CelebrityIntro = new IntroDate("Celebrity", RoleClass.Celebrity.color, 1, CustomRPC.RoleId.Celebrity);
        public static IntroDate NocturnalityIntro = new IntroDate("Nocturnality", RoleClass.Nocturnality.color, 1, CustomRPC.RoleId.Nocturnality);
        public static IntroDate ObserverIntro = new IntroDate("Observer", RoleClass.Observer.color, 1, CustomRPC.RoleId.Observer);
        public static IntroDate VampireIntro = new IntroDate("Vampire", RoleClass.Vampire.color, 1, CustomRPC.RoleId.Vampire, TeamRoleType.Impostor);
        public static IntroDate FoxIntro = new IntroDate("Fox", RoleClass.Fox.color, 1, CustomRPC.RoleId.Fox, TeamRoleType.Neutral);
        public static IntroDate DarkKillerIntro = new IntroDate("DarkKiller", RoleClass.DarkKiller.color, 1, CustomRPC.RoleId.DarkKiller, TeamRoleType.Impostor);
        public static IntroDate SeerIntro = new IntroDate("Seer", RoleClass.Seer.color, 1, CustomRPC.RoleId.Seer);
        public static IntroDate MadSeerIntro = new IntroDate("MadSeer", RoleClass.MadSeer.color, 1, CustomRPC.RoleId.MadSeer);
        public static IntroDate EvilSeerIntro = new IntroDate("EvilSeer", RoleClass.EvilSeer.color, 1, CustomRPC.RoleId.EvilSeer, TeamRoleType.Impostor);
        public static IntroDate RemoteSheriffIntro = new IntroDate("RemoteSheriff", RoleClass.RemoteSheriff.color, 1, CustomRPC.RoleId.RemoteSheriff);
        public static IntroDate TeleportingJackalIntro = new IntroDate("TeleportingJackal", RoleClass.TeleportingJackal.color, 1, CustomRPC.RoleId.TeleportingJackal);
        public static IntroDate MadMakerIntro = new IntroDate("MadMaker", RoleClass.MadMaker.color, 1, CustomRPC.RoleId.MadMaker);
        public static IntroDate DemonIntro = new IntroDate("Demon", RoleClass.Demon.color, 1, CustomRPC.RoleId.Demon, TeamRoleType.Neutral);
        public static IntroDate TaskManagerIntro = new IntroDate("TaskManager", RoleClass.TaskManager.color, 1, CustomRPC.RoleId.TaskManager);
        public static IntroDate SeerFriendsIntro = new IntroDate("SeerFriends", RoleClass.SeerFriends.color, 1, CustomRPC.RoleId.SeerFriends, TeamRoleType.Neutral);
        public static IntroDate JackalSeerIntro = new IntroDate("JackalSeer", RoleClass.JackalSeer.color, 1, CustomRPC.RoleId.JackalSeer, TeamRoleType.Neutral);
        public static IntroDate SidekickSeerIntro = new IntroDate("SidekickSeer", RoleClass.JackalSeer.color, 1, CustomRPC.RoleId.SidekickSeer, TeamRoleType.Neutral);
        public static IntroDate AssassinIntro = new IntroDate("Assassin", RoleClass.Assassin.color, 1, CustomRPC.RoleId.Assassin);
        public static IntroDate MarineIntro = new IntroDate("Marine", RoleClass.Marine.color, 1, CustomRPC.RoleId.Marine);
        public static IntroDate ArsonistIntro = new IntroDate("Arsonist", RoleClass.Arsonist.color, 1, CustomRPC.RoleId.Arsonist, TeamRoleType.Neutral);
        public static IntroDate ChiefIntro = new IntroDate("Chief", RoleClass.Chief.color, 1, CustomRPC.RoleId.Chief);
        public static IntroDate CleanerIntro = new IntroDate("Cleaner", RoleClass.Cleaner.color, 1, CustomRPC.RoleId.Cleaner, TeamRoleType.Impostor);
        public static IntroDate MadCleanerIntro = new IntroDate("MadCleaner", RoleClass.MadCleaner.color, 1, CustomRPC.RoleId.MadCleaner);
        public static IntroDate SamuraiIntro = new IntroDate("Samurai", RoleClass.Samurai.color, 1, CustomRPC.RoleId.Samurai, TeamRoleType.Impostor);
        public static IntroDate MayorFriendsIntro = new IntroDate("MayorFriends", RoleClass.MayorFriends.color, 1, CustomRPC.RoleId.MayorFriends, TeamRoleType.Neutral);
        public static IntroDate VentMakerIntro = new IntroDate("VentMaker", RoleClass.VentMaker.color, 1, CustomRPC.RoleId.VentMaker, TeamRoleType.Impostor);
        public static IntroDate GhostMechanicIntro = new IntroDate("GhostMechanic", RoleClass.GhostMechanic.color, 1, CustomRPC.RoleId.GhostMechanic, TeamRoleType.Crewmate, true);
        public static IntroDate EvilHackerIntro = new IntroDate("EvilHacker", RoleClass.EvilHacker.color, 1, CustomRPC.RoleId.EvilHacker, TeamRoleType.Impostor);
        public static IntroDate HauntedWolfIntro = new IntroDate("HauntedWolf", RoleClass.HauntedWolf.color, 1, CustomRPC.RoleId.HauntedWolf);
        //イントロオブジェ
    }
}