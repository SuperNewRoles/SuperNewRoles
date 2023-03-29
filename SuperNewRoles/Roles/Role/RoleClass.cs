using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Sabotage;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles;

[HarmonyPatch]
public static class RoleClass
{
    public static bool IsMeeting;
    public static bool IsCoolTimeSetted;
    public static System.Random rnd = new((int)DateTime.Now.Ticks);
    public static Color ImpostorRed = Palette.ImpostorRed;
    public static Color CrewmateWhite = Color.white;
    public static Color FoxPurple = Palette.Purple;
    private static Color32 SheriffYellow = new(250, 191, 20, byte.MaxValue);
    public static Color32 JackalBlue = new(0, 180, 235, byte.MaxValue);
    public static bool IsStart;
    public static List<byte> BlockPlayers;
    public static float DefaultKillCoolDown;

    public static void ClearAndReloadRoles()
    {
        ModHelpers.IdControlDic = new();
        ModHelpers.VentIdControlDic = new();
        BlockPlayers = new();
        IsMeeting = false;
        RandomSpawn.IsFirstSpawn = true;
        DeadPlayer.deadPlayers = new();
        AllRoleSetClass.Assigned = false;
        LateTask.Tasks = new();
        LateTask.AddTasks = new();
        BotManager.AllBots = new();
        IsCoolTimeSetted = false;
        DefaultKillCoolDown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        IsStart = false;
        Agartha.MapData.ClearAndReloads();
        Mode.PlusMode.PlusGameOptions.ClearAndReload();
        LadderDead.Reset();
        //Map.Data.ClearAndReloads();
        ElectricPatch.Reset();
        SabotageManager.ClearAndReloads();
        Roles.Madmate.CheckedImpostor = new();
        Roles.JackalFriends.CheckedJackal = new();
        Mode.BattleRoyal.Main.VentData = new();
        FinalStatusPatch.FinalStatusData.ClearFinalStatusData();
        Mode.ModeHandler.ClearAndReload();
        MapCustoms.MapCustomClearAndReload.ClearAndReload();
        MapCustoms.AdditionalVents.ClearAndReload();
        MapCustoms.SpecimenVital.ClearAndReload();
        MapCustoms.MoveElecPad.ClearAndReload();
        Beacon.ClearBeacons();
        MeetingHudUpdatePatch.ErrorNames = new();
        FixSabotage.ClearAndReload();
        RoleBases.Role.ClearAll();

        /* 陣営playerがうまく動かず使われてない為コメントアウト。
        RoleHelpers.CrewmatePlayer = new();
        RoleHelpers.ImposterPlayer = new();
        RoleHelpers.NeutralPlayer = new();
        RoleHelpers.MadRolesPlayer = new();
        RoleHelpers.FriendRolesPlayer = new();
        RoleHelpers.NeutralKillingPlayer = new();
        */

        Debugger.ClearAndReload();
        SoothSayer.ClearAndReload();
        Jester.ClearAndReload();
        Lighter.ClearAndReload();
        EvilLighter.ClearAndReload();
        EvilScientist.ClearAndReload();
        Sheriff.ClearAndReload();
        MeetingSheriff.ClearAndReload();
        Jackal.ClearAndReload();
        Teleporter.ClearAndReload();
        SpiritMedium.ClearAndReload();
        SpeedBooster.ClearAndReload();
        EvilSpeedBooster.ClearAndReload();
        Tasker.ClearAndReload();
        Doorr.ClearAndReload();
        EvilDoorr.ClearAndReload();
        Shielder.ClearAndReload();
        Speeder.ClearAndReload();
        Freezer.ClearAndReload();
        NiceGuesser.ClearAndReload();
        EvilGuesser.ClearAndReload();
        Vulture.ClearAndReload();
        NiceScientist.ClearAndReload();
        Clergyman.ClearAndReload();
        Madmate.ClearAndReload();
        Bait.ClearAndReload();
        HomeSecurityGuard.ClearAndReload();
        StuntMan.ClearAndReload();
        Moving.ClearAndReload();
        Opportunist.ClearAndReload();
        NiceGambler.ClearAndReload();
        EvilGambler.ClearAndReload();
        Bestfalsecharge.ClearAndReload();
        Researcher.ClearAndReload();
        SelfBomber.ClearAndReload();
        God.ClearAndReload();
        AllCleaner.ClearAndReload();
        NiceNekomata.ClearAndReload();
        EvilNekomata.ClearAndReload();
        JackalFriends.ClearAndReload();
        Doctor.ClearAndReload();
        CountChanger.ClearAndReload();
        Pursuer.ClearAndReload();
        Minimalist.ClearAndReload();
        Hawk.ClearAndReload();
        Egoist.ClearAndReload();
        NiceRedRidingHood.ClearAndReload();
        EvilEraser.ClearAndReload();
        Workperson.ClearAndReload();
        Magaziner.ClearAndReload();
        Mayor.ClearAndReload();
        Truelover.ClearAndReload();
        Technician.ClearAndReload();
        SerialKiller.ClearAndReload();
        OverKiller.ClearAndReload();
        Levelinger.ClearAndReload();
        EvilMoving.ClearAndReload();
        Amnesiac.ClearAndReload();
        SideKiller.ClearAndReload();
        Survivor.ClearAndReload();
        MadMayor.ClearAndReload();
        NiceHawk.ClearAndReload();
        Bakery.ClearAndReload();
        MadStuntMan.ClearAndReload();
        MadHawk.ClearAndReload();
        MadJester.ClearAndReload();
        FalseCharges.ClearAndReload();
        NiceTeleporter.ClearAndReload();
        Celebrity.ClearAndReload();
        Nocturnality.ClearAndReload();
        Observer.ClearAndReload();
        Vampire.ClearAndReload();
        Fox.ClearAndReload();
        DarkKiller.ClearAndReload();
        Seer.ClearAndReload();
        Crewmate.Seer.ShowFlash_ClearAndReload();
        MadSeer.ClearAndReload();
        EvilSeer.ClearAndReload();
        RemoteSheriff.ClearAndReload();
        TeleportingJackal.ClearAndReload();
        MadMaker.ClearAndReload();
        Demon.ClearAndReload();
        TaskManager.ClearAndReload();
        SeerFriends.ClearAndReload();
        JackalSeer.ClearAndReload();
        Assassin.ClearAndReload();
        Marlin.ClearAndReload();
        Arsonist.ClearAndReload();
        Chief.ClearAndReload();
        Cleaner.ClearAndReload();
        MadCleaner.ClearAndReload();
        Samurai.ClearAndReload();
        MayorFriends.ClearAndReload();
        VentMaker.ClearAndReload();
        GhostMechanic.ClearAndReload();
        EvilHacker.ClearAndReload();
        HauntedWolf.ClearAndReload();
        PositionSwapper.ClearAndReload();
        Tuna.ClearAndReload();
        Mafia.ClearAndReload();
        BlackCat.ClearAndReload();
        SecretlyKiller.ClearAndReload();
        Spy.ClearAndReload();
        Kunoichi.ClearAndReload();
        DoubleKiller.ClearAndReload();
        Smasher.ClearAndReload();
        SuicideWisher.ClearAndReload();
        Neet.ClearAndReload();
        FastMaker.ClearAndReload();
        ToiletFan.ClearAndReload();
        SatsumaAndImo.ClearAndReload();
        EvilButtoner.ClearAndReload();
        NiceButtoner.ClearAndReload();
        Finder.ClearAndReload();
        Revolutionist.ClearAndReload();
        Dictator.ClearAndReload();
        Spelunker.ClearAndReload();
        SuicidalIdeation.ClearAndReload();
        Hitman.ClearAndReload();
        Matryoshka.ClearAndReload();
        Nun.ClearAndReload();
        Psychometrist.ClearAndReload();
        SeeThroughPerson.ClearAndReload();
        PartTimer.ClearAndReload();
        Painter.ClearAndReload();
        Photographer.ClearAndReload();
        Stefinder.ClearAndReload();
        Slugger.ClearAndReload();
        ShiftActor.ClearAndReload();
        ConnectKiller.ClearAndReload();
        GM.ClearAndReload();
        Cracker.ClearAndReload();
        NekoKabocha.ClearAndReload();
        WaveCannon.ClearAndReload();
        Doppelganger.ClearAndReload();
        Werewolf.ClearAndReload();
        Knight.ClearAndReload();
        Pavlovsdogs.ClearAndReload();
        Pavlovsowner.ClearAndReload();
        Neutral.WaveCannonJackal.ClearAndReload();
        //SidekickWaveCannon.Clear();
        Conjurer.ClearAndReload();
        Camouflager.ClearAndReload();
        Cupid.ClearAndReload();
        HamburgerShop.ClearAndReload();
        Penguin.ClearAndReload();
        Dependents.ClearAndReload();
        LoversBreaker.ClearAndReload();
        Jumbo.ClearAndReload();
        Impostor.MadRole.Worshiper.ClearAndReload();
        Safecracker.ClearAndReload();
        FireFox.ClearAndReload();
        Squid.ClearAndReload();
        DyingMessenger.ClearAndReload();
        WiseMan.ClearAndReload();
        NiceMechanic.ClearAndReload();
        EvilMechanic.ClearAndReload();
        TheThreeLittlePigs.ClearAndReload();
        OrientalShaman.ClearAndReload();
        // ロールクリア
        Quarreled.ClearAndReload();
        Lovers.ClearAndReload();
        MapOption.MapOption.ClearAndReload();
        ChacheManager.Load();
    }

    public static class Debugger
    {
        public static bool AmDebugger;
        public static Color32 color = new(130, 130, 130, byte.MaxValue);
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.GhostMechanicRepairButton.png", 115f);

        public static void ClearAndReload()
        {
            AmDebugger = AmongUsClient.Instance.AmHost && ConfigRoles.DebugMode.Value && CustomOptionHolder.DebuggerOption.GetBool();
        }
    }

    public static class SoothSayer
    {
        public static List<PlayerControl> SoothSayerPlayer;
        public static List<byte> DisplayedPlayer;
        public static bool DisplayMode;
        public static int Count;
        public static Color32 color = new(190, 86, 235, byte.MaxValue);
        public static bool CanFirstWhite;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SoothSayerButton.png", 115f);
        public static void ClearAndReload()
        {
            SoothSayerPlayer = new();
            DisplayedPlayer = new();
            DisplayMode = CustomOptionHolder.SoothSayerDisplayMode.GetBool();
            Count = CustomOptionHolder.SoothSayerMaxCount.GetInt();
            CanFirstWhite = CustomOptionHolder.SoothSayerFirstWhiteOption.GetBool();
        }
    }
    public static class Jester
    {
        public static List<PlayerControl> JesterPlayer;
        public static bool IsJesterWin;
        public static Color32 color = new(255, 165, 0, byte.MaxValue);
        public static bool IsUseVent;
        public static bool IsUseSabo;
        public static bool IsJesterTaskClearWin;
        public static void ClearAndReload()
        {
            IsJesterWin = false;
            JesterPlayer = new();
            IsUseSabo = CustomOptionHolder.JesterIsSabotage.GetBool();
            IsUseVent = CustomOptionHolder.JesterIsVent.GetBool();
            IsJesterTaskClearWin = CustomOptionHolder.JesterIsWinCleartask.GetBool();
        }
    }
    public static class Lighter
    {
        public static List<PlayerControl> LighterPlayer;
        public static Color32 color = new(255, 236, 71, byte.MaxValue);
        public static float CoolTime;
        public static float DurationTime;
        public static bool IsLightOn;
        public static float UpVision;
        public static float DefaultCrewVision;
        public static DateTime ButtonTimer;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.LighterLightOnButton.png", 115f);
        public static void ClearAndReload()
        {
            LighterPlayer = new();
            CoolTime = CustomOptionHolder.LighterCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.LighterDurationTime.GetFloat();
            UpVision = CustomOptionHolder.LighterUpVision.GetFloat();
            DefaultCrewVision = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
        }
    }
    public static class EvilLighter
    {
        public static List<PlayerControl> EvilLighterPlayer;
        public static Color32 color = RoleClass.ImpostorRed;
        //public static float CoolTime;
        //public static float DurationTime;

        public static void ClearAndReload()
        {
            EvilLighterPlayer = new();
            //CoolTime = CustomOptionHolder.EvilLighterCoolTime.GetFloat();
            //DurationTime = CustomOptionHolder.EvilLighterDurationTime.GetFloat();
        }
    }
    public static class EvilScientist
    {
        public static List<PlayerControl> EvilScientistPlayer;
        public static Color32 color = RoleClass.ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.EvilScientistButton.png.png", 115f);
        public static void ClearAndReload()
        {
            EvilScientistPlayer = new();
            CoolTime = CustomOptionHolder.EvilScientistCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.EvilScientistDurationTime.GetFloat();
        }
    }
    public static class Sheriff
    {
        public static List<PlayerControl> SheriffPlayer;
        public static Color32 color = SheriffYellow;
        public static PlayerControl currentTarget;
        public static float CoolTime;
        public static float KillMaxCount;
        public static Dictionary<int, int> KillCount;
        public static DateTime ButtonTimer;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);

