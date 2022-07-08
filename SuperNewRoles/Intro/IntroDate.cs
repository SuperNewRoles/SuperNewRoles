using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Intro
{
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
            System.Random r1 = new();
            return ModTranslation.getString(name + "Title" + r1.Next(1, num + 1).ToString());
        }

        public static IntroDate CrewmateIntro = new("CrewMate", Color.white, 1, CustomRPC.RoleId.DefaultRole);
        public static IntroDate ImpostorIntro = new("Impostor", RoleClass.ImpostorRed, 1, CustomRPC.RoleId.DefaultRole, TeamRoleType.Impostor);
        public static IntroDate SoothSayerIntro = new("SoothSayer", RoleClass.SoothSayer.color, 1, CustomRPC.RoleId.SoothSayer);
        public static IntroDate JesterIntro = new("Jester", RoleClass.Jester.color, 1, CustomRPC.RoleId.Jester, TeamRoleType.Neutral);
        public static IntroDate LighterIntro = new("Lighter", RoleClass.Lighter.color, 1, CustomRPC.RoleId.Lighter);
        public static IntroDate EvilLighterIntro = new("EvilLighter", RoleClass.EvilLighter.color, 2, CustomRPC.RoleId.EvilLighter, TeamRoleType.Impostor);
        public static IntroDate EvilScientist = new("EvilScientist", RoleClass.EvilScientist.color, 2, CustomRPC.RoleId.EvilScientist, TeamRoleType.Impostor);
        public static IntroDate SheriffIntro = new("Sheriff", RoleClass.Sheriff.color, 2, CustomRPC.RoleId.Sheriff);
        public static IntroDate MeetingSheriffIntro = new("MeetingSheriff", RoleClass.MeetingSheriff.color, 4, CustomRPC.RoleId.MeetingSheriff);
        public static IntroDate JackalIntro = new("Jackal", RoleClass.Jackal.color, 3, CustomRPC.RoleId.Jackal, TeamRoleType.Neutral);
        public static IntroDate SidekickIntro = new("Sidekick", RoleClass.Jackal.color, 1, CustomRPC.RoleId.Sidekick, TeamRoleType.Neutral);
        public static IntroDate TeleporterIntro = new("Teleporter", RoleClass.Teleporter.color, 2, CustomRPC.RoleId.Teleporter, TeamRoleType.Impostor);
        public static IntroDate SpiritMediumIntro = new("SpiritMedium", RoleClass.SpiritMedium.color, 1, CustomRPC.RoleId.SpiritMedium);
        public static IntroDate SpeedBoosterIntro = new("SpeedBooster", RoleClass.SpeedBooster.color, 2, CustomRPC.RoleId.SpeedBooster);
        public static IntroDate EvilSpeedBoosterIntro = new("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 4, CustomRPC.RoleId.EvilSpeedBooster, TeamRoleType.Impostor);
        public static IntroDate TaskerIntro = new("Tasker", RoleClass.Tasker.color, 2, CustomRPC.RoleId.Tasker, TeamRoleType.Impostor);
        public static IntroDate DoorrIntro = new("Doorr", RoleClass.Doorr.color, 2, CustomRPC.RoleId.Doorr);
        public static IntroDate EvilDoorrIntro = new("EvilDoorr", RoleClass.EvilDoorr.color, 3, CustomRPC.RoleId.EvilDoorr, TeamRoleType.Impostor);
        public static IntroDate ShielderIntro = new("Shielder", RoleClass.Shielder.color, 3, CustomRPC.RoleId.Shielder);
        public static IntroDate FreezerIntro = new("Freezer", RoleClass.Freezer.color, 3, CustomRPC.RoleId.Freezer, TeamRoleType.Impostor);
        public static IntroDate SpeederIntro = new("Speeder", RoleClass.Speeder.color, 2, CustomRPC.RoleId.Speeder, TeamRoleType.Impostor);
        public static IntroDate GuesserIntro = new("Guesser", RoleClass.Guesser.color, 2, CustomRPC.RoleId.Guesser);
        public static IntroDate EvilGuesserIntro = new("EvilGuesser", RoleClass.EvilGuesser.color, 1, CustomRPC.RoleId.EvilGuesser, TeamRoleType.Impostor);
        public static IntroDate VultureIntro = new("Vulture", RoleClass.Vulture.color, 1, CustomRPC.RoleId.Vulture, TeamRoleType.Neutral);
        public static IntroDate NiceScientistIntro = new("NiceScientist", RoleClass.NiceScientist.color, 2, CustomRPC.RoleId.NiceScientist);
        public static IntroDate ClergymanIntro = new("Clergyman", RoleClass.Clergyman.color, 2, CustomRPC.RoleId.Clergyman);
        public static IntroDate MadMateIntro = new("MadMate", RoleClass.MadMate.color, 1, CustomRPC.RoleId.MadMate);
        public static IntroDate BaitIntro = new("Bait", RoleClass.Bait.color, 1, CustomRPC.RoleId.Bait);
        public static IntroDate HomeSecurityGuardIntro = new("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, CustomRPC.RoleId.HomeSecurityGuard);
        public static IntroDate StuntManIntro = new("StuntMan", RoleClass.StuntMan.color, 1, CustomRPC.RoleId.StuntMan);
        public static IntroDate MovingIntro = new("Moving", RoleClass.Moving.color, 1, CustomRPC.RoleId.Moving);
        public static IntroDate OpportunistIntro = new("Opportunist", RoleClass.Opportunist.color, 2, CustomRPC.RoleId.Opportunist, TeamRoleType.Neutral);
        public static IntroDate NiceGamblerIntro = new("NiceGambler", RoleClass.NiceGambler.color, 1, CustomRPC.RoleId.NiceGambler);
        public static IntroDate EvilGamblerIntro = new("EvilGambler", RoleClass.EvilGambler.color, 1, CustomRPC.RoleId.EvilGambler, TeamRoleType.Impostor);
        public static IntroDate BestfalsechargeIntro = new("Bestfalsecharge", RoleClass.Bestfalsecharge.color, 1, CustomRPC.RoleId.Bestfalsecharge);
        public static IntroDate ResearcherIntro = new("Researcher", RoleClass.Researcher.color, 1, CustomRPC.RoleId.Researcher, TeamRoleType.Neutral);
        public static IntroDate SelfBomberIntro = new("SelfBomber", RoleClass.SelfBomber.color, 1, CustomRPC.RoleId.SelfBomber, TeamRoleType.Impostor);
        public static IntroDate GodIntro = new("God", RoleClass.God.color, 1, CustomRPC.RoleId.God, TeamRoleType.Neutral);
        public static IntroDate AllCleanerIntro = new("AllCleaner", RoleClass.AllCleaner.color, 1, CustomRPC.RoleId.AllCleaner, TeamRoleType.Impostor);
        public static IntroDate NiceNekomataIntro = new("NiceNekomata", RoleClass.NiceNekomata.color, 3, CustomRPC.RoleId.NiceNekomata);
        public static IntroDate EvilNekomataIntro = new("EvilNekomata", RoleClass.EvilNekomata.color, 1, CustomRPC.RoleId.EvilNekomata, TeamRoleType.Impostor);
        public static IntroDate JackalFriendsIntro = new("JackalFriends", RoleClass.JackalFriends.color, 2, CustomRPC.RoleId.JackalFriends);
        public static IntroDate DoctorIntro = new("Doctor", RoleClass.Doctor.color, 1, CustomRPC.RoleId.Doctor);
        public static IntroDate CountChangerIntro = new("CountChanger", RoleClass.CountChanger.color, 2, CustomRPC.RoleId.CountChanger, TeamRoleType.Impostor);
        public static IntroDate PursuerIntro = new("Pursuer", RoleClass.Pursuer.color, 3, CustomRPC.RoleId.Pursuer, TeamRoleType.Impostor);
        public static IntroDate MinimalistIntro = new("Minimalist", RoleClass.Minimalist.color, 2, CustomRPC.RoleId.Minimalist, TeamRoleType.Impostor);
        public static IntroDate HawkIntro = new("Hawk", RoleClass.Hawk.color, 1, CustomRPC.RoleId.Hawk, TeamRoleType.Impostor);
        public static IntroDate EgoistIntro = new("Egoist", RoleClass.Egoist.color, 1, CustomRPC.RoleId.Egoist, TeamRoleType.Neutral);
        public static IntroDate NiceRedRidingHoodIntro = new("NiceRedRidingHood", RoleClass.NiceRedRidingHood.color, 1, CustomRPC.RoleId.NiceRedRidingHood);
        public static IntroDate EvilEraserIntro = new("EvilEraser", RoleClass.EvilEraser.color, 1, CustomRPC.RoleId.EvilEraser, TeamRoleType.Impostor);
        public static IntroDate WorkpersonIntro = new("Workperson", RoleClass.Workperson.color, 1, CustomRPC.RoleId.Workperson, TeamRoleType.Neutral);
        public static IntroDate MagazinerIntro = new("Magaziner", RoleClass.Magaziner.color, 1, CustomRPC.RoleId.Magaziner, TeamRoleType.Impostor);
        public static IntroDate MayorIntro = new("Mayor", RoleClass.Mayor.color, 1, CustomRPC.RoleId.Mayor);
        public static IntroDate trueloverIntro = new("truelover", RoleClass.truelover.color, 1, CustomRPC.RoleId.truelover, TeamRoleType.Neutral);
        public static IntroDate TechnicianIntro = new("Technician", RoleClass.Technician.color, 1, CustomRPC.RoleId.Technician);
        public static IntroDate SerialKillerIntro = new("SerialKiller", RoleClass.SerialKiller.color, 1, CustomRPC.RoleId.SerialKiller, TeamRoleType.Impostor);
        public static IntroDate OverKillerIntro = new("OverKiller", RoleClass.OverKiller.color, 1, CustomRPC.RoleId.OverKiller, TeamRoleType.Impostor);
        public static IntroDate LevelingerIntro = new("Levelinger", RoleClass.Levelinger.color, 1, CustomRPC.RoleId.Levelinger, TeamRoleType.Impostor);
        public static IntroDate EvilMovingIntro = new("EvilMoving", RoleClass.EvilMoving.color, 1, CustomRPC.RoleId.EvilMoving, TeamRoleType.Impostor);
        public static IntroDate AmnesiacIntro = new("Amnesiac", RoleClass.Amnesiac.color, 1, CustomRPC.RoleId.Amnesiac, TeamRoleType.Neutral);
        public static IntroDate SideKillerIntro = new("SideKiller", RoleClass.SideKiller.color, 1, CustomRPC.RoleId.SideKiller, TeamRoleType.Impostor);
        public static IntroDate MadKillerIntro = new("MadKiller", RoleClass.SideKiller.color, 1, CustomRPC.RoleId.MadKiller, TeamRoleType.Impostor);
        public static IntroDate SurvivorIntro = new("Survivor", RoleClass.Survivor.color, 1, CustomRPC.RoleId.Survivor, TeamRoleType.Impostor);
        public static IntroDate MadMayorIntro = new("MadMayor", RoleClass.MadMayor.color, 1, CustomRPC.RoleId.MadMayor);
        public static IntroDate NiceHawkIntro = new("NiceHawk", RoleClass.NiceHawk.color, 2, CustomRPC.RoleId.NiceHawk);
        public static IntroDate BakeryIntro = new("Bakery", RoleClass.Bakery.color, 1, CustomRPC.RoleId.Bakery);
        public static IntroDate MadStuntManIntro = new("MadStuntMan", RoleClass.MadStuntMan.color, 1, CustomRPC.RoleId.MadStuntMan, TeamRoleType.Impostor);
        public static IntroDate MadHawkIntro = new("MadHawk", RoleClass.MadHawk.color, 1, CustomRPC.RoleId.MadHawk);
        public static IntroDate MadJesterIntro = new("MadJester", RoleClass.MadJester.color, 1, CustomRPC.RoleId.MadJester);
        public static IntroDate FalseChargesIntro = new("FalseCharges", RoleClass.FalseCharges.color, 1, CustomRPC.RoleId.FalseCharges, TeamRoleType.Neutral);
        public static IntroDate NiceTeleporterIntro = new("NiceTeleporter", RoleClass.NiceTeleporter.color, 1, CustomRPC.RoleId.NiceTeleporter);
        public static IntroDate CelebrityIntro = new("Celebrity", RoleClass.Celebrity.color, 1, CustomRPC.RoleId.Celebrity);
        public static IntroDate NocturnalityIntro = new("Nocturnality", RoleClass.Nocturnality.color, 1, CustomRPC.RoleId.Nocturnality);
        public static IntroDate ObserverIntro = new("Observer", RoleClass.Observer.color, 1, CustomRPC.RoleId.Observer);
        public static IntroDate VampireIntro = new("Vampire", RoleClass.Vampire.color, 1, CustomRPC.RoleId.Vampire, TeamRoleType.Impostor);
        public static IntroDate FoxIntro = new("Fox", RoleClass.Fox.color, 1, CustomRPC.RoleId.Fox, TeamRoleType.Neutral);
        public static IntroDate DarkKillerIntro = new("DarkKiller", RoleClass.DarkKiller.color, 1, CustomRPC.RoleId.DarkKiller, TeamRoleType.Impostor);
        public static IntroDate SeerIntro = new("Seer", RoleClass.Seer.color, 1, CustomRPC.RoleId.Seer);
        public static IntroDate MadSeerIntro = new("MadSeer", RoleClass.MadSeer.color, 1, CustomRPC.RoleId.MadSeer);
        public static IntroDate EvilSeerIntro = new("EvilSeer", RoleClass.EvilSeer.color, 1, CustomRPC.RoleId.EvilSeer, TeamRoleType.Impostor);
        public static IntroDate RemoteSheriffIntro = new("RemoteSheriff", RoleClass.RemoteSheriff.color, 1, CustomRPC.RoleId.RemoteSheriff);
        public static IntroDate TeleportingJackalIntro = new("TeleportingJackal", RoleClass.TeleportingJackal.color, 1, CustomRPC.RoleId.TeleportingJackal);
        public static IntroDate MadMakerIntro = new("MadMaker", RoleClass.MadMaker.color, 1, CustomRPC.RoleId.MadMaker);
        public static IntroDate DemonIntro = new("Demon", RoleClass.Demon.color, 1, CustomRPC.RoleId.Demon, TeamRoleType.Neutral);
        public static IntroDate TaskManagerIntro = new("TaskManager", RoleClass.TaskManager.color, 1, CustomRPC.RoleId.TaskManager);
        public static IntroDate SeerFriendsIntro = new("SeerFriends", RoleClass.SeerFriends.color, 1, CustomRPC.RoleId.SeerFriends);
        public static IntroDate JackalSeerIntro = new("JackalSeer", RoleClass.JackalSeer.color, 1, CustomRPC.RoleId.JackalSeer, TeamRoleType.Neutral);
        public static IntroDate SidekickSeerIntro = new("SidekickSeer", RoleClass.JackalSeer.color, 1, CustomRPC.RoleId.SidekickSeer, TeamRoleType.Neutral);
        public static IntroDate AssassinIntro = new("Assassin", RoleClass.Assassin.color, 1, CustomRPC.RoleId.Assassin);
        public static IntroDate MarineIntro = new("Marine", RoleClass.Marine.color, 1, CustomRPC.RoleId.Marine);
        public static IntroDate ArsonistIntro = new("Arsonist", RoleClass.Arsonist.color, 1, CustomRPC.RoleId.Arsonist, TeamRoleType.Neutral);
        public static IntroDate ChiefIntro = new("Chief", RoleClass.Chief.color, 1, CustomRPC.RoleId.Chief);
        public static IntroDate CleanerIntro = new("Cleaner", RoleClass.Cleaner.color, 1, CustomRPC.RoleId.Cleaner, TeamRoleType.Impostor);
        public static IntroDate MadCleanerIntro = new("MadCleaner", RoleClass.MadCleaner.color, 1, CustomRPC.RoleId.MadCleaner);
        public static IntroDate SamuraiIntro = new("Samurai", RoleClass.Samurai.color, 1, CustomRPC.RoleId.Samurai, TeamRoleType.Impostor);
        public static IntroDate MayorFriendsIntro = new("MayorFriends", RoleClass.MayorFriends.color, 1, CustomRPC.RoleId.MayorFriends);
        public static IntroDate VentMakerIntro = new("VentMaker", RoleClass.VentMaker.color, 1, CustomRPC.RoleId.VentMaker, TeamRoleType.Impostor);
        public static IntroDate GhostMechanicIntro = new("GhostMechanic", RoleClass.GhostMechanic.color, 1, CustomRPC.RoleId.GhostMechanic, TeamRoleType.Crewmate, true);
        public static IntroDate EvilHackerIntro = new("EvilHacker", RoleClass.EvilHacker.color, 1, CustomRPC.RoleId.EvilHacker, TeamRoleType.Impostor);
        public static IntroDate HauntedWolfIntro = new("HauntedWolf", RoleClass.HauntedWolf.color, 1, CustomRPC.RoleId.HauntedWolf, TeamRoleType.Crewmate);
        public static IntroDate PositionSwapperIntro = new("PositionSwapper", RoleClass.PositionSwapper.color, 1, CustomRPC.RoleId.PositionSwapper, TeamRoleType.Impostor);
        public static IntroDate TunaIntro = new("Tuna", RoleClass.Tuna.color, 1, CustomRPC.RoleId.Tuna, TeamRoleType.Neutral);
        public static IntroDate MafiaIntro = new("Mafia", RoleClass.Mafia.color, 1, CustomRPC.RoleId.Mafia, TeamRoleType.Impostor);
        public static IntroDate BlackCatIntro = new("BlackCat", RoleClass.BlackCat.color, 1, CustomRPC.RoleId.BlackCat);
        public static IntroDate SecretlyKillerIntro = new("SecretlyKiller", RoleClass.SecretlyKiller.color, 1, CustomRPC.RoleId.SecretlyKiller, TeamRoleType.Impostor);
        public static IntroDate SpyIntro = new("Spy", RoleClass.Spy.color, 1, CustomRPC.RoleId.Spy, TeamRoleType.Crewmate);
        public static IntroDate KunoichiIntro = new("Kunoichi", RoleClass.Kunoichi.color, 1, CustomRPC.RoleId.Kunoichi, TeamRoleType.Impostor);
        public static IntroDate DoubleKillerIntro = new IntroDate("DoubleKiller", RoleClass.DoubleKiller.color, 1, CustomRPC.RoleId.DoubleKiller, TeamRoleType.Impostor);
        //イントロオブジェ
    }
}
