using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class TeamData
{
    public string NameKey;
    public Color color;
    public Color BackGround;
    public List<RoleId> RoleIds;

    TeamData(string NameKey, Color color, Color BackGround, List<RoleId> RoleId)
    {
        this.color = color;
        this.BackGround = BackGround;
        this.NameKey = NameKey;
        RoleIds = RoleId;
    }
    public static TeamData VultureTeam = new("Test", Color.black, Color.yellow, new List<RoleId> { RoleId.Sheriff });
}
public class IntroData
{
    public static Dictionary<RoleId, IntroData> Intros = new();
    public static Dictionary<RoleId, IntroData> IntroDataCache = new();
    public static List<IntroData> GhostRoleData = new();
    public static List<Dictionary<RoleId, string>> IntroGroup = new()
    {
        new() {{RoleId.Seer,"SeerCombIntro1"},{RoleId.EvilSeer,"EvilSeerCombIntro1"}},
        new() {{RoleId.NiceMechanic,"NiceMechanicCombIntro1"},{RoleId.EvilMechanic,"EvilMechanicCombIntro1"}},
        new() {{RoleId.Bakery,"BakeryCombIntro1"},{RoleId.HamburgerShop,"BakeryCombIntro1"}},
        new() {{RoleId.Sheriff,"SheriffCombIntroDearJackal"},{RoleId.Jackal,"JackalCombIntroDearSheriff"}},
        new() {{RoleId.SuicidalIdeation,"SuicidalIdeationCombIntroDearSuicideWisher"},{RoleId.SuicideWisher,"SuicideWisherCombIntroDearSuicidalIdeation"}},
        new() {{RoleId.Survivor,"SurvivorCombIntroDearSuicideWisher"},{RoleId.SuicideWisher,"SuicideWisherCombIntroSurvivor"}},
        new() {{RoleId.Nun,"NunCombIntroDearPteranodon"},{RoleId.Pteranodon,"PteranodonCombIntroDearNun"}},
        new() {{RoleId.MedicalTechnologist,"MedicalTechnologistCombIntroToDoctor"}, {RoleId.Doctor,"DoctorCombIntro1"}},
        new() {{RoleId.MedicalTechnologist,"MedicalTechnologistCombIntroToPoliceSurgeon"}, {RoleId.PoliceSurgeon,"PoliceSurgeonCombIntro1"}},
        new() {{RoleId.Jammer,"JammerCombIntroDearEvilScientist"},{RoleId.EvilScientist,"EvilScientistDearJammerCombIntro"}}
    };
    public string NameKey;
    public string Name;
    public short TitleNum;
    public string TitleDesc
    {
        get
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                return _titleDesc;
            }
            return GetTitle(NameKey, TitleNum, RoleId);
        }
    }
    public string _titleDesc;
    public Color color;
    public RoleId RoleId;
    public string Description;
    public TeamRoleType Team;
    public TeamType TeamType;
    public bool IsGhostRole;
    public RoleTypes IntroSound;
    IntroData(string NameKey, Color color, Int16 TitleNum, RoleId RoleId, TeamRoleType team = TeamRoleType.Crewmate, TeamType teamType = TeamType.Crewmate, bool IsGhostRole = false, RoleTypes IntroSound = RoleTypes.Crewmate)
    {
        this.color = color;
        this.NameKey = NameKey;
        this.Name = ModTranslation.GetString(NameKey + "Name");
        this.RoleId = RoleId;
        this.TitleNum = TitleNum;
        this._titleDesc = GetTitle(NameKey, TitleNum, RoleId);
        this.Description = ModTranslation.GetString(NameKey + "Description");
        this.Team = team;
        if (teamType == TeamType.Crewmate && team == TeamRoleType.Crewmate) this.TeamType = TeamType.Crewmate;
        else if (teamType == TeamType.Crewmate && (int)team != (int)teamType) this.TeamType = (TeamType)team;
        else this.TeamType = teamType;
        this.IsGhostRole = IsGhostRole;
        this.IntroSound = IntroSound;

        if (IsGhostRole)
        {
            GhostRoleData.Add(this);
        }
        if (!Intros.TryAdd(RoleId, this))
            Logger.Info(RoleId.ToString() + "が追加されませんでした。");
    }
    [Obsolete]
    public static IntroData GetIntrodata(RoleId RoleId, PlayerControl p = null, bool IsImpostorReturn = false)
    {
        if (RoleId is RoleId.DefaultRole)
        {
            if (IsImpostorReturn || p.IsImpostor())
                return ImpostorIntro;
            else
                return CrewmateIntro;
        }
        else if (RoleId is RoleId.Jumbo)
        {
            return p == null ? JumboIntro : p.IsImpostor() ? EvilJumboIntro : NiceJumboIntro;
        }
        if (IntroDataCache.TryGetValue(RoleId, out IntroData introData))
            return introData;
        else
        {
            if (!Intros.TryGetValue(RoleId, out IntroData data))
                data = CrewmateIntro;
            IntroDataCache[RoleId] = data;
            return data;
        }
    }
    public static CustomRoleOption GetOption(RoleId roleId)
    {
        CustomRoleOption.RoleOptions.TryGetValue(roleId, out CustomRoleOption opt);
        return opt;
    }
    public static string GetTitle(string name, Int16 num, RoleId RoleId = RoleId.DefaultRole)
    {
        if (RoleId is not RoleId.DefaultRole)
        {
            for (int i = 0; i < IntroGroup.Count; i++)
            {
                Dictionary<RoleId, string> list = IntroGroup[i];
                bool Flag = true;
                foreach (KeyValuePair<RoleId, string> item in list)
                {
                    if (GetOption(item.Key) == null) continue;
                    if (GetOption(item.Key).GetSelection() is 0)
                    {
                        Flag = false;
                        break;
                    }
                }
                if (Flag && list.ContainsKey(RoleId)) return ModTranslation.GetString(list[RoleId]);
            }
        }
        System.Random r1 = new();
        return ModTranslation.GetString(name + "Title" + r1.Next(1, num + 1).ToString());
    }
    public static void PlayIntroSound(RoleId RoleId)
    {
        RoleBase roleBase = PlayerControl.LocalPlayer.GetRoleBase();
        AudioClip clip = null;
        if (roleBase != null)
        {
            clip = roleBase.GetIntroAudioClip();
        }
        else
        {
            var Info = GetIntrodata(RoleId, PlayerControl.LocalPlayer);
            clip = RoleManager.Instance.GetRole(Info.IntroSound)?.IntroSound;
        }

        PlayerControl.LocalPlayer.Data.Role.IntroSound = clip;
        SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1);
    }


    public static IntroData CrewmateIntro = new("Crewmate", Color.white, 1, RoleId.DefaultRole);
    public static IntroData ImpostorIntro = new("Impostor", RoleClass.ImpostorRed, 1, RoleId.DefaultRole, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SoothSayerIntro = new("SoothSayer", RoleClass.SoothSayer.color, 1, RoleId.SoothSayer);
    public static IntroData JesterIntro = new("Jester", RoleClass.Jester.color, 1, RoleId.Jester, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData LighterIntro = new("Lighter", RoleClass.Lighter.color, 1, RoleId.Lighter);
    public static IntroData EvilLighterIntro = new("EvilLighter", RoleClass.EvilLighter.color, 2, RoleId.EvilLighter, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SheriffIntro = new("Sheriff", RoleClass.Sheriff.color, 2, RoleId.Sheriff, IntroSound: RoleTypes.Engineer);
    public static IntroData MeetingSheriffIntro = new("MeetingSheriff", RoleClass.MeetingSheriff.color, 4, RoleId.MeetingSheriff, IntroSound: RoleTypes.Engineer);
    public static IntroData JackalIntro = new("Jackal", RoleClass.Jackal.color, 3, RoleId.Jackal, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SidekickIntro = new("Sidekick", RoleClass.Jackal.color, 1, RoleId.Sidekick, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData TeleporterIntro = new("Teleporter", RoleClass.Teleporter.color, 2, RoleId.Teleporter, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SpiritMediumIntro = new("SpiritMedium", RoleClass.SpiritMedium.color, 1, RoleId.SpiritMedium);
    public static IntroData SpeedBoosterIntro = new("SpeedBooster", RoleClass.SpeedBooster.color, 2, RoleId.SpeedBooster);
    public static IntroData EvilSpeedBoosterIntro = new("EvilSpeedBooster", RoleClass.EvilSpeedBooster.color, 4, RoleId.EvilSpeedBooster, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData TaskerIntro = new("Tasker", RoleClass.Tasker.color, 2, RoleId.Tasker, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData DoorrIntro = new("Doorr", RoleClass.Doorr.color, 2, RoleId.Doorr);
    public static IntroData EvilDoorrIntro = new("EvilDoorr", RoleClass.EvilDoorr.color, 3, RoleId.EvilDoorr, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData ShielderIntro = new("Shielder", RoleClass.Shielder.color, 3, RoleId.Shielder);
    public static IntroData FreezerIntro = new("Freezer", RoleClass.Freezer.color, 3, RoleId.Freezer, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SpeederIntro = new("Speeder", RoleClass.Speeder.color, 2, RoleId.Speeder, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData VultureIntro = new("Vulture", RoleClass.Vulture.color, 1, RoleId.Vulture, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData ClergymanIntro = new("Clergyman", RoleClass.Clergyman.color, 2, RoleId.Clergyman);
    public static IntroData MadmateIntro = new("Madmate", RoleClass.Madmate.color, 1, RoleId.Madmate, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData BaitIntro = new("Bait", RoleClass.Bait.color, 1, RoleId.Bait);
    public static IntroData HomeSecurityGuardIntro = new("HomeSecurityGuard", RoleClass.HomeSecurityGuard.color, 1, RoleId.HomeSecurityGuard);
    public static IntroData StuntManIntro = new("StuntMan", RoleClass.StuntMan.color, 1, RoleId.StuntMan);
    public static IntroData MovingIntro = new("Moving", RoleClass.Moving.color, 1, RoleId.Moving, IntroSound: RoleTypes.Engineer);
    public static IntroData OpportunistIntro = new("Opportunist", RoleClass.Opportunist.color, 2, RoleId.Opportunist, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData NiceGamblerIntro = new("NiceGambler", RoleClass.NiceGambler.color, 1, RoleId.NiceGambler);
    public static IntroData EvilGamblerIntro = new("EvilGambler", RoleClass.EvilGambler.color, 1, RoleId.EvilGambler, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData ResearcherIntro = new("Researcher", RoleClass.Researcher.color, 1, RoleId.Researcher, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SelfBomberIntro = new("SelfBomber", RoleClass.SelfBomber.color, 1, RoleId.SelfBomber, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData GodIntro = new("God", RoleClass.God.color, 1, RoleId.God, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData AllCleanerIntro = new("AllCleaner", RoleClass.AllCleaner.color, 1, RoleId.AllCleaner, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NiceNekomataIntro = new("NiceNekomata", RoleClass.NiceNekomata.color, 3, RoleId.NiceNekomata);
    public static IntroData EvilNekomataIntro = new("EvilNekomata", RoleClass.EvilNekomata.color, 1, RoleId.EvilNekomata, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData JackalFriendsIntro = new("JackalFriends", RoleClass.JackalFriends.color, 2, RoleId.JackalFriends, teamType: TeamType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData DoctorIntro = new("Doctor", RoleClass.Doctor.color, 1, RoleId.Doctor, IntroSound: RoleTypes.Scientist);
    public static IntroData CountChangerIntro = new("CountChanger", RoleClass.CountChanger.color, 2, RoleId.CountChanger, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData PursuerIntro = new("Pursuer", RoleClass.Pursuer.color, 3, RoleId.Pursuer, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MinimalistIntro = new("Minimalist", RoleClass.Minimalist.color, 2, RoleId.Minimalist, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData HawkIntro = new("Hawk", RoleClass.Hawk.color, 1, RoleId.Hawk, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData EgoistIntro = new("Egoist", RoleClass.Egoist.color, 1, RoleId.Egoist, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData NiceRedRidingHoodIntro = new("NiceRedRidingHood", RoleClass.NiceRedRidingHood.color, 1, RoleId.NiceRedRidingHood);
    public static IntroData EvilEraserIntro = new("EvilEraser", RoleClass.EvilEraser.color, 1, RoleId.EvilEraser, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData WorkpersonIntro = new("Workperson", RoleClass.Workperson.color, 1, RoleId.Workperson, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData MagazinerIntro = new("Magaziner", RoleClass.Magaziner.color, 1, RoleId.Magaziner, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MayorIntro = new("Mayor", RoleClass.Mayor.color, 1, RoleId.Mayor);
    public static IntroData trueloverIntro = new("truelover", RoleClass.Truelover.color, 1, RoleId.truelover, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData TechnicianIntro = new("Technician", RoleClass.Technician.color, 1, RoleId.Technician);
    public static IntroData SerialKillerIntro = new("SerialKiller", RoleClass.SerialKiller.color, 1, RoleId.SerialKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData OverKillerIntro = new("OverKiller", RoleClass.OverKiller.color, 1, RoleId.OverKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData LevelingerIntro = new("Levelinger", RoleClass.Levelinger.color, 1, RoleId.Levelinger, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData EvilMovingIntro = new("EvilMoving", RoleClass.EvilMoving.color, 1, RoleId.EvilMoving, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData AmnesiacIntro = new("Amnesiac", RoleClass.Amnesiac.color, 1, RoleId.Amnesiac, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SideKillerIntro = new("SideKiller", RoleClass.SideKiller.color, 1, RoleId.SideKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MadKillerIntro = new("MadKiller", RoleClass.SideKiller.color, 1, RoleId.MadKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SurvivorIntro = new("Survivor", RoleClass.Survivor.color, 1, RoleId.Survivor, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MadMayorIntro = new("MadMayor", RoleClass.MadMayor.color, 1, RoleId.MadMayor, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NiceHawkIntro = new("NiceHawk", RoleClass.NiceHawk.color, 2, RoleId.NiceHawk, IntroSound: RoleTypes.Crewmate);
    public static IntroData BakeryIntro = new("Bakery", RoleClass.Bakery.color, 1, RoleId.Bakery);
    public static IntroData MadStuntManIntro = new("MadStuntMan", RoleClass.MadStuntMan.color, 1, RoleId.MadStuntMan, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MadHawkIntro = new("MadHawk", RoleClass.MadHawk.color, 1, RoleId.MadHawk, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MadJesterIntro = new("MadJester", RoleClass.MadJester.color, 3, RoleId.MadJester, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData FalseChargesIntro = new("FalseCharges", RoleClass.FalseCharges.color, 1, RoleId.FalseCharges, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData NiceTeleporterIntro = new("NiceTeleporter", RoleClass.NiceTeleporter.color, 1, RoleId.NiceTeleporter);
    public static IntroData CelebrityIntro = new("Celebrity", RoleClass.Celebrity.color, 1, RoleId.Celebrity);
    public static IntroData NocturnalityIntro = new("Nocturnality", RoleClass.Nocturnality.color, 1, RoleId.Nocturnality);
    public static IntroData ObserverIntro = new("Observer", RoleClass.Observer.color, 1, RoleId.Observer);
    public static IntroData VampireIntro = new("Vampire", RoleClass.Vampire.color, 1, RoleId.Vampire, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData FoxIntro = new("Fox", RoleClass.Fox.color, 1, RoleId.Fox, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData DarkKillerIntro = new("DarkKiller", RoleClass.DarkKiller.color, 1, RoleId.DarkKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SeerIntro = new("Seer", RoleClass.Seer.color, 1, RoleId.Seer);
    public static IntroData MadSeerIntro = new("MadSeer", RoleClass.MadSeer.color, 1, RoleId.MadSeer, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData RemoteSheriffIntro = new("RemoteSheriff", RoleClass.RemoteSheriff.color, 1, RoleId.RemoteSheriff, IntroSound: RoleTypes.Engineer);
    public static IntroData TeleportingJackalIntro = new("TeleportingJackal", RoleClass.TeleportingJackal.color, 1, RoleId.TeleportingJackal, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData MadMakerIntro = new("MadMaker", RoleClass.MadMaker.color, 1, RoleId.MadMaker, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData DemonIntro = new("Demon", RoleClass.Demon.color, 1, RoleId.Demon, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData TaskManagerIntro = new("TaskManager", RoleClass.TaskManager.color, 1, RoleId.TaskManager);
    public static IntroData SeerFriendsIntro = new("SeerFriends", RoleClass.SeerFriends.color, 1, RoleId.SeerFriends, teamType: TeamType.Neutral);
    public static IntroData JackalSeerIntro = new("JackalSeer", RoleClass.JackalSeer.color, 1, RoleId.JackalSeer, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SidekickSeerIntro = new("SidekickSeer", RoleClass.JackalSeer.color, 1, RoleId.SidekickSeer, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData AssassinIntro = new("Assassin", RoleClass.Assassin.color, 1, RoleId.Assassin, TeamRoleType.Impostor, TeamType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData MarlinIntro = new("Marlin", RoleClass.Marlin.color, 1, RoleId.Marlin);
    public static IntroData ArsonistIntro = new("Arsonist", RoleClass.Arsonist.color, 1, RoleId.Arsonist, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData ChiefIntro = new("Chief", RoleClass.Chief.color, 1, RoleId.Chief, IntroSound: RoleTypes.Engineer);
    public static IntroData CleanerIntro = new("Cleaner", RoleClass.Cleaner.color, 1, RoleId.Cleaner, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MadCleanerIntro = new("MadCleaner", RoleClass.MadCleaner.color, 1, RoleId.MadCleaner, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SamuraiIntro = new("Samurai", RoleClass.Samurai.color, 1, RoleId.Samurai, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData MayorFriendsIntro = new("MayorFriends", RoleClass.MayorFriends.color, 1, RoleId.MayorFriends, teamType: TeamType.Neutral);
    public static IntroData VentMakerIntro = new("VentMaker", RoleClass.VentMaker.color, 1, RoleId.VentMaker, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData GhostMechanicIntro = new("GhostMechanic", RoleClass.GhostMechanic.color, 1, RoleId.GhostMechanic, TeamRoleType.Crewmate, IsGhostRole: true);
    public static IntroData PositionSwapperIntro = new("PositionSwapper", RoleClass.PositionSwapper.color, 1, RoleId.PositionSwapper, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData TunaIntro = new("Tuna", RoleClass.Tuna.color, 1, RoleId.Tuna, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData MafiaIntro = new("Mafia", RoleClass.Mafia.color, 1, RoleId.Mafia, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData BlackCatIntro = new("BlackCat", RoleClass.BlackCat.color, 1, RoleId.BlackCat, teamType: TeamType.Impostor);
    public static IntroData SecretlyKillerIntro = new("SecretlyKiller", RoleClass.SecretlyKiller.color, 1, RoleId.SecretlyKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SpyIntro = new("Spy", RoleClass.Spy.color, 1, RoleId.Spy);
    public static IntroData KunoichiIntro = new("Kunoichi", RoleClass.Kunoichi.color, 1, RoleId.Kunoichi, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData DoubleKillerIntro = new("DoubleKiller", RoleClass.DoubleKiller.color, 1, RoleId.DoubleKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SmasherIntro = new("Smasher", RoleClass.Smasher.color, 1, RoleId.Smasher, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SuicideWisherIntro = new("SuicideWisher", RoleClass.SuicideWisher.color, 2, RoleId.SuicideWisher, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NeetIntro = new("Neet", RoleClass.Neet.color, 1, RoleId.Neet, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData FastMakerIntro = new("FastMaker", RoleClass.FastMaker.color, 1, RoleId.FastMaker, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData ToiletFanIntro = new("ToiletFan", RoleClass.ToiletFan.color, 1, RoleId.ToiletFan);
    public static IntroData SatsumaAndImoIntro = new("SatsumaAndImo", RoleClass.SatsumaAndImo.color, 2, RoleId.SatsumaAndImo, teamType: TeamType.Neutral);
    public static IntroData EvilButtonerIntro = new("EvilButtoner", RoleClass.EvilButtoner.color, 1, RoleId.EvilButtoner, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NiceButtonerIntro = new("NiceButtoner", RoleClass.NiceButtoner.color, 1, RoleId.NiceButtoner);
    public static IntroData FinderIntro = new("Finder", RoleClass.Finder.color, 1, RoleId.Finder, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData RevolutionistIntro = new("Revolutionist", RoleClass.Revolutionist.color, 1, RoleId.Revolutionist, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData DictatorIntro = new("Dictator", RoleClass.Dictator.color, 1, RoleId.Dictator);
    public static IntroData SpelunkerIntro = new("Spelunker", RoleClass.Spelunker.color, 1, RoleId.Spelunker, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SuicidalIdeationIntro = new("SuicidalIdeation", RoleClass.SuicidalIdeation.color, 1, RoleId.SuicidalIdeation, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData HitmanIntro = new("Hitman", RoleClass.Hitman.color, 1, RoleId.Hitman, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData HauntedWolfIntro = new("HauntedWolf", HauntedWolf.RoleData.color, 2, RoleId.HauntedWolf);
    public static IntroData MatryoshkaIntro = new("Matryoshka", RoleClass.Matryoshka.color, 1, RoleId.Matryoshka, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NunIntro = new("Nun", RoleClass.Nun.color, 1, RoleId.Nun, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData PsychometristIntro = new("Psychometrist", RoleClass.Psychometrist.color, 1, RoleId.Psychometrist, IntroSound: RoleTypes.Scientist);
    public static IntroData SeeThroughPersonIntro = new("SeeThroughPerson", RoleClass.SeeThroughPerson.color, 1, RoleId.SeeThroughPerson);
    public static IntroData PartTimerIntro = new("PartTimer", RoleClass.PartTimer.color, 1, RoleId.PartTimer, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData PainterIntro = new("Painter", RoleClass.Painter.color, 1, RoleId.Painter);
    public static IntroData PhotographerIntro = new("Photographer", RoleClass.Photographer.color, 1, RoleId.Photographer, TeamRoleType.Neutral);
    public static IntroData StefinderIntro = new("Stefinder", RoleClass.Stefinder.color, 1, RoleId.Stefinder, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData StefinderIntro1 = new("Stefinder", RoleClass.ImpostorRed, 1, RoleId.Stefinder1, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData ShiftActorIntro = new("ShiftActor", ShiftActor.color, 1, RoleId.ShiftActor, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData ConnectKillerIntro = new("ConnectKiller", RoleClass.ConnectKiller.color, 1, RoleId.ConnectKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData GMIntro = new("GM", RoleClass.GM.color, 1, RoleId.GM, TeamRoleType.Neutral, IntroSound: RoleTypes.Engineer);
    public static IntroData CrackerIntro = new("Cracker", RoleClass.Cracker.color, 1, RoleId.Cracker, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NekoKabochaIntro = new("NekoKabocha", NekoKabocha.color, 1, RoleId.NekoKabocha, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData DoppelgangerIntro = new("Doppelganger", RoleClass.Doppelganger.color, 1, RoleId.Doppelganger, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData WerewolfIntro = new("Werewolf", RoleClass.Werewolf.color, 1, RoleId.Werewolf, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData KnightIntro = new("Knight", Knight.color, 1, RoleId.Knight);
    public static IntroData PavlovsdogsIntro = new("Pavlovsdogs", RoleClass.Pavlovsdogs.color, 1, RoleId.Pavlovsdogs, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData PavlovsownerIntro = new("Pavlovsowner", RoleClass.Pavlovsowner.color, 1, RoleId.Pavlovsowner, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData WaveCannonJackalIntro = new("WaveCannonJackal", WaveCannonJackal.color, 1, RoleId.WaveCannonJackal, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SidekickWaveCannonIntro = new("SidekickWaveCannon", WaveCannonJackal.color, 1, RoleId.SidekickWaveCannon, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData CamouflagerIntro = new("Camouflager", RoleClass.Camouflager.color, 1, RoleId.Camouflager, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData HamburgerShopIntro = new("HamburgerShop", RoleClass.HamburgerShop.color, 3, RoleId.HamburgerShop);
    public static IntroData PenguinIntro = new("Penguin", RoleClass.Penguin.color, 1, RoleId.Penguin, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData DependentsIntro = new("Dependents", RoleClass.Dependents.color, 1, RoleId.Dependents);
    public static IntroData LoversBreakerIntro = new("LoversBreaker", RoleClass.LoversBreaker.color, 1, RoleId.LoversBreaker, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData JumboIntro = new("Jumbo", RoleClass.Jumbo.color, 1, RoleId.Jumbo, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData NiceJumboIntro = new("NiceJumbo", CrewmateIntro.color, 1, RoleId.Jumbo, TeamRoleType.Crewmate);
    public static IntroData EvilJumboIntro = new("EvilJumbo", ImpostorIntro.color, 1, RoleId.Jumbo, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData WorshiperIntro = new("Worshiper", Worshiper.RoleData.color, 1, RoleId.Worshiper, teamType: TeamType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData SafecrackerIntro = new("Safecracker", Safecracker.color, 1, RoleId.Safecracker, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData FireFoxIntro = new("FireFox", FireFox.color, 1, RoleId.FireFox, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData SquidIntro = new("Squid", Squid.color, 2, RoleId.Squid, TeamRoleType.Crewmate);
    public static IntroData DyingMessengerIntro = new("DyingMessenger", DyingMessenger.color, 1, RoleId.DyingMessenger, TeamRoleType.Crewmate);
    public static IntroData WiseManIntro = new("WiseMan", WiseMan.color, 1, RoleId.WiseMan, TeamRoleType.Crewmate);
    public static IntroData NiceMechanicIntro = new("NiceMechanic", NiceMechanic.color, 1, RoleId.NiceMechanic, TeamRoleType.Crewmate);
    public static IntroData EvilMechanicIntro = new("EvilMechanic", EvilMechanic.color, 1, RoleId.EvilMechanic, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData TheFirstLittlePigIntro = new("TheFirstLittlePig", TheThreeLittlePigs.color, 1, RoleId.TheFirstLittlePig, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData TheSecondLittlePigIntro = new("TheSecondLittlePig", TheThreeLittlePigs.color, 1, RoleId.TheSecondLittlePig, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData TheThirdLittlePig = new("TheThirdLittlePig", TheThreeLittlePigs.color, 1, RoleId.TheThirdLittlePig, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData OrientalShamanIntro = new("OrientalShaman", OrientalShaman.color, 1, RoleId.OrientalShaman, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData ShermansServantIntro = new("ShermansServant", OrientalShaman.color, 1, RoleId.ShermansServant, TeamRoleType.Crewmate, TeamType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData BalancerIntro = new("Balancer", Balancer.color, 1, RoleId.Balancer, TeamRoleType.Crewmate);
    public static IntroData PteranodonIntro = new("Pteranodon", Pteranodon.color, 1, RoleId.Pteranodon, TeamRoleType.Crewmate);
    public static IntroData BlackHatHackerIntro = new("BlackHatHacker", BlackHatHacker.color, 1, RoleId.BlackHatHacker, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData ReviverIntro = new("Reviver", RoleClass.ImpostorRed, 1, RoleId.Reviver, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData GuardrawerIntro = new("Guardrawer", RoleClass.ImpostorRed, 1, RoleId.Guardrawer, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData KingPosterIntro = new("KingPoster", RoleClass.ImpostorRed, 1, RoleId.KingPoster, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData LongKillerIntro = new("LongKiller", RoleClass.ImpostorRed, 1, RoleId.LongKiller, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData DarknightIntro = new("Darknight", RoleClass.ImpostorRed, 1, RoleId.Darknight, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData RevengerIntro = new("Revenger", RoleClass.ImpostorRed, 1, RoleId.Revenger, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData CrystalMagicianIntro = new("CrystalMagician", RoleClass.ImpostorRed, 1, RoleId.CrystalMagician, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData GrimReaperIntro = new("GrimReaper", RoleClass.ImpostorRed, 1, RoleId.GrimReaper, TeamRoleType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData PoliceSurgeonIntro = new("PoliceSurgeon", PoliceSurgeon.RoleData.color, 1, RoleId.PoliceSurgeon, TeamRoleType.Crewmate, IntroSound: RoleTypes.Scientist);
    public static IntroData MadRaccoonIntro = new("MadRaccoon", MadRaccoon.RoleData.color, 2, RoleId.MadRaccoon, teamType: TeamType.Impostor, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData MoiraIntro = new("Moira", Moira.color, 1, RoleId.Moira, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    public static IntroData JumpDancerIntro = new("JumpDancer", JumpDancer.color, 1, RoleId.JumpDancer, TeamRoleType.Crewmate);
    public static IntroData SaunerIntro = new("Sauner", Sauner.RoleData.color, 1, RoleId.Sauner, TeamRoleType.Neutral);
    public static IntroData BatIntro = new("Bat", Bat.RoleData.color, 1, RoleId.Bat, TeamRoleType.Impostor);
    public static IntroData RocketIntro = new("Rocket", Rocket.RoleData.color, 2, RoleId.Rocket, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData WellBehaverIntro = new("WellBehaver", WellBehaver.color, 1, RoleId.WellBehaver, TeamRoleType.Crewmate);
    public static IntroData PokerfaceIntro = new("Pokerface", Pokerface.RoleData.color, 1, RoleId.Pokerface, TeamRoleType.Neutral);
    public static IntroData SpiderIntro = new("Spider", Spider.RoleData.color, 2, RoleId.Spider, TeamRoleType.Impostor, IntroSound: RoleTypes.Impostor);
    public static IntroData CrookIntro = new("Crook", Crook.RoleData.color, 1, RoleId.Crook, TeamRoleType.Neutral, IntroSound: RoleTypes.Impostor);
    public static IntroData FrankensteinIntro = new("Frankenstein", Frankenstein.color, 1, RoleId.Frankenstein, TeamRoleType.Neutral, IntroSound: RoleTypes.Shapeshifter);
    // イントロオブジェ
}