        public static void ClearAndReload()
        {
            SheriffPlayer = new();
            CoolTime = CustomOptionHolder.SheriffCoolTime.GetFloat();
            KillMaxCount = CustomOptionHolder.SheriffKillMaxCount.GetFloat();
            KillCount = new();
        }
    }
    public static class MeetingSheriff
    {
        public static List<PlayerControl> MeetingSheriffPlayer;
        public static Color32 color = SheriffYellow;
        public static float KillMaxCount;
        public static bool OneMeetingMultiKill;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MeetingSheriffKillButton.png", 200f);
        public static void ClearAndReload()
        {
            MeetingSheriffPlayer = new();
            KillMaxCount = CustomOptionHolder.MeetingSheriffKillMaxCount.GetFloat();
            OneMeetingMultiKill = CustomOptionHolder.MeetingSheriffOneMeetingMultiKill.GetBool();
        }
    }
    public static class Jackal
    {
        public static List<PlayerControl> JackalPlayer;
        public static List<PlayerControl> SidekickPlayer;
        public static List<PlayerControl> FakeSidekickPlayer;
        public static Color32 color = JackalBlue;
        public static float KillCooldown;
        public static bool IsUseVent;
        public static bool IsUseSabo;
        public static bool IsImpostorLight;
        public static bool CreateSidekick;
        public static bool CanCreateSidekick;
        public static List<int> CreatePlayers;
        public static bool IsCreatedFriend;
        public static bool CanCreateFriend;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JackalSidekickButton.png", 115f);
        public static void ClearAndReload()
        {
            JackalPlayer = new();
            SidekickPlayer = new();
            FakeSidekickPlayer = new();
            KillCooldown = CustomOptionHolder.JackalKillCooldown.GetFloat();
            IsUseVent = CustomOptionHolder.JackalUseVent.GetBool();
            IsUseSabo = CustomOptionHolder.JackalUseSabo.GetBool();
            IsImpostorLight = CustomOptionHolder.JackalIsImpostorLight.GetBool();
            CreateSidekick = CustomOptionHolder.JackalCreateSidekick.GetBool();
            CanCreateSidekick = CustomOptionHolder.JackalCreateSidekick.GetBool();
            IsCreatedFriend = false;
            CreatePlayers = new();
            CanCreateFriend = CustomOptionHolder.JackalCreateFriend.GetBool();
        }
    }
    public static class Teleporter
    {
        public static List<PlayerControl> TeleporterPlayer;
        public static Color32 color = RoleClass.ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static DateTime ButtonTimer;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
        public static void ClearAndReload()
        {
            TeleporterPlayer = new();
            CoolTime = CustomOptionHolder.TeleporterCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.TeleporterDurationTime.GetFloat();
        }
    }
    public static class SpiritMedium
    {
        public static List<PlayerControl> SpiritMediumPlayer;
        public static Color32 color = new(0, 191, 255, byte.MaxValue);
        public static bool DisplayMode;
        public static float MaxCount;
        public static PlayerControl ExilePlayer;

        public static void ClearAndReload()
        {
            SpiritMediumPlayer = new();
            DisplayMode = CustomOptionHolder.SpiritMediumDisplayMode.GetBool();
            MaxCount = CustomOptionHolder.SpiritMediumMaxCount.GetFloat();
            ExilePlayer = null;
        }
    }
    public static class SpeedBooster
    {
        public static List<PlayerControl> SpeedBoosterPlayer;
        public static Color32 color = new(100, 149, 237, byte.MaxValue);
        public static Sprite SpeedBoostButtonSprite;
        public static float CoolTime;
        public static float DurationTime;
        public static float Speed;
        public static DateTime ButtonTimer;
        public static bool IsSpeedBoost;
        public static Dictionary<int, bool> IsBoostPlayers;
        public static Sprite GetSpeedBoostButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);

        public static void ClearAndReload()
        {
            SpeedBoosterPlayer = new();
            CoolTime = CustomOptionHolder.SpeedBoosterCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.SpeedBoosterDurationTime.GetFloat();
            Speed = CustomOptionHolder.SpeedBoosterSpeed.GetFloat();
            IsSpeedBoost = false;
            IsBoostPlayers = new Dictionary<int, bool>();
        }
    }
    public static class EvilSpeedBooster
    {
        public static List<PlayerControl> EvilSpeedBoosterPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static float Speed { get { return CustomOptionHolder.EvilSpeedBoosterSpeed.GetFloat(); } }
        public static bool IsSpeedBoost;
        public static DateTime ButtonTimer;
        public static Dictionary<int, bool> IsBoostPlayers;
        public static void ClearAndReload()
        {
            ButtonTimer = DateTime.Now;
            EvilSpeedBoosterPlayer = new();
            CoolTime = CustomOptionHolder.EvilSpeedBoosterCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.EvilSpeedBoosterDurationTime.GetFloat();
            IsSpeedBoost = false;
            IsBoostPlayers = new Dictionary<int, bool>();
        }
    }
    public static class Tasker
    {
        public static List<PlayerControl> TaskerPlayer;
        public static Color32 color = ImpostorRed;
        //public static bool IsKill;
        //public static float TaskCount;

        public static void ClearAndReload()
        {
            TaskerPlayer = new();
            //IsKill = CustomOptionHolder.TaskerIsKill.GetBool();
            //TaskCount = CustomOptionHolder.TaskerAmount.GetFloat();
        }
    }
    public static class Doorr
    {
        public static List<PlayerControl> DoorrPlayer;
        public static Color32 color = new(205, 133, 63, byte.MaxValue);
        public static float CoolTime;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.DoorrDoorButton.png", 115f);

        public static void ClearAndReload()
        {
            DoorrPlayer = new();
            CoolTime = CustomOptionHolder.DoorrCoolTime.GetFloat();
        }
    }
    public static class EvilDoorr
    {
        public static List<PlayerControl> EvilDoorrPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static void ClearAndReload()
        {
            EvilDoorrPlayer = new();
            CoolTime = CustomOptionHolder.EvilDoorrCoolTime.GetFloat();
        }
    }
    public static class Shielder
    {
        public static List<PlayerControl> ShielderPlayer;
        public static Color32 color = new(100, 149, 237, byte.MaxValue);
        public static float CoolTime;
        public static float DurationTime;
        public static Dictionary<byte, bool> IsShield;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ShielderButton.png", 115f);

        public static void ClearAndReload()
        {
            ShielderPlayer = new();
            CoolTime = CustomOptionHolder.ShielderCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.ShielderDurationTime.GetFloat();
            IsShield = new Dictionary<byte, bool>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers) RoleClass.Shielder.IsShield[p.PlayerId] = false;
        }
    }
    public static class Freezer
    {
        public static List<PlayerControl> FreezerPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static bool IsSpeedDown;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FreezerButton.png", 115f);

        public static void ClearAndReload()
        {
            FreezerPlayer = new();
            CoolTime = CustomOptionHolder.FreezerCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.FreezerDurationTime.GetFloat();
            IsSpeedDown = false;
        }
    }

    public static class Speeder
    {
        public static List<PlayerControl> SpeederPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static bool IsSpeedDown;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedDownButton.png", 115f);

        public static void ClearAndReload()
        {
            SpeederPlayer = new();
            CoolTime = CustomOptionHolder.SpeederCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.SpeederDurationTime.GetFloat();
            IsSpeedDown = false;
        }
    }
    public static class NiceGuesser
    {
        public static List<PlayerControl> NiceGuesserPlayer;
        public static Color32 color = Color.yellow;
        public static int Count;
        public static void ClearAndReload()
        {
            NiceGuesserPlayer = new();
            Count = -1;
        }
    }
    public static class EvilGuesser
    {
        public static List<PlayerControl> EvilGuesserPlayer;
        public static Color32 color = ImpostorRed;
        public static void ClearAndReload()
        {
            EvilGuesserPlayer = new();
        }
    }
    public static class Vulture
    {
        public static List<PlayerControl> VulturePlayer;
        public static Color32 color = new(205, 133, 63, byte.MaxValue);
        public static int DeadBodyMaxCount;
        public static float CoolTime;
        public static int DeadBodyCount;
        public static bool IsUseVent;
        public static bool ShowArrows;
        public static Dictionary<DeadBody, Arrow> DeadPlayerArrows;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.VultureButton.png", 115f);

        public static void ClearAndReload()
        {
            VulturePlayer = new();
            CoolTime = CustomOptionHolder.VultureCooldown.GetFloat();
            DeadBodyCount = CustomOptionHolder.VultureDeadBodyMaxCount.GetInt();
            IsUseVent = CustomOptionHolder.VultureIsUseVent.GetBool();
            ShowArrows = CustomOptionHolder.VultureShowArrows.GetBool();
            DeadPlayerArrows = new();
        }
    }
    public static class NiceScientist
    {
        public static List<PlayerControl> NiceScientistPlayer;
        public static Color32 color = Palette.CrewmateBlue;
        public static float CoolTime;
        public static float DurationTime;
        public static DateTime ButtonTimer;
        public static bool IsScientist;
        public static Dictionary<int, bool> IsScientistPlayers;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.NiceScientistButton.png", 115f);

        public static void ClearAndReload()
        {
            NiceScientistPlayer = new();
            CoolTime = CustomOptionHolder.NiceScientistCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.NiceScientistDurationTime.GetFloat();
            ButtonTimer = DateTime.Now;
            IsScientist = false;
            IsScientistPlayers = new Dictionary<int, bool>();
        }
    }
    public static class Clergyman
    {
        public static List<PlayerControl> ClergymanPlayer;
        public static Color32 color = new(255, 0, 255, byte.MaxValue);
        public static float CoolTime;
        public static float DurationTime;
        public static bool IsLightOff;
        public static float DownImpoVision;
        public static float DefaultImpoVision;
        public static DateTime ButtonTimer;
        public static DateTime OldButtonTimer;
        public static float OldButtonTime;

        public static CustomMessage currentMessage;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ClergymanLightOutButton.png", 115f);

        public static void ClearAndReload()
        {
            ClergymanPlayer = new();
            CoolTime = CustomOptionHolder.ClergymanCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.ClergymanDurationTime.GetFloat();
            IsLightOff = false;
            DownImpoVision = CustomOptionHolder.ClergymanDownVision.GetFloat();
            DefaultImpoVision = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);
            OldButtonTimer = DateTime.Now;
            OldButtonTime = 0;
            currentMessage = null;
        }
    }
    public static class Madmate
    {
        public static List<PlayerControl> MadmatePlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static void ClearAndReload()
        {
            MadmatePlayer = new();
            IsImpostorCheck = CustomOptionHolder.MadmateIsCheckImpostor.GetBool();
            IsUseVent = CustomOptionHolder.MadmateIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadmateIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.MadmateCommonTask.GetInt();
            int Long = CustomOptionHolder.MadmateLongTask.GetInt();
            int Short = CustomOptionHolder.MadmateShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.MadmateCheckImpostorTask.GetString().Replace("%", "")) / 100f));
        }
    }
    public static class Bait
    {
        public static List<PlayerControl> BaitPlayer;
        public static Color32 color = new(222, 184, 135, byte.MaxValue);
        public static bool Reported;
        public static List<int> ReportedPlayer;
        public static float ReportTime = 0f;

        public static void ClearAndReload()
        {
            BaitPlayer = new();
            Reported = false;
            ReportTime = CustomOptionHolder.BaitReportTime.GetFloat();
            ReportedPlayer = new();
        }
    }
    public static class HomeSecurityGuard
    {
        public static List<PlayerControl> HomeSecurityGuardPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);

        public static void ClearAndReload()
        {
            HomeSecurityGuardPlayer = new();
        }
    }
    public static class StuntMan
    {
        public static List<PlayerControl> StuntManPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static Dictionary<int, int> GuardCount;

        public static void ClearAndReload()
        {
            StuntManPlayer = new();
            GuardCount = new();
        }
    }
    public static class Moving
    {
        public static List<PlayerControl> MovingPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static float CoolTime;
        public static DateTime ButtonTimer;
        public static Vector3 setpostion;
        public static Sprite GetNoSetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MovingLocationSetButton.png", 115f);

        public static Sprite GetSetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MovingTpButton.png", 115f);

        public static void ClearAndReload()
        {
            MovingPlayer = new();
            setpostion = new Vector3(0, 0, 0);
            CoolTime = CustomOptionHolder.MovingCoolTime.GetFloat();
        }
    }
    public static class Opportunist
    {
        public static List<PlayerControl> OpportunistPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static void ClearAndReload()
        {
            OpportunistPlayer = new();
        }
    }
    public static class NiceGambler
    {
        public static List<PlayerControl> NiceGamblerPlayer;
        //public static int Num;
        public static Color32 color = new(218, 112, 214, byte.MaxValue);
        public static void ClearAndReload()
        {
            NiceGamblerPlayer = new();
            //Num = CustomOptionHolder.NiceGamblerUseCount.GetInt();
        }
    }
    public static class EvilGambler
    {
        public static List<PlayerControl> EvilGamblerPlayer;
        public static float SucCool;
        public static float NotSucCool;
        public static int SucPar;
        public static bool IsSuc;
        public static float currentCool;
        public static Color32 color = ImpostorRed;
        public static void ClearAndReload()
        {
            EvilGamblerPlayer = new();
            IsSuc = false;
            SucCool = CustomOptionHolder.EvilGamblerSucTime.GetFloat();
            NotSucCool = CustomOptionHolder.EvilGamblerNotSucTime.GetFloat();
            var temp = CustomOptionHolder.EvilGamblerSucpar.GetString().Replace("0%", "");
            SucPar = temp == "" ? 0 : int.Parse(temp);
        }
        public static bool GetSuc()
        {
            List<string> a = new();
            for (int i = 0; i < SucPar; i++)
            {
                a.Add("Suc");
            }
            for (int i = 0; i < 10 - SucPar; i++)
            {
                a.Add("No");
            }
            return ModHelpers.GetRandom<string>(a) == "Suc";
        }
    }
    public static class Bestfalsecharge
    {
        public static List<PlayerControl> BestfalsechargePlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static bool IsOnMeeting;
        public static void ClearAndReload()
        {
            BestfalsechargePlayer = new();
            IsOnMeeting = false;
        }
    }
    public static class Researcher
    {
        public static List<PlayerControl> ResearcherPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        //public static Vector3 SamplePosition;
        private static readonly List<Vector3> SamplePoss = new()
            {
                new Vector3(-11, -2.1f, 0),
                new Vector3(16.9f, 0.4f, 0),
                new Vector3(35.2f, -6.8f, 0),
                new Vector3(11f, -2.1f, 0),
                new Vector3(28.9f, -5.2f, 0)
            };
        public static List<PlayerControl> GetSamplePlayers;
        public static List<PlayerControl> OKSamplePlayers;
        public static int MySample;
        public static void ClearAndReload()
        {
            ResearcherPlayer = new();
            OKSamplePlayers = new();
            GetSamplePlayers = new();
            //SamplePosition = SamplePoss[GameOptionsManager.Instance.CurrentGameOptions.MapId];
            MySample = 0;
        }
    }
    public static class SelfBomber
    {
        public static List<PlayerControl> SelfBomberPlayer;
        public static Color32 color = ImpostorRed;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SelfBomberBomButton.png", 115f);

        public static void ClearAndReload()
        {
            SelfBomberPlayer = new();
        }
    }
    public static class God
    {
        public static List<PlayerControl> GodPlayer;
        public static Color32 color = Color.yellow;
        public static bool IsVoteView;
        public static bool IsTaskEndWin;
        public static void ClearAndReload()
        {
            GodPlayer = new();
            IsVoteView = CustomOptionHolder.GodViewVote.GetBool();
            IsTaskEndWin = CustomOptionHolder.GodIsEndTaskWin.GetBool();
        }
    }
    public static class AllCleaner
    {
        public static List<PlayerControl> AllCleanerPlayer;
        public static Color32 color = ImpostorRed;
        public static void ClearAndReload()
        {
            AllCleanerPlayer = new();
        }
    }
    public static class NiceNekomata
    {
        public static List<PlayerControl> NiceNekomataPlayer;
        public static Color32 color = new(244, 164, 96, byte.MaxValue);
        public static bool IsChain;
        public static void ClearAndReload()
        {
            NiceNekomataPlayer = new();
            IsChain = CustomOptionHolder.NiceNekomataIsChain.GetBool();
        }
    }
    public static class EvilNekomata
    {
        public static List<PlayerControl> EvilNekomataPlayer;
        public static Color32 color = ImpostorRed;
        public static bool NotImpostorExiled;
        public static void ClearAndReload()
        {
            EvilNekomataPlayer = new();
            NotImpostorExiled = CustomOptionHolder.EvilNekomataNotImpostorExiled.GetBool();
        }
    }
    public static class JackalFriends
    {
        public static List<PlayerControl> JackalFriendsPlayer;
        public static Color32 color = JackalBlue;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsJackalCheck;
        public static int JackalCheckTask;
        public static void ClearAndReload()
        {
            JackalFriendsPlayer = new();

            IsJackalCheck = CustomOptionHolder.JackalFriendsIsCheckJackal.GetBool();
            IsUseVent = CustomOptionHolder.JackalFriendsIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.JackalFriendsIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.JackalFriendsCommonTask.GetInt();
            int Long = CustomOptionHolder.JackalFriendsLongTask.GetInt();
            int Short = CustomOptionHolder.JackalFriendsShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.JackalFriendsCheckJackalTask.GetString().Replace("%", "")) / 100f));
            Roles.JackalFriends.CheckedJackal = new();
        }
    }
    public static class Doctor
    {
        public static List<PlayerControl> DoctorPlayer;
        public static Color32 color = new(102, 102, 255, byte.MaxValue);
        public static bool MyPanelFlag;
        //100%から減っていく
        //0%だと使用できない
        //100%までチャージされると再度使えるようになる
        public static int Battery;
        public static float BatteryZeroTime;
        public static float ChargeTime;
        public static float UseTime;
        public static bool IsChargingNow;
        public static Minigame Vital;
        private static Sprite VitalSprite;
        public static Sprite GetVitalsSprite()
        {
            if (VitalSprite) return VitalSprite;
            VitalSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
            return VitalSprite;
        }
        public static void ClearAndReload()
        {
            DoctorPlayer = new();
            MyPanelFlag = false;
            Vital = null;
            Battery = 100;
            ChargeTime = CustomOptionHolder.DoctorChargeTime.GetFloat();
            UseTime = CustomOptionHolder.DoctorUseTime.GetFloat();
            BatteryZeroTime = UseTime;
            IsChargingNow = false;
            Roles.Doctor.VitalsPatch.ResetData();
        }
    }
    public static class CountChanger
    {
        public static List<PlayerControl> CountChangerPlayer;
        public static Color32 color = ImpostorRed;
        public static Dictionary<int, int> ChangeData;
        public static Dictionary<int, int> Setdata;
        public static int Count;
        public static bool IsSet;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CountChangerButton.png", 115f);

        public static void ClearAndReload()
        {
            CountChangerPlayer = new();
            ChangeData = new();
            Setdata = new();
            Count = CustomOptionHolder.CountChangerMaxCount.GetInt();
            IsSet = false;
        }
    }
    public static class Pursuer
    {
        public static List<PlayerControl> PursuerPlayer;
        public static Color32 color = ImpostorRed;
        public static Arrow arrow = null;
        public static void ClearAndReload()
        {
            PursuerPlayer = new();
            if (arrow != null)
            {
                GameObject.Destroy(arrow.arrow);
            }
            arrow = new Arrow(color);
            arrow.arrow.SetActive(false);
        }
    }
    public static class Minimalist
    {
        public static List<PlayerControl> MinimalistPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static bool UseVent;
        public static bool UseSabo;
        public static bool UseReport;
        public static void ClearAndReload()
        {
            MinimalistPlayer = new();
            KillCoolTime = CustomOptionHolder.MinimalistKillCoolTime.GetFloat();
            UseVent = CustomOptionHolder.MinimalistVent.GetBool();
            UseSabo = CustomOptionHolder.MinimalistSabo.GetBool();
            UseReport = CustomOptionHolder.MinimalistReport.GetBool();
        }
    }
    public static class Hawk
    {
        public static List<PlayerControl> HawkPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static bool IsHawkOn;
        public static float Timer;
        public static DateTime ButtonTimer;
        public static float Default;
        public static float CameraDefault;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.HawkHawkEye.png", 115f);

        public static void ClearAndReload()
        {
            HawkPlayer = new();
            CoolTime = CustomOptionHolder.HawkCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.HawkDurationTime.GetFloat();
            IsHawkOn = false;
            Timer = 0;
            ButtonTimer = DateTime.Now;
            CameraDefault = Camera.main.orthographicSize;
            Default = FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize;
        }
    }
    public static class Egoist
    {
        public static List<PlayerControl> EgoistPlayer;
        public static Color32 color = new(192, 192, 192, byte.MaxValue);
        public static bool ImpostorLight;
        public static bool UseVent;
        public static bool UseSabo;
        public static bool UseKill;
        public static void ClearAndReload()
        {
            EgoistPlayer = new();
            ImpostorLight = CustomOptionHolder.EgoistImpostorLight.GetBool();
            UseVent = CustomOptionHolder.EgoistUseVent.GetBool();
            UseSabo = CustomOptionHolder.EgoistUseSabo.GetBool();
            UseKill = CustomOptionHolder.EgoistUseKill.GetBool();
        }
    }
    public static class NiceRedRidingHood
    {
        public static List<PlayerControl> NiceRedRidingHoodPlayer;
        public static Color32 color = new(250, 128, 114, byte.MaxValue);
        public static int Count;
        public static Vector3? deadbodypos;
        public static void ClearAndReload()
        {
            NiceRedRidingHoodPlayer = new();
            Count = CustomOptionHolder.NiceRedRidingHoodCount.GetInt();
            deadbodypos = null;
        }
    }
    public static class EvilEraser
    {
        public static List<PlayerControl> EvilEraserPlayer;
        public static Color32 color = ImpostorRed;
        public static Dictionary<int, int> Counts;
        public static int Count;
        public static void ClearAndReload()
        {
            EvilEraserPlayer = new();
            Counts = new();
            Count = CustomOptionHolder.EvilEraserMaxCount.GetInt() - 1;
        }
    }
    public static class Workperson
    {
        public static List<PlayerControl> WorkpersonPlayer;
        public static Color32 color = new(210, 180, 140, byte.MaxValue);
        public static bool IsAliveWin;
        public static void ClearAndReload()
        {
            WorkpersonPlayer = new();
            IsAliveWin = CustomOptionHolder.WorkpersonIsAliveWin.GetBool();
        }
    }
    public static class Magaziner
    {
        public static List<PlayerControl> MagazinerPlayer;
        public static Color32 color = ImpostorRed;
        public static int MyPlayerCount;
        public static float SetTime;
        public static bool IsOKSet;
        public static Sprite GetGetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MagazinerGetButton.png", 115f);
        public static Sprite GetAddButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MagazinerAddButton.png", 115f);

        public static void ClearAndReload()
        {
            MagazinerPlayer = new();
            MyPlayerCount = 0;
            SetTime = CustomOptionHolder.MagazinerSetKillTime.GetFloat();
            IsOKSet = true;
        }
    }
    public static class Mayor
    {
        public static List<PlayerControl> MayorPlayer;
        public static Color32 color = new(0, 128, 128, byte.MaxValue);
        public static int AddVote;
        public static void ClearAndReload()
        {
            MayorPlayer = new();
            AddVote = CustomOptionHolder.MayorVoteCount.GetInt();
        }
    }
    public static class Truelover
    {
        public static List<PlayerControl> trueloverPlayer;
        public static Color32 color = Lovers.color;
        public static bool IsCreate;
        public static List<int> CreatePlayers;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.trueloverloveButton.png", 115f);

        public static void ClearAndReload()
        {
            trueloverPlayer = new();
            IsCreate = false;
            CreatePlayers = new();
        }
    }
    public static class Technician
    {
        public static List<PlayerControl> TechnicianPlayer;
        public static Color32 color = Color.blue;
        public static void ClearAndReload()
        {
            TechnicianPlayer = new();
        }
    }
    public static class SerialKiller
    {
        public static List<PlayerControl> SerialKillerPlayer;
        public static Color32 color = ImpostorRed;
        public static float SuicideDefaultTime;
        public static float SuicideTime;
        public static float KillTime;
        public static Dictionary<byte, float> SuicideTimers;
        public static bool IsSuicideView;
        public static Dictionary<byte, bool> IsSuicideViews;
        public static bool IsMeetingReset;
        public static TextMeshPro SuicideKillText = null;
        public static void ClearAndReload()
        {
            SerialKillerPlayer = new();
            SuicideTime = CustomOptionHolder.SerialKillerSuicideTime.GetFloat();
            KillTime = CustomOptionHolder.SerialKillerKillTime.GetFloat();
            SuicideDefaultTime = SuicideTime;
            IsMeetingReset = CustomOptionHolder.SerialKillerIsMeetingReset.GetBool();
            IsSuicideView = false;
            IsSuicideViews = new Dictionary<byte, bool>();
            SuicideTimers = new Dictionary<byte, float>();
        }
    }
    public static class OverKiller
    {
        public static List<PlayerControl> OverKillerPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static int KillCount;
        public static void ClearAndReload()
        {
            OverKillerPlayer = new();
            KillCoolTime = CustomOptionHolder.OverKillerKillCoolTime.GetFloat();
            KillCount = CustomOptionHolder.OverKillerKillCount.GetInt();
        }
    }
    public static class Levelinger
    {
        public enum LevelPowerTypes
        {
            None,
            Keep,
            Pursuer,
            Teleporter,
            Sidekick,
            SpeedBooster,
            Moving
        }
        public static List<PlayerControl> LevelingerPlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsCreateMadmate;
        public static int ThisXP;
        public static int OneKillXP;
        public static int UpLevelXp;
        public static List<LevelPowerTypes> GetPowerData;
        public static bool IsUseOKRevive;
        public static int ReviveUseXP;
        public static void ClearAndReload()
        {
            try
            {
                LevelingerPlayer = new();
                ThisXP = 0;
                IsCreateMadmate = false;
                OneKillXP = CustomOptionHolder.LevelingerOneKillXP.GetInt();
                UpLevelXp = CustomOptionHolder.LevelingerUpLevelXP.GetInt();
                GetPowerData = new();
                for (int i = 0; i < 5; i++)
                {
                    string getdata = "";
                    if (i == 0) { getdata = CustomOptionHolder.LevelingerLevelOneGetPower.GetString(); }
                    else if (i == 1) { getdata = CustomOptionHolder.LevelingerLevelTwoGetPower.GetString(); }
                    else if (i == 2) { getdata = CustomOptionHolder.LevelingerLevelThreeGetPower.GetString(); }
                    else if (i == 3) { getdata = CustomOptionHolder.LevelingerLevelFourGetPower.GetString(); }
                    else if (i == 4) { getdata = CustomOptionHolder.LevelingerLevelFiveGetPower.GetString(); }
                    GetPowerData.Add(GetLevelPowerType(getdata));
                }
                IsUseOKRevive = CustomOptionHolder.LevelingerReviveXP.GetBool();
                ReviveUseXP = CustomOptionHolder.LevelingerUseXPRevive.GetInt();
            }
            catch { }
        }
        public static bool IsPower(LevelPowerTypes power)
        {
            return GetThisPower() == power;
        }
        public static LevelPowerTypes GetThisPower(int Level = 0, PlayerControl player = null)
        {
            if (player == null) player = PlayerControl.LocalPlayer;
            if (!player.IsRole(RoleId.Levelinger)) return LevelPowerTypes.None;
            if (Level == 0)
            {
                Level = ThisXP / UpLevelXp;
            }
            LevelPowerTypes thispowertype = LevelPowerTypes.None;
            if (Level <= 0) { return thispowertype; }
            thispowertype = GetPowerData[Level - 1];
            return thispowertype;
        }
        public static LevelPowerTypes GetLevelPowerType(string name)
        {
            try
            {
                return name == CustomOptionHolder.LevelingerTexts[0]
                    ? LevelPowerTypes.None
                    : name == CustomOptionHolder.LevelingerTexts[1]
                    ? LevelPowerTypes.Keep
                    : name == CustomOptionHolder.LevelingerTexts[2]
                    ? LevelPowerTypes.Pursuer
                    : name == CustomOptionHolder.LevelingerTexts[3]
                    ? LevelPowerTypes.Teleporter
                    : name == CustomOptionHolder.LevelingerTexts[4]
                    ? LevelPowerTypes.Sidekick
                    : name == CustomOptionHolder.LevelingerTexts[5]
                        ? LevelPowerTypes.SpeedBooster
                        : name == CustomOptionHolder.LevelingerTexts[6] ? LevelPowerTypes.Moving : LevelPowerTypes.None;
            }
            catch
            {
                return LevelPowerTypes.None;
            }
        }
    }
    public static class EvilMoving
    {
        public static List<PlayerControl> EvilMovingPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static void ClearAndReload()
        {
            EvilMovingPlayer = new();
            CoolTime = CustomOptionHolder.EvilMovingCoolTime.GetFloat();
        }
    }
    public static class Amnesiac
    {
        public static List<PlayerControl> AmnesiacPlayer;
        public static Color32 color = new(125, 125, 125, byte.MaxValue);
        public static void ClearAndReload()
        {
            AmnesiacPlayer = new();
        }
    }
    public static class SideKiller
    {
        public static List<PlayerControl> SideKillerPlayer;
        public static List<PlayerControl> MadKillerPlayer;
        public static Dictionary<byte, byte> MadKillerPair;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static float MadKillerCoolTime;
        public static bool IsCreateMadKiller;
        public static bool IsUpMadKiller;
        public static void ClearAndReload()
        {
            SideKillerPlayer = new();
            MadKillerPlayer = new();
            MadKillerPair = new Dictionary<byte, byte>();
            KillCoolTime = CustomOptionHolder.SideKillerKillCoolTime.GetFloat();
            MadKillerCoolTime = CustomOptionHolder.SideKillerMadKillerKillCoolTime.GetFloat();
            IsCreateMadKiller = false;
            IsUpMadKiller = false;
        }
        public static PlayerControl GetSidePlayer(PlayerControl p)
        {
            if (MadKillerPair.ContainsKey(p.PlayerId))
            {
                return ModHelpers.PlayerById(MadKillerPair[p.PlayerId]);
            }
            else if (MadKillerPair.ContainsValue(p.PlayerId))
            {
                var key = MadKillerPair.GetKey(p.PlayerId);
                return key == null ? null : ModHelpers.PlayerById((byte)key);
            }
            return null;
        }
    }
    public static class Survivor
    {
        public static List<PlayerControl> SurvivorPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static void ClearAndReload()
        {
            SurvivorPlayer = new();
            KillCoolTime = CustomOptionHolder.SurvivorKillCoolTime.GetFloat();
        }
    }
    public static class MadMayor
    {
        public static List<PlayerControl> MadMayorPlayer;
        public static Color32 color = ImpostorRed;
        public static int AddVote;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static void ClearAndReload()
        {
            MadMayorPlayer = new();
            AddVote = CustomOptionHolder.MadMayorVoteCount.GetInt();
            IsImpostorCheck = CustomOptionHolder.MadMayorIsCheckImpostor.GetBool();
            IsUseVent = CustomOptionHolder.MadMayorIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadMayorIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.MadMayorCommonTask.GetInt();
            int Long = CustomOptionHolder.MadMayorLongTask.GetInt();
            int Short = CustomOptionHolder.MadMayorShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.MadMayorCheckImpostorTask.GetString().Replace("%", "")) / 100f));
            Roles.MadMayor.CheckedImpostor = new();
        }
    }
    public static class NiceHawk
    {
        public static List<PlayerControl> NiceHawkPlayer;
        public static Color32 color = new(226, 162, 0, byte.MaxValue);
        public static float CoolTime;
        public static float DurationTime;
        public static float Timer;
        public static DateTime ButtonTimer;
        public static float Default;
        public static float CameraDefault;
        public static Vector3 Postion;
        public static float timer1;
        public static DateTime Timer2;
        public static void ClearAndReload()
        {
            NiceHawkPlayer = new();
            CoolTime = CustomOptionHolder.NiceHawkCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.NiceHawkDurationTime.GetFloat();
            Timer = 0;
            ButtonTimer = DateTime.Now;
            CameraDefault = Camera.main.orthographicSize;
            Default = FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize;
            Postion = new Vector3(0, 0, 0);
            timer1 = 0;
            Timer2 = DateTime.Now;
        }
    }
    public static class Bakery
    {
        public static List<PlayerControl> BakeryPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static void ClearAndReload()
        {
            BakeryPlayer = new();
        }
    }
    public static class MadStuntMan
    {
        public static List<PlayerControl> MadStuntManPlayer;
        public static Color32 color = ImpostorRed;
        public static Dictionary<int, int> GuardCount;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static void ClearAndReload()
        {
            MadStuntManPlayer = new();
            IsUseVent = CustomOptionHolder.MadStuntManIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadStuntManIsImpostorLight.GetBool();
        }
    }
    public static class MadHawk
    {
        public static List<PlayerControl> MadHawkPlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static float CoolTime;
        public static float DurationTime;
        public static float Timer;
        public static DateTime ButtonTimer;
        public static float Default;
        public static float CameraDefault;
        public static Vector3 Postion;
        public static float timer1;
        public static DateTime Timer2;
        public static void ClearAndReload()
        {
            MadHawkPlayer = new();
            IsUseVent = CustomOptionHolder.MadHawkIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadHawkIsImpostorLight.GetBool();
            MadHawkPlayer = new();
            CoolTime = CustomOptionHolder.MadHawkCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.MadHawkDurationTime.GetFloat();
            Timer = 0;
            ButtonTimer = DateTime.Now;
            CameraDefault = Camera.main.orthographicSize;
            Default = FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize;
            Postion = new Vector3(0, 0, 0);
            timer1 = 0;
            Timer2 = DateTime.Now;
        }
    }
    public static class MadJester
    {
        public static List<PlayerControl> MadJesterPlayer;
        public static bool IsMadJesterWin;
        public static Color32 color = ImpostorRed;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsMadJesterTaskClearWin;

        public static void ClearAndReload()
        {
            MadJesterPlayer = new();
            IsMadJesterWin = false;
            IsUseVent = CustomOptionHolder.MadJesterIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadJesterIsImpostorLight.GetBool();
            IsMadJesterTaskClearWin = CustomOptionHolder.IsMadJesterTaskClearWin.GetBool();
            IsImpostorCheck = CustomOptionHolder.MadJesterIsCheckImpostor.GetBool();
            int Common = CustomOptionHolder.MadJesterCommonTask.GetInt();
            int Long = CustomOptionHolder.MadJesterLongTask.GetInt();
            int Short = CustomOptionHolder.MadJesterShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.MadJesterCheckImpostorTask.GetString().Replace("%", "")) / 100f));
        }
    }
    public static class FalseCharges
    {
        public static List<PlayerControl> FalseChargesPlayer;
        public static Dictionary<byte, int> AllTurns;
        public static Dictionary<byte, byte> FalseChargePlayers;
        public static byte FalseChargePlayer;
        public static int Turns;
        public static int DefaultTurn;
        public static float CoolTime;
        public static Color32 color = Color.green;
        public static void ClearAndReload()
        {
            FalseChargesPlayer = new();
            AllTurns = new Dictionary<byte, int>();
            FalseChargePlayers = new Dictionary<byte, byte>();
            FalseChargePlayer = 255;
            Turns = 255;
            DefaultTurn = CustomOptionHolder.FalseChargesExileTurn.GetInt();
            CoolTime = CustomOptionHolder.FalseChargesCoolTime.GetFloat();
        }
    }
    public static class NiceTeleporter
    {
        public static List<PlayerControl> NiceTeleporterPlayer;
        public static Color32 color = new(0, 0, 128, byte.MaxValue);
        public static float CoolTime;
        public static float DurationTime;
        public static DateTime ButtonTimer;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);

        public static void ClearAndReload()
        {
            NiceTeleporterPlayer = new();
            CoolTime = CustomOptionHolder.NiceTeleporterCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.NiceTeleporterDurationTime.GetFloat();
        }
    }
    public static class Celebrity
    {
        public static List<PlayerControl> CelebrityPlayer;
        public static Color32 color = Color.yellow;
        public static bool ChangeRoleView;
        public static List<PlayerControl> ViewPlayers;
        public static float FlashTime;
        public static void ClearAndReload()
        {
            CelebrityPlayer = new();
            ChangeRoleView = CustomOptionHolder.CelebrityChangeRoleView.GetBool();
            ViewPlayers = new();
            FlashTime = DefaultKillCoolDown >= 5 ? DefaultKillCoolDown * 1000 : 5000;
        }
    }
    public static class Nocturnality
    {
        public static List<PlayerControl> NocturnalityPlayer;
        public static Color32 color = new(255, 0, 255, byte.MaxValue);
        public static void ClearAndReload()
        {
            NocturnalityPlayer = new();
        }
    }
    public static class Observer
    {
        public static List<PlayerControl> ObserverPlayer;
        public static Color32 color = new(127, 127, 127, byte.MaxValue);
        public static bool IsVoteView;
        public static void ClearAndReload()
        {
            ObserverPlayer = new();
            IsVoteView = true;
        }
    }
    public static class Vampire
    {
        public static List<PlayerControl> VampirePlayer;
        public static Color32 color = ImpostorRed;
        public static PlayerControl target;
        public static float KillDelay;
        public static float Timer;
        public static DateTime KillTimer;
        public static Dictionary<PlayerControl, PlayerControl> Targets;
        public static Dictionary<byte, List<BloodStain>> BloodStains;
        public static List<BloodStain> WaitActiveBloodStains;
        public static Dictionary<List<BloodStain>, int> NoActiveTurnWait;
        public static bool CreatedDependents;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.VampireCreateDependentsButton.png", 115f);
        public static void ClearAndReload()
        {
            VampirePlayer = new();
            target = null;
            KillDelay = CustomOptionHolder.VampireKillDelay.GetFloat();
            Timer = 0;
            KillTimer = DateTime.Now;
            Targets = new();
            BloodStains = new();
            WaitActiveBloodStains = new();
            NoActiveTurnWait = new();
            CreatedDependents = !CustomOptionHolder.VampireCanCreateDependents.GetBool();
        }
    }
    public static class Fox
    {
        public static List<PlayerControl> FoxPlayer;
        public static Color32 color = FoxPurple;
        public static Dictionary<int, int> KillGuard;
        public static bool IsUseVent;
        public static bool UseReport;
        public static bool IsImpostorLight;
        public static void ClearAndReload()
        {
            FoxPlayer = new();
            KillGuard = new();
            IsUseVent = CustomOptionHolder.FoxIsUseVent.GetBool();
            UseReport = CustomOptionHolder.FoxReport.GetBool();
            IsImpostorLight = CustomOptionHolder.FoxIsImpostorLight.GetBool();
        }
    }
    public static class DarkKiller
    {
        public static List<PlayerControl> DarkKillerPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static bool KillButtonDisable;
        public static void ClearAndReload()
        {
            DarkKillerPlayer = new();
            KillCoolTime = CustomOptionHolder.DarkKillerKillCoolTime.GetFloat();
            KillButtonDisable = false;
        }
    }
    public static class Seer
    {
        public static List<PlayerControl> SeerPlayer;
        public static Color color = new Color32(97, 178, 108, byte.MaxValue);
        public static List<Vector3> deadBodyPositions;

        public static float soulDuration;
        public static bool limitSoulDuration;
        public static int mode;

        public static void ClearAndReload()
        {
            SeerPlayer = new();
            deadBodyPositions = new();
            limitSoulDuration = CustomOptionHolder.SeerLimitSoulDuration.GetBool();
            soulDuration = CustomOptionHolder.SeerSoulDuration.GetFloat();
            mode = Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) ? 1 : CustomOptionHolder.SeerMode.GetSelection();
        }
    }
    public static class MadSeer
    {
        public static List<PlayerControl> MadSeerPlayer;
        public static Color color = ImpostorRed;
        public static List<Vector3> deadBodyPositions;

        public static float soulDuration;
        public static bool limitSoulDuration;
        public static int mode;

        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;

        public static void ClearAndReload()
        {
            MadSeerPlayer = new();
            deadBodyPositions = new();
            limitSoulDuration = CustomOptionHolder.MadSeerLimitSoulDuration.GetBool();
            soulDuration = CustomOptionHolder.MadSeerSoulDuration.GetFloat();
            mode = Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) ? 1 : CustomOptionHolder.MadSeerMode.GetSelection();

            IsImpostorCheck = CustomOptionHolder.MadSeerIsCheckImpostor.GetBool();
            IsUseVent = CustomOptionHolder.MadSeerIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadSeerIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.MadSeerCommonTask.GetInt();
            int Long = CustomOptionHolder.MadSeerLongTask.GetInt();
            int Short = CustomOptionHolder.MadSeerShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.MadSeerCheckImpostorTask.GetString().Replace("%", "")) / 100f));
            Roles.MadSeer.CheckedImpostor = new();
        }
    }
    public static class EvilSeer
    {
        public static List<PlayerControl> EvilSeerPlayer;
        public static Color32 color = ImpostorRed;
        public static List<Vector3> deadBodyPositions;

        public static float soulDuration;
        public static bool limitSoulDuration;
        public static int mode;
        public static bool IsCreateMadmate;
        public static void ClearAndReload()
        {
            EvilSeerPlayer = new();
            deadBodyPositions = new();
            limitSoulDuration = CustomOptionHolder.EvilSeerLimitSoulDuration.GetBool();
            soulDuration = CustomOptionHolder.EvilSeerSoulDuration.GetFloat();
            mode = Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) ? 1 : CustomOptionHolder.EvilSeerMode.GetSelection();
            IsCreateMadmate = CustomOptionHolder.EvilSeerMadmateSetting.GetBool();
        }
    }
    public static class RemoteSheriff
    {
        public static List<PlayerControl> RemoteSheriffPlayer;
        public static Color32 color = SheriffYellow;
        public static float CoolTime;
        public static float KillMaxCount;
        public static Dictionary<int, int> KillCount;
        public static bool IsKillTeleport;
        public static float KillCoolTime;
        public static void ClearAndReload()
        {
            RemoteSheriffPlayer = new();
            CoolTime = CustomOptionHolder.RemoteSheriffCoolTime.GetFloat();
            KillMaxCount = CustomOptionHolder.RemoteSheriffKillMaxCount.GetFloat();
            KillCount = new();
            IsKillTeleport = CustomOptionHolder.RemoteSheriffIsKillTeleportSetting.GetBool();
            KillCoolTime = CustomOptionHolder.RemoteSheriffCoolTime.GetFloat();
        }
    }
    public static class TeleportingJackal
    {
        public static List<PlayerControl> TeleportingJackalPlayer;
        public static Color32 color = JackalBlue;
        public static float KillCooldown;
        public static bool IsUseVent;
        public static bool IsUseSabo;
        public static bool IsImpostorLight;
        public static float CoolTime;
        public static float DurationTime;
        public static DateTime ButtonTimer;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);

        public static void ClearAndReload()
        {
            TeleportingJackalPlayer = new();
            KillCooldown = CustomOptionHolder.TeleportingJackalKillCooldown.GetFloat();
            IsUseVent = CustomOptionHolder.TeleportingJackalUseVent.GetBool();
            IsUseSabo = CustomOptionHolder.TeleportingJackalUseSabo.GetBool();
            IsImpostorLight = CustomOptionHolder.TeleportingJackalIsImpostorLight.GetBool();
            CoolTime = CustomOptionHolder.TeleportingJackalCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.TeleportingJackalDurationTime.GetFloat();
        }
    }
    public static class MadMaker
    {
        public static List<PlayerControl> MadMakerPlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsCreateMadmate;
        public static List<int> CreatePlayers;
        public static void ClearAndReload()
        {
            MadMakerPlayer = new();
            IsUseVent = CustomOptionHolder.MadMakerIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadMakerIsImpostorLight.GetBool();
            IsCreateMadmate = false;
            CreatePlayers = new();
        }
    }
    public static class Demon
    {
        public static List<PlayerControl> DemonPlayer;
        public static Dictionary<byte, List<PlayerControl>> CurseData;
        public static Color32 color = new(110, 0, 165, byte.MaxValue);
        public static bool IsUseVent;
        public static bool IsCheckImpostor;
        public static bool IsAliveWin;
        public static float CoolTime;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.DemonButton.png", 115f);

        public static void ClearAndReload()
        {
            DemonPlayer = new();
            CurseData = new Dictionary<byte, List<PlayerControl>>();
            IsUseVent = CustomOptionHolder.DemonIsUseVent.GetBool();
            CoolTime = CustomOptionHolder.DemonCoolTime.GetFloat();
            IsCheckImpostor = CustomOptionHolder.DemonIsCheckImpostor.GetBool();
            IsAliveWin = CustomOptionHolder.DemonIsAliveWin.GetBool();
        }
    }
    public static class TaskManager
    {
        public static List<PlayerControl> TaskManagerPlayer;
        public static Color32 color = new(153, 255, 255, byte.MaxValue);
        public static void ClearAndReload()
        {
            TaskManagerPlayer = new();
            int Common = CustomOptionHolder.TaskManagerCommonTask.GetInt();
            int Long = CustomOptionHolder.TaskManagerLongTask.GetInt();
            int Short = CustomOptionHolder.TaskManagerShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
        }
    }
    public static class SeerFriends
    {
        public static List<PlayerControl> SeerFriendsPlayer;
        public static Color32 color = JackalBlue;

        public static List<Vector3> deadBodyPositions;

        public static float soulDuration;
        public static bool limitSoulDuration;
        public static int mode;

        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsJackalCheck;
        public static int JackalCheckTask;
        public static void ClearAndReload()
        {
            SeerFriendsPlayer = new();

            deadBodyPositions = new();
            limitSoulDuration = CustomOptionHolder.SeerFriendsLimitSoulDuration.GetBool();
            soulDuration = CustomOptionHolder.SeerFriendsSoulDuration.GetFloat();
            mode = Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) ? 1 : CustomOptionHolder.SeerFriendsMode.GetSelection();

            IsJackalCheck = CustomOptionHolder.SeerFriendsIsCheckJackal.GetBool();
            IsUseVent = CustomOptionHolder.SeerFriendsIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.SeerFriendsIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.SeerFriendsCommonTask.GetInt();
            int Long = CustomOptionHolder.SeerFriendsLongTask.GetInt();
            int Short = CustomOptionHolder.SeerFriendsShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.SeerFriendsCheckJackalTask.GetString().Replace("%", "")) / 100f));
        }
    }
    public static class JackalSeer
    {
        public static List<PlayerControl> JackalSeerPlayer;
        public static List<PlayerControl> SidekickSeerPlayer;
        public static List<PlayerControl> FakeSidekickSeerPlayer;
        public static List<int> CreatePlayers;
        public static Color32 color = JackalBlue;

        public static List<Vector3> deadBodyPositions;
        public static float soulDuration;
        public static bool limitSoulDuration;
        public static int mode;

        public static float KillCooldown;
        public static bool IsUseVent;
        public static bool IsUseSabo;
        public static bool IsImpostorLight;
        public static bool CreateSidekick;
        public static bool CanCreateSidekick;
        public static bool CanCreateFriend;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JackalSeerSidekickButton.png", 115f);

        public static void ClearAndReload()
        {
            JackalSeerPlayer = new();
            SidekickSeerPlayer = new();
            FakeSidekickSeerPlayer = new();
            CreatePlayers = new();

            deadBodyPositions = new();
            limitSoulDuration = CustomOptionHolder.JackalSeerLimitSoulDuration.GetBool();
            soulDuration = CustomOptionHolder.JackalSeerSoulDuration.GetFloat();
            mode = Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) ? 1 : CustomOptionHolder.JackalSeerMode.GetSelection();

            KillCooldown = CustomOptionHolder.JackalSeerKillCooldown.GetFloat();
            IsUseVent = CustomOptionHolder.JackalSeerUseVent.GetBool();
            IsUseSabo = CustomOptionHolder.JackalSeerUseSabo.GetBool();
            IsImpostorLight = CustomOptionHolder.JackalSeerIsImpostorLight.GetBool();
            CreateSidekick = CustomOptionHolder.JackalSeerCreateSidekick.GetBool();
            CanCreateSidekick = CustomOptionHolder.JackalSeerCreateSidekick.GetBool();
            CanCreateFriend = CustomOptionHolder.JackalSeerCreateFriend.GetBool();
        }
    }
    public static class Assassin
    {
        public static List<PlayerControl> AssassinPlayer;
        public static Color32 color = ImpostorRed;
        public static List<byte> MeetingEndPlayers;
        public static PlayerControl TriggerPlayer;
        public static PlayerControl DeadPlayer;
        public static bool IsVoteView;
        public static bool IsImpostorWin;
        public static void ClearAndReload()
        {
            AssassinPlayer = new();
            MeetingEndPlayers = new();
            TriggerPlayer = null;
            DeadPlayer = null;
            IsImpostorWin = false;
            IsVoteView = CustomOptionHolder.AssassinViewVote.GetBool();
        }
    }
    public static class Marlin
    {
        public static List<PlayerControl> MarlinPlayer;
        public static Color32 color = new(175, 223, 228, byte.MaxValue);
        public static bool IsVoteView;
        public static void ClearAndReload()
        {
            MarlinPlayer = new();
            IsVoteView = CustomOptionHolder.MarlinViewVote.GetBool();
        }
    }
    public static class Arsonist
    {
        public static List<PlayerControl> ArsonistPlayer;
        public static Dictionary<byte, List<PlayerControl>> DouseData;
        public static Color32 color = new(238, 112, 46, byte.MaxValue);
        public static bool IsUseVent;
        public static float CoolTime;
        public static float DurationTime;
        public static bool TriggerArsonistWin;
        public static bool IsDouse;
        public static PlayerControl DouseTarget;
        public static Sprite GetDouseButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ArsonistDouse.png", 115f);
        public static Sprite GetIgniteButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ArsonistIgnite.png", 115f);


        public static void ClearAndReload()
        {
            ArsonistPlayer = new();
            DouseData = new Dictionary<byte, List<PlayerControl>>();
            IsUseVent = CustomOptionHolder.ArsonistIsUseVent.GetBool();
            CoolTime = CustomOptionHolder.ArsonistCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.ArsonistDurationTime.GetFloat();
            TriggerArsonistWin = true;
            IsDouse = false;
            DouseTarget = null;
        }
    }
    public static class Chief
    {
        public static List<PlayerControl> ChiefPlayer;
        public static List<byte> SheriffPlayer;
        public static List<byte> NoTaskSheriffPlayer;
        public static Color32 color = SheriffYellow;
        public static bool IsCreateSheriff;
        public static float CoolTime;
        public static int KillLimit;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ChiefSidekickButton.png", 115f);

        public static void ClearAndReload()
        {
            ChiefPlayer = new();
            SheriffPlayer = new();
            NoTaskSheriffPlayer = new();
            IsCreateSheriff = false;
            CoolTime = CustomOptionHolder.ChiefSheriffCoolTime.GetFloat();
            KillLimit = CustomOptionHolder.ChiefSheriffKillLimit.GetInt();
        }
    }
    public static class Cleaner
    {
        public static List<PlayerControl> CleanerPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static int CleanMaxCount;
        public static float KillCoolTime;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CleanerButton.png", 115f);

        public static void ClearAndReload()
        {
            CleanerPlayer = new();
            CoolTime = CustomOptionHolder.CleanerCooldown.GetFloat();
            KillCoolTime = CustomOptionHolder.CleanerKillCoolTime.GetFloat();
        }
    }
    public static class MadCleaner
    {
        public static List<PlayerControl> MadCleanerPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static bool IsUseVent;
        public static bool IsImpostorLight;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CleanerButton.png", 115f);

        public static void ClearAndReload()
        {
            MadCleanerPlayer = new();
            CoolTime = CustomOptionHolder.MadCleanerCooldown.GetFloat();
            IsUseVent = CustomOptionHolder.MadCleanerIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MadCleanerIsImpostorLight.GetBool();
        }
    }
    public static class Samurai
    {
        public static List<PlayerControl> SamuraiPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static float SwordCoolTime;
        public static bool UseVent;
        public static bool UseSabo;
        public static bool Sword;
        public static List<byte> SwordedPlayer;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SamuraiButton.png", 115f);

        public static void ClearAndReload()
        {
            SamuraiPlayer = new();
            KillCoolTime = CustomOptionHolder.SamuraiKillCoolTime.GetFloat();
            SwordCoolTime = CustomOptionHolder.SamuraiSwordCoolTime.GetFloat();
            UseVent = CustomOptionHolder.SamuraiVent.GetBool();
            UseSabo = CustomOptionHolder.SamuraiSabo.GetBool();
            Sword = false;
            SwordedPlayer = new();
        }
    }
    public static class MayorFriends
    {
        public static List<PlayerControl> MayorFriendsPlayer;
        public static Color32 color = JackalBlue;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static bool IsJackalCheck;
        public static int JackalCheckTask;
        public static int AddVote;
        public static void ClearAndReload()
        {
            MayorFriendsPlayer = new();
            IsJackalCheck = CustomOptionHolder.MayorFriendsIsCheckJackal.GetBool();
            IsUseVent = CustomOptionHolder.MayorFriendsIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.MayorFriendsIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.MayorFriendsCommonTask.GetInt();
            int Long = CustomOptionHolder.MayorFriendsLongTask.GetInt();
            int Short = CustomOptionHolder.MayorFriendsShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.MayorFriendsCheckJackalTask.GetString().Replace("%", "")) / 100f));
            AddVote = CustomOptionHolder.MayorFriendsVoteCount.GetInt();
        }
    }
    public static class VentMaker
    {
        public static List<PlayerControl> VentMakerPlayer;
        public static Color32 color = ImpostorRed;
        public static Vent Vent;
        public static int VentCount;
        public static bool IsMakeVent;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.VentMakerButton.png", 115f);

        public static void ClearAndReload()
        {
            VentMakerPlayer = new();
            Vent = null;
            VentCount = 0;
            IsMakeVent = true;
        }
    }
    public static class GhostMechanic
    {
        public static List<PlayerControl> GhostMechanicPlayer;
        public static Color32 color = new(25, 68, 142, byte.MaxValue);
        public static int LimitCount;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.GhostMechanicRepairButton.png", 115f);

        public static void ClearAndReload()
        {
            GhostMechanicPlayer = new();
            LimitCount = CustomOptionHolder.GhostMechanicRepairLimit.GetInt();
        }
    }
    public static class EvilHacker
    {
        public static List<PlayerControl> EvilHackerPlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsCreateMadmate;
        public static bool IsMyAdmin;
        public static Sprite GetCreateMadmateButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CreateMadmateButton.png", 115f);

        public static Sprite GetButtonSprite()
        {
            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
            if (mapId is 0 or 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
            else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
            else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
            return button.Image;
        }
        public static void ClearAndReload()
        {
            EvilHackerPlayer = new();
            IsCreateMadmate = CustomOptionHolder.EvilHackerMadmateSetting.GetBool();
            IsMyAdmin = false;
        }
    }
    public static class HauntedWolf
    {
        public static List<PlayerControl> HauntedWolfPlayer;
        public static Color32 color = new(50, 0, 25, byte.MaxValue);
        public static void ClearAndReload()
        {
            HauntedWolfPlayer = new();
        }
    }
    public static class PositionSwapper
    {
        public static List<PlayerControl> PositionSwapperPlayer;
        public static Color32 color = ImpostorRed;
        public static int SwapCount;
        public static float CoolTime;
        public static DateTime ButtonTimer;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PositionSwapperButton.png", 115f);

        public static void ClearAndReload()
        {
            PositionSwapperPlayer = new();
            CoolTime = CustomOptionHolder.PositionSwapperCoolTime.GetFloat();
            SwapCount = CustomOptionHolder.PositionSwapperSwapCount.GetInt();
        }
    }

    public static class Tuna
    {
        public static List<PlayerControl> TunaPlayer;
        public static Color32 color = new(0, 255, 255, byte.MaxValue);
        public static Dictionary<byte, Vector2> Position;
        public static float Timer;
        public static float StoppingTime;
        public static bool IsUseVent;
        public static Dictionary<byte, float> Timers;
        public static bool IsMeetingEnd;
        public static bool IsTunaAddWin;
        public static void ClearAndReload()
        {
            TunaPlayer = new();
            Position = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers) Position[p.PlayerId] = new Vector3(9999f, 9999f, 9999f);
            StoppingTime = CustomOptionHolder.TunaStoppingTime.GetFloat();
            if (Mode.ModeHandler.IsMode(Mode.ModeId.Default)) Timer = StoppingTime;
            IsUseVent = CustomOptionHolder.TunaIsUseVent.GetBool();
            IsTunaAddWin = CustomOptionHolder.TunaIsAddWin.GetBool();
            IsMeetingEnd = false;
            if (Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles))
            {
                Timers = new();
                foreach (PlayerControl p in CachedPlayer.AllPlayers) Timers[p.PlayerId] = StoppingTime;
            }
        }
    }
    public static class Mafia
    {
        public static List<PlayerControl> MafiaPlayer;
        public static Color32 color = ImpostorRed;
        public static bool CachedIs;
        public static void ClearAndReload()
        {
            MafiaPlayer = new();
            CachedIs = false;
        }
    }
    public static class BlackCat
    {
        public static List<PlayerControl> BlackCatPlayer;
        public static Color32 color = ImpostorRed;
        public static bool NotImpostorExiled;
        public static bool IsImpostorCheck;
        public static int ImpostorCheckTask;
        public static bool IsUseVent;
        public static bool IsImpostorLight;
        public static void ClearAndReload()
        {
            BlackCatPlayer = new();
            NotImpostorExiled = CustomOptionHolder.BlackCatNotImpostorExiled.GetBool();
            IsImpostorCheck = CustomOptionHolder.BlackCatIsCheckImpostor.GetBool();
            IsUseVent = CustomOptionHolder.BlackCatIsUseVent.GetBool();
            IsImpostorLight = CustomOptionHolder.BlackCatIsImpostorLight.GetBool();
            int Common = CustomOptionHolder.BlackCatCommonTask.GetInt();
            int Long = CustomOptionHolder.BlackCatLongTask.GetInt();
            int Short = CustomOptionHolder.BlackCatShortTask.GetInt();
            int AllTask = Common + Long + Short;
            if (AllTask == 0)
            {
                Common = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
                Long = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
                Short = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            }
            ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptionHolder.BlackCatCheckImpostorTask.GetString().Replace("%", "")) / 100f));
        }
    }

    public static class SecretlyKiller
    {
        public static List<PlayerControl> SecretlyKillerPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static bool IsKillCoolChange;
        public static bool IsBlackOutKillCharge;
        public static int SecretlyKillLimit;
        public static float SecretlyKillCoolTime;

        public static float MainCool;
        public static float SecretlyCool;

        public static PlayerControl target;
        public static DateTime ButtonTimer;
        public static Sprite buttonSprite;
        public static void ClearAndReload()
        {
            SecretlyKillerPlayer = new();
            KillCoolTime = CustomOptionHolder.SecretlyKillerKillCoolTime.GetFloat();
            IsKillCoolChange = CustomOptionHolder.SecretlyKillerIsKillCoolTimeChange.GetBool();
            IsBlackOutKillCharge = CustomOptionHolder.SecretlyKillerIsBlackOutKillCharge.GetBool();
            SecretlyKillLimit = CustomOptionHolder.SecretlyKillerSecretKillLimit.GetInt();
            SecretlyKillCoolTime = CustomOptionHolder.SecretlyKillerSecretKillCoolTime.GetFloat();
        }
    }

    public static class Spy
    {
        public static List<PlayerControl> SpyPlayer;
        public static Color32 color = ImpostorRed;
        public static bool CanUseVent;
        public static void ClearAndReload()
        {
            SpyPlayer = new();
            CanUseVent = CustomOptionHolder.SpyCanUseVent.GetBool();
        }
    }
    public static class Kunoichi
    {
        public static List<PlayerControl> KunoichiPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static int KillKunai;
        public static Kunai Kunai;
        public static Kunai SendKunai;
        public static List<Kunai> Kunais = new();
        public static Dictionary<byte, Dictionary<byte, int>> HitCount;
        public static bool KunaiSend;
        public static bool HideKunai;
        public static float MouseAngle;
        public static Vector2 OldPosition;
        public static float StopTime;
        public static float HideTime;
        public static bool IsWaitAndPressTheButtonToHide;
        public static bool IsHideButton;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.KunoichiKunaiButton.png", 115f);
        public static Sprite GetHideButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.KunoichiHideButton.png", 115f);

        public static void ClearAndReload()
        {
            HideKunai = CustomOptionHolder.KunoichiHideKunai.GetBool();
            OldPosition = new();
            StopTime = 0;
            HideTime = CustomOptionHolder.KunoichiIsHide.GetBool() ? CustomOptionHolder.KunoichiHideTime.GetFloat() : -1;
            IsWaitAndPressTheButtonToHide = CustomOptionHolder.KunoichiIsWaitAndPressTheButtonToHide.GetBool();
            IsHideButton = false;
            KunoichiPlayer = new();
            KillCoolTime = CustomOptionHolder.KunoichiCoolTime.GetFloat();
            KillKunai = CustomOptionHolder.KunoichiKillKunai.GetInt();
            HitCount = new();
            if (Kunai != null) { GameObject.Destroy(Kunai.kunai); }
            if (SendKunai != null) { GameObject.Destroy(SendKunai.kunai); }
            if (Kunais.Count > 0)
            {
                foreach (Kunai kunai in Kunais)
                {
                    if (kunai != null)
                    {
                        GameObject.Destroy(kunai.kunai);
                    }
                }
            }
            Kunais = new();
            SendKunai = null;
            Kunai = new Kunai();
            Kunai.kunai.SetActive(false);
            KunaiSend = false;
        }
    }
    public static class DoubleKiller
    {
        public static List<PlayerControl> DoubleKillerPlayer;
        public static Color32 color = ImpostorRed;
        public static void ClearAndReload()
        {
            DoubleKillerPlayer = new();
        }
    }
    public static class Smasher
    {
        public static List<PlayerControl> SmasherPlayer;
        public static Color32 color = ImpostorRed;
        public static float KillCoolTime;
        public static bool SmashOn;
        public static void ClearAndReload()
        {
            SmasherPlayer = new();
            KillCoolTime = CustomOptionHolder.SmasherKillCoolTime.GetFloat();
            SmashOn = false;
        }
    }
    public static class SuicideWisher
    {
        public static List<PlayerControl> SuicideWisherPlayer;
        public static Color32 color = ImpostorRed;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SuicideWisherButton.png", 115f);

        public static void ClearAndReload()
        {
            SuicideWisherPlayer = new();
        }
    }
    public static class Neet
    {
        public static List<PlayerControl> NeetPlayer;
        public static Color32 color = new(127, 127, 127, byte.MaxValue);
        public static bool IsAddWin;
        public static void ClearAndReload()
        {
            NeetPlayer = new();
            IsAddWin = CustomOptionHolder.NeetIsAddWin.GetBool();
        }
    }
    public static class FastMaker
    {
        public static List<PlayerControl> FastMakerPlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsCreatedMadmate;
        public static List<int> CreatePlayers;
        public static void ClearAndReload()
        {
            FastMakerPlayer = new();
            IsCreatedMadmate = false;
            CreatePlayers = new();
        }
    }
    public static class ToiletFan
    {
        public static List<PlayerControl> ToiletFanPlayer;
        public static Color32 color = new(116, 80, 48, byte.MaxValue);
        public static float ToiletCool;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ToiletFanButton.png", 115f);

        public static void ClearAndReload()
        {
            ToiletFanPlayer = new();
            ToiletCool = CustomOptionHolder.ToiletFanCoolTime.GetFloat();
        }
    }
    public static class EvilButtoner
    {
        public static List<PlayerControl> EvilButtonerPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static float SkillCount;
        public static Dictionary<int, int> SkillCountSHR;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ButtonerButton.png", 115f);
        public static void ClearAndReload()
        {
            EvilButtonerPlayer = new();
            CoolTime = CustomOptionHolder.EvilButtonerCoolTime.GetFloat();
            SkillCount = CustomOptionHolder.EvilButtonerCount.GetFloat();
            SkillCountSHR = new();
        }
    }
    public static class NiceButtoner
    {
        public static List<PlayerControl> NiceButtonerPlayer;
        public static Color32 color = new(0, 128, 128, byte.MaxValue);
        public static float CoolTime;
        public static float SkillCount;
        public static Dictionary<int, int> SkillCountSHR;
        public static void ClearAndReload()
        {
            NiceButtonerPlayer = new();
            CoolTime = CustomOptionHolder.NiceButtonerCoolTime.GetFloat();
            SkillCount = CustomOptionHolder.NiceButtonerCount.GetFloat();
            SkillCountSHR = new();
        }
    }
    public static class Finder
    {
        public static List<PlayerControl> FinderPlayer;
        public static Color32 color = ImpostorRed;
        public static int CheckMadmateKillCount;
        public static int KillCount;
        public static Dictionary<byte, int> KillCounts;
        public static bool IsCheck
        {
            get
            {
                return CheckMadmateKillCount <= KillCount;
            }
        }
        public static void ClearAndReload()
        {
            FinderPlayer = new();
            CheckMadmateKillCount = CustomOptionHolder.FinderCheckMadmateSetting.GetInt();
            KillCount = 0;
            KillCounts = new();
        }
    }
    public static class Revolutionist
    {
        public static List<PlayerControl> RevolutionistPlayer;
        public static Color32 color = new(255, 0, 51, byte.MaxValue);
        public static float CoolTime;
        public static float TouchTime;
        public static List<byte> RevolutionedPlayerId;
        public static List<PlayerControl> RevolutionedPlayer
        {
            get
            {
                if (_revolutionedPlayer.Length != RevolutionedPlayerId.Count)
                {
                    List<PlayerControl> newList = new();
                    foreach (byte playerid in RevolutionedPlayerId)
                    {
                        PlayerControl player = ModHelpers.PlayerById(playerid);
                        if (player == null) continue;
                        newList.Add(player);
                    }
                    _revolutionedPlayer = newList.ToArray();
                }
                return _revolutionedPlayer.ToList();
            }
        }
        public static PlayerControl[] _revolutionedPlayer;
        public static bool IsAddWin;
        public static bool IsAddWinAlive;
        public static PlayerControl CurrentTarget;
        public static PlayerControl MeetingTrigger;
        public static bool IsEndMeeting;
        public static PlayerControl WinPlayer;
        public static void ClearAndReload()
        {
            RevolutionistPlayer = new();
            CoolTime = CustomOptionHolder.RevolutionistCoolTime.GetFloat();
            TouchTime = CustomOptionHolder.RevolutionistTouchTime.GetFloat();
            RevolutionedPlayerId = new();
            _revolutionedPlayer = new PlayerControl[] { };
            IsAddWin = CustomOptionHolder.RevolutionistAddWin.GetBool();
            IsAddWinAlive = CustomOptionHolder.RevolutionistAddWinIsAlive.GetBool();
            CurrentTarget = null;
            MeetingTrigger = null;
            IsEndMeeting = false;
            WinPlayer = null;
        }
    }
    public static class Dictator
    {
        public static List<PlayerControl> DictatorPlayer;
        public static Color32 color = new(0, 102, 51, byte.MaxValue);
        public static int VoteCount;
        public static int SubExileLimit;
        public static Dictionary<byte, int> SubExileLimitData;
        public static void ClearAndReload()
        {
            DictatorPlayer = new();
            VoteCount = CustomOptionHolder.DictatorVoteCount.GetInt();
            SubExileLimit = CustomOptionHolder.DictatorSubstituteExile.GetBool() ? CustomOptionHolder.DictatorSubstituteExileLimit.GetInt() : 0;
            SubExileLimitData = new();
        }
    }

    public static class Spelunker
    {
        public static List<PlayerControl> SpelunkerPlayer;
        public static Color32 color = new(255, 255, 0, byte.MaxValue);
        public static bool IsVentChecked;
        public static int VentDeathChance;
        public static int LadderDeathChance;
        public static float CommsOrLightdownDeathTime;
        public static float CommsOrLightdownTime;
        public static int LiftDeathChance;
        public static int DoorOpenChance;
        public static void ClearAndReload()
        {
            SpelunkerPlayer = new();
            IsVentChecked = false;
            VentDeathChance = CustomOptionHolder.SpelunkerVentDeathChance.GetSelection();
            LadderDeathChance = CustomOptionHolder.SpelunkerLadderDeadChance.GetSelection();
            CommsOrLightdownDeathTime = CustomOptionHolder.SpelunkerIsDeathCommsOrPowerdown.GetBool() ? CustomOptionHolder.SpelunkerDeathCommsOrPowerdownTime.GetFloat() : -1f;
            CommsOrLightdownTime = 0f;
            LiftDeathChance = CustomOptionHolder.SpelunkerLiftDeathChance.GetSelection();
            Neutral.Spelunker.DeathPosition = null;
            DoorOpenChance = CustomOptionHolder.SpelunkerDoorOpenChance.GetSelection();
        }
    }

    public static class SuicidalIdeation
    {
        public static List<PlayerControl> SuicidalIdeationPlayer;
        public static Color32 color = new(71, 71, 71, byte.MaxValue);
        public static DateTime ButtonTimer;
        public static int CompletedTask;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SuicidalIdeationButton.png", 115f);
        public static void ClearAndReload()
        {
            SuicidalIdeationPlayer = new();
            ButtonTimer = DateTime.Now;
            CompletedTask = 0;
        }
    }
    public static class Hitman
    {
        public static List<PlayerControl> HitmanPlayer;
        public static Color32 color = new(86, 41, 18, byte.MaxValue);
        public static int OutMissionLimit;
        public static PlayerControl Target;
        public static float UpdateTime;
        public static Arrow TargetArrow;
        public static float ArrowUpdateTimeDefault;
        public static float ArrowUpdateTime;
        public static int WinKillCount;
        public static Vector3 ArrowPosition;
        public static TextMeshPro cooldownText;
        public static void ClearAndReload()
        {
            HitmanPlayer = new();
            OutMissionLimit = CustomOptionHolder.HitmanIsOutMission.GetBool() ? CustomOptionHolder.HitmanOutMissionLimit.GetInt() : -1;
            UpdateTime = CustomOptionHolder.HitmanChangeTargetTime.GetFloat();
            cooldownText = null;
            WinKillCount = CustomOptionHolder.HitmanWinKillCount.GetInt();
            if (TargetArrow != null && TargetArrow.arrow != null)
            {
                UnityEngine.Object.Destroy(TargetArrow.arrow);
            }
            TargetArrow = null;
            ArrowUpdateTimeDefault = CustomOptionHolder.HitmanIsArrowView.GetBool() ? CustomOptionHolder.HitmanArrowUpdateTime.GetFloat() : -1;
            ArrowUpdateTime = ArrowUpdateTimeDefault;
        }
    }
    public static class Matryoshka
    {
        public static List<PlayerControl> MatryoshkaPlayer;
        public static Color32 color = ImpostorRed;
        public static int WearLimit;
        public static float WearDefaultTime;
        public static float WearTime;
        public static float MyKillCoolTime;
        public static bool IsLocalOn => !Data.Keys.All(data => data != CachedPlayer.LocalPlayer.PlayerId);
        public static Dictionary<byte, DeadBody> Data;
        public static Sprite PutOnButtonSprite => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MatryoshkaPutOnButton.png", 115f);
        public static Sprite TakeOffButtonSprite => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MatryoshkaTakeOffButton.png", 115f);
        public static void ClearAndReload()
        {
            MatryoshkaPlayer = new();
            WearLimit = CustomOptionHolder.MatryoshkaWearLimit.GetInt();
            WearTime = 0;
            Data = new();
            MyKillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        }
    }
    public static class Nun
    {
        public static List<PlayerControl> NunPlayer;
        public static Color32 color = ImpostorRed;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.NunButton.png", 115f);
        public static void ClearAndReload()
        {
            NunPlayer = new();
        }
    }
    public static class PartTimer
    {
        public static List<PlayerControl> PartTimerPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static int DeathDefaultTurn;
        public static int DeathTurn;
        public static Dictionary<byte, byte> Data;
        public static bool IsLocalOn => Data.ContainsKey(CachedPlayer.LocalPlayer.PlayerId);
        public static PlayerControl CurrentTarget => IsLocalOn ? ModHelpers.PlayerById(Data[CachedPlayer.LocalPlayer.PlayerId]) : null;

        public static Dictionary<PlayerControl, PlayerControl> PlayerData
        {
            get
            {
                //キャッシュ済みのプレイヤーリストとplayerByIdのリストの数が違ったらキャッシュを更新する
                if (_playerData.Count != Data.Count)
                {
                    Dictionary<PlayerControl, PlayerControl> newdic = new();
                    foreach (var data in Data)
                    {
                        newdic.Add(ModHelpers.PlayerById(data.Key), ModHelpers.PlayerById(data.Value));
                    }
                    _playerData = newdic;
                }
                return _playerData;
            }
        }
        private static Dictionary<PlayerControl, PlayerControl> _playerData;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PartTimerButton.png", 115f);
        public static void ClearAndReload()
        {
            PartTimerPlayer = new();
            DeathTurn = DeathDefaultTurn = CustomOptionHolder.PartTimerDeathTurn.GetInt();
            Data = new();
            _playerData = new();
        }
    }

    public static class SatsumaAndImo
    {
        public static List<PlayerControl> SatsumaAndImoPlayer;
        public static Color32 color = new(153, 0, 68, byte.MaxValue);
        public static int TeamNumber;
        public static void ClearAndReload()
        {
            SatsumaAndImoPlayer = new();
            TeamNumber = 1;
            //1=クルー
            //2=マッド
        }
    }
    public static class Painter
    {
        public static List<PlayerControl> PainterPlayer;
        public static Color32 color = new(170, 255, 0, byte.MaxValue);
        public static Dictionary<Crewmate.Painter.ActionType, List<Vector2>> ActionData;
        public static List<Footprint> Prints;
        public static Dictionary<Crewmate.Painter.ActionType, bool> IsEnables;
        public static bool IsLocalActionSend;
        public static bool IsDeathFootpointBig;
        public static bool IsFootprintMeetingDestroy;
        public static PlayerControl CurrentTarget;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PainterButton.png", 115f);
        public static void ClearAndReload()
        {
            PainterPlayer = new();
            ActionData = new();
            IsEnables = new();
            foreach (Crewmate.Painter.ActionType type in Enum.GetValues(typeof(Crewmate.Painter.ActionType)))
            {
                ActionData[type] = new();
            }
            Prints = new();
            CurrentTarget = null;
            IsLocalActionSend = false;
            IsEnables[Crewmate.Painter.ActionType.TaskComplete] = CustomOptionHolder.PainterIsTaskCompleteFootprint.GetBool();
            IsEnables[Crewmate.Painter.ActionType.SabotageRepair] = CustomOptionHolder.PainterIsSabotageRepairFootprint.GetBool();
            IsEnables[Crewmate.Painter.ActionType.InVent] = CustomOptionHolder.PainterIsInVentFootprint.GetBool();
            IsEnables[Crewmate.Painter.ActionType.ExitVent] = CustomOptionHolder.PainterIsExitVentFootprint.GetBool();
            IsEnables[Crewmate.Painter.ActionType.CheckVital] = CustomOptionHolder.PainterIsCheckVitalFootprint.GetBool();
            IsEnables[Crewmate.Painter.ActionType.CheckAdmin] = CustomOptionHolder.PainterIsCheckAdminFootprint.GetBool();
            IsEnables[Crewmate.Painter.ActionType.Death] = CustomOptionHolder.PainterIsDeathFootprint.GetBool();
            IsDeathFootpointBig = CustomOptionHolder.PainterIsDeathFootprintBig.GetBool();
            IsFootprintMeetingDestroy = CustomOptionHolder.PainterIsFootprintMeetingDestroy.GetBool();
        }
    }
    public static class Psychometrist
    {
        public static List<PlayerControl> PsychometristPlayer;
        public static Color32 color = new(238, 130, 238, byte.MaxValue);
        //(source, target) : Vector2
        public static Dictionary<(byte, byte), (List<Vector2>, bool)> FootprintsPosition;
        public static Dictionary<(byte, byte), float> FootprintsDeathTime;
        public static Dictionary<(byte, byte), List<Footprint>> FootprintObjects;
        public static float UpdateTime;
        public static float Distance = 0.5f;
        //(死体, テキスト, 誤差)
        public static List<(DeadBody, TextMeshPro, int)> DeathTimeTexts;
        public static DeadBody CurrentTarget;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PsychometristButton.png", 115f);

        public static void ClearAndReload()
        {
            PsychometristPlayer = new();
            UpdateTime = 0.1f;
            CurrentTarget = null;
            DeathTimeTexts = new();
            FootprintsPosition = new();
            FootprintObjects = new();
            FootprintsDeathTime = new();
        }
    }
    public static class SeeThroughPerson
    {
        public static List<PlayerControl> SeeThroughPersonPlayer;
        public static Color32 color = new(157, 204, 224, byte.MaxValue);
        public static List<EdgeCollider2D> Objects;
        public static void ClearAndReload()
        {
            SeeThroughPersonPlayer = new();
            Objects = new();
        }
    }
    public static class Photographer
    {
        public static List<PlayerControl> PhotographerPlayer;
        public static Color32 color = new(141, 160, 182, byte.MaxValue);
        public static int BonusCount;
        public static List<byte> PhotedPlayerIds;
        public static bool IsPhotographerShared;
        public static List<PlayerControl> PhotedPlayer
        {
            get
            {
                if (PhotedPlayerIds.Count != _photedPlayer.Count)
                {
                    List<PlayerControl> NewList = new();
                    foreach (byte playerid in PhotedPlayerIds)
                    {
                        PlayerControl player = ModHelpers.PlayerById(playerid);
                        if (player) NewList.Add(player);
                    }
                    _photedPlayer = NewList;
                }
                return _photedPlayer;
            }
        }
        public static List<PlayerControl> _photedPlayer;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PhotographerButton.png", 115f);
        public static void ClearAndReload()
        {
            PhotographerPlayer = new();
            PhotedPlayerIds = new();
            _photedPlayer = new();
            IsPhotographerShared = false;
            BonusCount = CustomOptionHolder.PhotographerIsBonus.GetBool() ? CustomOptionHolder.PhotographerBonusCount.GetInt() : -1;
        }
    }
    public static class Stefinder
    {
        public static List<PlayerControl> StefinderPlayer;
        public static Color32 color = new(0, 255, 0, byte.MaxValue);
        public static List<byte> IsKillPlayer;
        public static bool IsKill;
        public static PlayerControl target;
        public static DateTime ButtonTimer;
        public static void ClearAndReload()
        {
            StefinderPlayer = new();
            IsKill = false;
            IsKillPlayer = new();
        }
    }
    public static class Slugger
    {
        public static List<PlayerControl> SluggerPlayer;
        public static Color32 color = ImpostorRed;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SluggerButton.png", 115f);
        public static void ClearAndReload()
        {
            SluggerPlayer = new();
        }
    }
    public static class ConnectKiller
    {
        public static List<PlayerControl> ConnectKillerPlayer;
        public static Color32 color = ImpostorRed;
        public static bool OldCommsData;
        public static void ClearAndReload()
        {
            ConnectKillerPlayer = new();
            OldCommsData = false;
        }
    }
    public static class Cracker
    {
        public static List<PlayerControl> CrackerPlayer;
        public static Color32 color = ImpostorRed;
        public static List<byte> CrackedPlayers;
        public static List<byte> currentCrackedPlayers;
        public static int DefaultCount;
        public static int TurnCount;
        public static int MaxTurnCount;
        public static List<PlayerControl> CurrentCrackedPlayerControls
        {
            get
            {
                if (currentCrackedPlayerControls.Count != currentCrackedPlayers.Count)
                {
                    List<PlayerControl> newList = new();
                    foreach (byte p in currentCrackedPlayers) newList.Add(ModHelpers.PlayerById(p));
                    currentCrackedPlayerControls = newList;
                }
                return currentCrackedPlayerControls;
            }
        }
        private static List<PlayerControl> currentCrackedPlayerControls;
        public static void ClearAndReload()
        {
            CrackerPlayer = new();
            CrackedPlayers = new();
            currentCrackedPlayers = new();
            MaxTurnCount = CustomOptionHolder.CrackerAllTurnSelectCount.GetInt();
            DefaultCount = CustomOptionHolder.CrackerOneTurnSelectCount.GetInt();
            TurnCount = DefaultCount;
            currentCrackedPlayerControls = new();
        }
    }

    public static class WaveCannon
    {
        public static List<PlayerControl> WaveCannonPlayer;
        public static Color32 color = ImpostorRed;
        public static List<byte> CannotMurderPlayers;
        public static bool IsLocalOn => WaveCannonObject.Objects.FirstOrDefault(x => x.Owner != null && x.Owner.PlayerId == CachedPlayer.LocalPlayer.PlayerId) != null;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannonButton.png", 115f);

        public static void ClearAndReload()
        {
            WaveCannonPlayer = new();
            WaveCannonObject.Ids = new();
            CannotMurderPlayers = new();
        }
    }
    public static class Doppelganger
    {
        public static List<PlayerControl> DoppelggerPlayer;
        public static Color32 color = ImpostorRed;
        public static float DurationTime;
        public static float CoolTime;
        public static float SucCool;
        public static float NotSucCool;
        public static float Duration;
        public static TextMeshPro DoppelgangerDurationText = null;
        public static Dictionary<byte, PlayerControl> Targets;
        public static float CurrentCool;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.DoppelgangerButton.png", 115f);
        public static void ClearAndReload()
        {
            DoppelggerPlayer = new();
            DurationTime = CustomOptionHolder.DoppelgangerDurationTime.GetFloat();
            CoolTime = CustomOptionHolder.DoppelgangerCoolTime.GetFloat();
            SucCool = CustomOptionHolder.DoppelgangerSucTime.GetFloat();
            NotSucCool = CustomOptionHolder.DoppelgangerNotSucTime.GetFloat();
            Duration = DurationTime + 1.1f;
            Targets = new();
            CurrentCool = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        }
    }
    public static class GM
    {
        public static PlayerControl gm;
        public static Color32 color = new(255, 91, 112, byte.MaxValue);
        public static void ClearAndReload()
        {
            gm = null;
        }
    }
    public static class Pavlovsdogs
    {
        public static List<PlayerControl> PavlovsdogsPlayer;
        public static Color32 color = new(244, 169, 106, byte.MaxValue);
        public static bool IsOwnerDead
        {
            get
            {
                return Pavlovsowner.PavlovsownerPlayer.All(x => x.IsDead());
            }
        }
        public static float DeathTime;
        public static void ClearAndReload()
        {
            PavlovsdogsPlayer = new();
            DeathTime = CustomOptionHolder.PavlovsdogRunAwayDeathTime.GetFloat();
        }
    }
    public static class Pavlovsowner
    {
        public static List<PlayerControl> PavlovsownerPlayer;
        public static Color32 color = Pavlovsdogs.color;
        public static bool CanCreateDog => (CurrentChildPlayer == null || CurrentChildPlayer.IsDead()) && CreateLimit > 0;
        public static PlayerControl CurrentChildPlayer;
        public static Arrow DogArrow;
        public static int CreateLimit;
        public static Dictionary<byte, int> CountData;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PavlovsownerCreatedogButton.png", 115f);
        public static void ClearAndReload()
        {
            PavlovsownerPlayer = new();
            CurrentChildPlayer = null;
            if (DogArrow != null) GameObject.Destroy(DogArrow.arrow);
            DogArrow = new(color);
            DogArrow.arrow.SetActive(false);
            CreateLimit = CustomOptionHolder.PavlovsownerCreateDogLimit.GetInt();
            CountData = new();
        }
    }
    public static class Camouflager
    {
        public static List<PlayerControl> CamouflagerPlayer;
        public static Color32 color = ImpostorRed;
        public static float CoolTime;
        public static float DurationTime;
        public static bool ArsonistMark;
        public static bool DemonMark;
        public static bool LoversMark;
        public static bool QuarreledMark;
        public static byte Color;
        private static Sprite buttonSprite;
        public static DateTime ButtonTimer;
        public static float CamoDurationData;
        public static bool IsCamouflage;
        public static float Duration;
        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CamouflagerButton.png", 115f);
            return buttonSprite;
        }
        public static void ClearAndReload()
        {
            CamouflagerPlayer = new();
            CamoDurationData = 0;
            CoolTime = CustomOptionHolder.CamouflagerCoolTime.GetFloat();
            DurationTime = CustomOptionHolder.CamouflagerDurationTime.GetFloat();
            ArsonistMark = CustomOptionHolder.CamouflagerCamouflageArsonist.GetBool();
            DemonMark = CustomOptionHolder.CamouflagerCamouflageDemon.GetBool();
            LoversMark = CustomOptionHolder.CamouflagerCamouflageLovers.GetBool();
            QuarreledMark = CustomOptionHolder.CamouflagerCamouflageQuarreled.GetBool();
            Color = (byte)(CustomOptionHolder.CamouflagerCamouflageChangeColor.GetBool() ? CustomOptionHolder.CamouflagerCamouflageColor.GetSelection() : 15);
            ButtonTimer = DateTime.Now;
            IsCamouflage = false;
            Duration = DurationTime;
            Impostor.Camouflager.Attire = new();
        }
    }
    public static class Werewolf
    {
        public static List<PlayerControl> WerewolfPlayer;
        public static Color32 color = ImpostorRed;
        public static bool IsShooted;
        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WereWolfButton.png", 200f);
        public static void ClearAndReload()
        {
            WerewolfPlayer = new();
            IsShooted = false;
        }
    }
    public static class Cupid
    {
        public static List<PlayerControl> CupidPlayer;
        public static Color32 color = Lovers.color;
        public static PlayerControl currentLovers;
        public static PlayerControl currentTarget;
        public static bool Created;
        public static Dictionary<byte, byte> CupidLoverPair;

        public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.cupidButton.png", 115f);

        public static void ClearAndReload()
        {
            CupidPlayer = new();
            currentLovers = null;
            currentTarget = null;
            Created = false;
            CupidLoverPair = new();
        }
    }

    public static class HamburgerShop
    {
        public static List<PlayerControl> HamburgerShopPlayer;
        public static Color32 color = new(255, 128, 64, byte.MaxValue);
        public static void ClearAndReload()
        {
            HamburgerShopPlayer = new();
        }
    }

    public static class Penguin
    {
        public static List<PlayerControl> PenguinPlayer;
        public static Color32 color = ImpostorRed;
        public static Dictionary<PlayerControl, PlayerControl> PenguinData;
        public static PlayerControl currentTarget => PenguinData.ContainsKey(CachedPlayer.LocalPlayer) ? PenguinData[CachedPlayer.LocalPlayer] : null;
        private static Sprite _buttonSprite;
        public static Sprite GetButtonSprite() => _buttonSprite;
        public static void ClearAndReload()
        {
            PenguinPlayer = new();
            PenguinData = new();
            bool Is = ModHelpers.IsSucsessChance(4);
            _buttonSprite = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.PenguinButton_{(Is ? 1 : 2)}.png", Is ? 87.5f : 110f);
        }
    }
    public static class Dependents
    {
        public static List<PlayerControl> DependentsPlayer;
        public static Color32 color = ImpostorRed;
        public static void ClearAndReload()
        {
            DependentsPlayer = new();
        }
    }
    public static class LoversBreaker
    {
        public static List<PlayerControl> LoversBreakerPlayer;
        public static Color32 color = new(224, 82, 0, byte.MaxValue);
        public static int BreakCount;
        public static List<byte> CanEndGamePlayers;
        public static void ClearAndReload()
        {
            LoversBreakerPlayer = new();
            BreakCount = CustomOptionHolder.LoversBreakerBreakCount.GetInt();
            CanEndGamePlayers = new();
        }
    }
    public static class Jumbo
    {
        public static List<PlayerControl> JumboPlayer;
        public static List<PlayerControl> BigPlayer;
        public static Color32 color = ImpostorRed;
        public static Dictionary<byte, float> JumboSize;
        //イビルジャンボ
        public static bool Killed;
        public static bool CanKillSeted;

        public static Dictionary<byte, Vector2> OldPos;
        public static Dictionary<byte, float> PlaySound;
        public static void ClearAndReload()
        {
            JumboPlayer = new();
            BigPlayer = new();
            JumboSize = new();
            Killed = false;
            CanKillSeted = false;
            OldPos = new();
            PlaySound = new();
        }
    }
    //新ロールクラス
    public static class Quarreled
    {
        public static List<List<PlayerControl>> QuarreledPlayer;
        public static Color32 color = new(210, 105, 30, byte.MaxValue);
        public static bool IsQuarreledWin;
        public static bool IsQuarreledSuicide;
        public static void ClearAndReload()
        {
            QuarreledPlayer = new List<List<PlayerControl>>();
            IsQuarreledWin = false;
            IsQuarreledSuicide = false;
        }
    }
    public static class Lovers
    {
        public static List<List<PlayerControl>> LoversPlayer;
        public static Color32 color = new(255, 105, 180, byte.MaxValue);
        public static bool SameDie;
        public static bool AliveTaskCount;
        public static bool IsSingleTeam;
        public static List<List<PlayerControl>> FakeLoverPlayers;
        public static List<byte> FakeLovers;
        public static void ClearAndReload()
        {
            LoversPlayer = new();
            FakeLoverPlayers = new();
            FakeLovers = new();
            SameDie = CustomOptionHolder.LoversSameDie.GetBool();
            AliveTaskCount = CustomOptionHolder.LoversAliveTaskCount.GetBool();
            IsSingleTeam = CustomOptionHolder.LoversSingleTeam.GetBool();
        }
    }
}