using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using SuperNewRoles.Sabotage;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch]
    public static class RoleClass
    {
        public static bool IsMeeting;
        public static bool IsCoolTimeSetted;
        public static System.Random rnd = new((int)DateTime.Now.Ticks);
        public static Color ImpostorRed = Palette.ImpostorRed;
        public static Color CrewmateWhite = Color.white;
        public static Color FoxPurple = Palette.Purple;
        public static bool IsStart;
        public static List<byte> BlockPlayers;

        public static void ClearAndReloadRoles()
        {
            BlockPlayers = new();
            DeadPlayer.deadPlayers = new();
            AllRoleSetClass.Assigned = false;
            LateTask.Tasks = new();
            LateTask.AddTasks = new();
            BotManager.AllBots = new();
            IsMeeting = false;
            IsCoolTimeSetted = false;
            IsStart = false;
            Agartha.MapData.ClearAndReloads();
            LadderDead.Reset();
            //Map.Data.ClearAndReloads();
            ElectricPatch.Reset();
            SabotageManager.ClearAndReloads();
            Madmate.CheckedImpostor = new();
            Roles.MadMayor.CheckedImpostor = new();
            Roles.MadSeer.CheckedImpostor = new();
            Roles.JackalFriends.CheckedJackal = new();
            Mode.BattleRoyal.Main.VentData = new();
            EndGame.FinalStatusPatch.FinalStatusData.ClearFinalStatusData();
            Mode.ModeHandler.ClearAndReload();
            MapCustoms.AdditionalVents.ClearAndReload();
            MapCustoms.SpecimenVital.ClearAndReload();
            MapCustoms.MoveElecPad.ClearAndReload();
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
            Guesser.ClearAndReload();
            EvilGuesser.ClearAndReload();
            Vulture.ClearAndReload();
            NiceScientist.ClearAndReload();
            Clergyman.ClearAndReload();
            MadMate.ClearAndReload();
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
            Marine.ClearAndReload();
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
            //ロールクリア
            Quarreled.ClearAndReload();
            Lovers.ClearAndReload();
            MapOptions.MapOption.ClearAndReload();
            ChacheManager.Load();
        }
        public static void NotRole() { }
        public static class SoothSayer
        {
            public static List<PlayerControl> SoothSayerPlayer;
            public static List<byte> DisplayedPlayer;
            public static bool DisplayMode;
            public static int Count;
            public static Color32 color = new(190, 86, 235, byte.MaxValue);
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SoothSayerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                SoothSayerPlayer = new();
                DisplayedPlayer = new();
                DisplayMode = CustomOptions.SoothSayerDisplayMode.GetBool();
                Count = CustomOptions.SoothSayerMaxCount.GetInt();
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
                IsUseSabo = CustomOptions.JesterIsSabotage.GetBool();
                IsUseVent = CustomOptions.JesterIsVent.GetBool();
                IsJesterTaskClearWin = CustomOptions.JesterIsWinCleartask.GetBool();
            }
        }
        public static class Lighter
        {
            public static List<PlayerControl> LighterPlayer;
            public static Color32 color = new(255, 255, 0, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static bool IsLightOn;
            public static float UpVision;
            public static float DefaultCrewVision;
            public static DateTime ButtonTimer;

            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.LighterLightOnButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                LighterPlayer = new();
                CoolTime = CustomOptions.LighterCoolTime.GetFloat();
                DurationTime = CustomOptions.LighterDurationTime.GetFloat();
                UpVision = CustomOptions.LighterUpVision.GetFloat();
                DefaultCrewVision = PlayerControl.GameOptions.CrewLightMod;
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
                //CoolTime = CustomOptions.EvilLighterCoolTime.GetFloat();
                //DurationTime = CustomOptions.EvilLighterDurationTime.GetFloat();
            }
        }
        public static class EvilScientist
        {
            public static List<PlayerControl> EvilScientistPlayer;
            public static Color32 color = RoleClass.ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.EvilScientistButton.png.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                EvilScientistPlayer = new();
                CoolTime = CustomOptions.EvilScientistCoolTime.GetFloat();
                DurationTime = CustomOptions.EvilScientistDurationTime.GetFloat();
            }
        }
        public static class Sheriff
        {
            public static List<PlayerControl> SheriffPlayer;
            public static Color32 color = new(255, 255, 0, byte.MaxValue);
            public static PlayerControl currentTarget;
            public static float CoolTime;
            public static bool IsNeutralKill;
            public static bool IsLoversKill;
            public static bool IsMadRoleKill;
            public static bool IsFriendsRoleKill;
            public static float KillMaxCount;
            public static Dictionary<int, int> KillCount;
            public static DateTime ButtonTimer;

            private static Sprite buttonSprite;

            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }

            public static void ClearAndReload()
            {
                SheriffPlayer = new();
                CoolTime = CustomOptions.SheriffCoolTime.GetFloat();
                IsNeutralKill = CustomOptions.SheriffNeutralKill.GetBool();
                IsLoversKill = CustomOptions.SheriffLoversKill.GetBool();
                IsMadRoleKill = CustomOptions.SheriffMadRoleKill.GetBool();
                IsFriendsRoleKill = CustomOptions.SheriffFriendsRoleKill.GetBool();
                KillMaxCount = CustomOptions.SheriffKillMaxCount.GetFloat();
                KillCount = new();
            }
        }
        public static class MeetingSheriff
        {
            public static List<PlayerControl> MeetingSheriffPlayer;
            public static Color32 color = new(255, 255, 0, byte.MaxValue);
            public static bool NeutralKill;
            public static bool MadRoleKill;
            public static float KillMaxCount;
            public static bool OneMeetingMultiKill;

            private static Sprite buttonSprite;

            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                MeetingSheriffPlayer = new();
                NeutralKill = CustomOptions.MeetingSheriffNeutralKill.GetBool();
                MadRoleKill = CustomOptions.MeetingSheriffMadRoleKill.GetBool();
                KillMaxCount = CustomOptions.MeetingSheriffKillMaxCount.GetFloat();
                OneMeetingMultiKill = CustomOptions.MeetingSheriffOneMeetingMultiKill.GetBool();
            }
        }
        public static class Jackal
        {
            public static List<PlayerControl> JackalPlayer;
            public static List<PlayerControl> SidekickPlayer;
            public static List<PlayerControl> FakeSidekickPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static float KillCoolDown;
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsImpostorLight;
            public static bool CreateSidekick;
            public static bool NewJackalCreateSidekick;
            public static bool IsCreateSidekick;
            public static List<int> CreatePlayers;
            public static bool IsCreatedFriend;
            public static bool CanCreateFriend;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JackalSidekickButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                JackalPlayer = new();
                SidekickPlayer = new();
                FakeSidekickPlayer = new();
                KillCoolDown = CustomOptions.JackalKillCoolDown.GetFloat();
                IsUseVent = CustomOptions.JackalUseVent.GetBool();
                IsUseSabo = CustomOptions.JackalUseSabo.GetBool();
                IsImpostorLight = CustomOptions.JackalIsImpostorLight.GetBool();
                CreateSidekick = CustomOptions.JackalCreateSidekick.GetBool();
                IsCreateSidekick = CustomOptions.JackalCreateSidekick.GetBool();
                NewJackalCreateSidekick = CustomOptions.JackalNewJackalCreateSidekick.GetBool();
                IsCreatedFriend = false;
                CreatePlayers = new();
                CanCreateFriend = CustomOptions.JackalCreateFriend.GetBool();
            }
        }
        public static class Teleporter
        {
            public static List<PlayerControl> TeleporterPlayer;
            public static Color32 color = RoleClass.ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;
            public static DateTime ButtonTimer;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                TeleporterPlayer = new();
                CoolTime = CustomOptions.TeleporterCoolTime.GetFloat();
                DurationTime = CustomOptions.TeleporterDurationTime.GetFloat();
            }
        }
        public static class SpiritMedium
        {
            public static List<PlayerControl> SpiritMediumPlayer;
            public static Color32 color = new(0, 191, 255, byte.MaxValue);
            public static bool DisplayMode;
            public static float MaxCount;

            public static void ClearAndReload()
            {
                SpiritMediumPlayer = new();
                DisplayMode = CustomOptions.SpiritMediumDisplayMode.GetBool();
                MaxCount = CustomOptions.SpiritMediumMaxCount.GetFloat();
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
            public static Sprite GetSpeedBoostButtonSprite()
            {
                if (SpeedBoostButtonSprite) return SpeedBoostButtonSprite;
                SpeedBoostButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return SpeedBoostButtonSprite;
            }

            public static void ClearAndReload()
            {
                SpeedBoosterPlayer = new();
                CoolTime = CustomOptions.SpeedBoosterCoolTime.GetFloat();
                DurationTime = CustomOptions.SpeedBoosterDurationTime.GetFloat();
                Speed = CustomOptions.SpeedBoosterSpeed.GetFloat();
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
            public static float Speed { get { return CustomOptions.EvilSpeedBoosterSpeed.GetFloat(); } }
            public static bool IsSpeedBoost;
            public static DateTime ButtonTimer;
            public static Dictionary<int, bool> IsBoostPlayers;
            public static void ClearAndReload()
            {
                ButtonTimer = DateTime.Now;
                EvilSpeedBoosterPlayer = new();
                CoolTime = CustomOptions.EvilSpeedBoosterCoolTime.GetFloat();
                DurationTime = CustomOptions.EvilSpeedBoosterDurationTime.GetFloat();
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
                //IsKill = CustomOptions.TaskerIsKill.GetBool();
                //TaskCount = CustomOptions.TaskerAmount.GetFloat();
            }
        }
        public static class Doorr
        {
            public static List<PlayerControl> DoorrPlayer;
            public static Color32 color = new(205, 133, 63, byte.MaxValue);
            public static float CoolTime;
            public static DateTime ButtonTimer;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.DoorrDoorButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                ButtonTimer = DateTime.Now;
                DoorrPlayer = new();
                CoolTime = CustomOptions.DoorrCoolTime.GetFloat();
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
                CoolTime = CustomOptions.EvilDoorrCoolTime.GetFloat();
            }
        }
        public static class Shielder
        {
            public static List<PlayerControl> ShielderPlayer;
            public static Color32 color = new(100, 149, 237, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static Dictionary<byte, bool> IsShield;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ShielderButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                ShielderPlayer = new();
                CoolTime = CustomOptions.ShielderCoolTime.GetFloat();
                DurationTime = CustomOptions.ShielderDurationTime.GetFloat();
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
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FreezerButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                FreezerPlayer = new();
                CoolTime = CustomOptions.FreezerCoolTime.GetFloat();
                DurationTime = CustomOptions.FreezerDurationTime.GetFloat();
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
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedDownButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                SpeederPlayer = new();
                CoolTime = CustomOptions.SpeederCoolTime.GetFloat();
                DurationTime = CustomOptions.SpeederDurationTime.GetFloat();
                IsSpeedDown = false;
            }
        }
        public static class Guesser
        {
            public static List<PlayerControl> GuesserPlayer;
            public static Color32 color = new(255, 255, 0, byte.MaxValue);
            public static void ClearAndReload()
            {
                GuesserPlayer = new();
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
            public static Arrow Arrow;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.VultureButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                VulturePlayer = new();
                CoolTime = CustomOptions.VultureCoolDown.GetFloat();
                DeadBodyCount = CustomOptions.VultureDeadBodyMaxCount.GetInt();
                IsUseVent = CustomOptions.VultureIsUseVent.GetBool();
                ShowArrows = CustomOptions.VultureShowArrows.GetBool();
                Arrow = null;
            }
        }
        public static class NiceScientist
        {
            public static List<PlayerControl> NiceScientistPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static DateTime ButtonTimer;
            public static bool IsScientist;
            private static Sprite buttonSprite;
            public static Dictionary<int, bool> IsScientistPlayers;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.NiceScientistButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                NiceScientistPlayer = new();
                CoolTime = CustomOptions.NiceScientistCoolTime.GetFloat();
                DurationTime = CustomOptions.NiceScientistDurationTime.GetFloat();
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

            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ClergymanLightOutButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                ClergymanPlayer = new();
                CoolTime = CustomOptions.ClergymanCoolTime.GetFloat();
                DurationTime = CustomOptions.ClergymanDurationTime.GetFloat();
                IsLightOff = false;
                DownImpoVision = CustomOptions.ClergymanDownVision.GetFloat();
                DefaultImpoVision = PlayerControl.GameOptions.ImpostorLightMod;
                OldButtonTimer = DateTime.Now;
                OldButtonTime = 0;
            }
        }
        public static class MadMate
        {
            public static List<PlayerControl> MadMatePlayer;
            public static Color32 color = ImpostorRed;
            public static bool IsImpostorCheck;
            public static int ImpostorCheckTask;
            public static bool IsUseVent;
            public static bool IsImpostorLight;
            public static void ClearAndReload()
            {
                MadMatePlayer = new();
                IsImpostorCheck = CustomOptions.MadMateIsCheckImpostor.GetBool();
                IsUseVent = CustomOptions.MadMateIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadMateIsImpostorLight.GetBool();
                int Common = CustomOptions.MadMateCommonTask.GetInt();
                int Long = CustomOptions.MadMateLongTask.GetInt();
                int Short = CustomOptions.MadMateShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MadMateCheckImpostorTask.GetString().Replace("%", "")) / 100f));
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
                ReportTime = CustomOptions.BaitReportTime.GetFloat();
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
            private static Sprite nosetbuttonSprite;
            private static Sprite setbuttonSprite;
            public static Sprite GetNoSetButtonSprite()
            {
                if (setbuttonSprite) return setbuttonSprite;
                setbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MovingLocationSetButton.png", 115f);
                return setbuttonSprite;
            }
            public static Sprite GetSetButtonSprite()
            {
                if (nosetbuttonSprite) return nosetbuttonSprite;
                nosetbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MovingTpButton.png", 115f);
                return nosetbuttonSprite;
            }
            public static void ClearAndReload()
            {
                MovingPlayer = new();
                setpostion = new Vector3(0, 0, 0);
                CoolTime = CustomOptions.MovingCoolTime.GetFloat();
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
                //Num = CustomOptions.NiceGamblerUseCount.GetInt();
            }
        }
        public static class EvilGambler
        {
            public static List<PlayerControl> EvilGamblerPlayer;
            public static int SucCool;
            public static int NotSucCool;
            public static int SucPar;
            public static bool IsSuc;
            public static Color32 color = ImpostorRed;
            public static void ClearAndReload()
            {
                EvilGamblerPlayer = new();
                IsSuc = false;
                SucCool = CustomOptions.EvilGamblerSucTime.GetInt();
                NotSucCool = CustomOptions.EvilGamblerNotSucTime.GetInt();
                var temp = CustomOptions.EvilGamblerSucpar.GetString().Replace("0%", "");
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
                //SamplePosition = SamplePoss[PlayerControl.GameOptions.MapId];
                MySample = 0;
            }
        }
        public static class SelfBomber
        {
            public static List<PlayerControl> SelfBomberPlayer;
            public static Color32 color = ImpostorRed;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SelfBomberBomButton.png", 115f);
                return ButtonSprite;
            }
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
                IsVoteView = CustomOptions.GodViewVote.GetBool();
                IsTaskEndWin = CustomOptions.GodIsEndTaskWin.GetBool();
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
                IsChain = CustomOptions.NiceNekomataIsChain.GetBool();
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
                NotImpostorExiled = CustomOptions.EvilNekomataNotImpostorExiled.GetBool();
            }
        }
        public static class JackalFriends
        {
            public static List<PlayerControl> JackalFriendsPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsImpostorLight;
            public static bool IsJackalCheck;
            public static int JackalCheckTask;
            public static void ClearAndReload()
            {
                JackalFriendsPlayer = new();

                IsJackalCheck = CustomOptions.JackalFriendsIsCheckJackal.GetBool();
                IsUseVent = CustomOptions.JackalFriendsIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.JackalFriendsIsImpostorLight.GetBool();
                int Common = CustomOptions.JackalFriendsCommonTask.GetInt();
                int Long = CustomOptions.JackalFriendsLongTask.GetInt();
                int Short = CustomOptions.JackalFriendsShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptions.JackalFriendsCheckJackalTask.GetString().Replace("%", "")) / 100f));
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
                ChargeTime = CustomOptions.DoctorChargeTime.GetFloat();
                UseTime = CustomOptions.DoctorUseTime.GetFloat();
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
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CountChangerButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                CountChangerPlayer = new();
                ChangeData = new();
                Setdata = new();
                Count = CustomOptions.CountChangerMaxCount.GetInt();
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
                KillCoolTime = CustomOptions.MinimalistKillCoolTime.GetFloat();
                UseVent = CustomOptions.MinimalistVent.GetBool();
                UseSabo = CustomOptions.MinimalistSabo.GetBool();
                UseReport = CustomOptions.MinimalistReport.GetBool();
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
            private static Sprite buttonSprite;
            public static float Default;
            public static float CameraDefault;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.HawkHawkEye.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                HawkPlayer = new();
                CoolTime = CustomOptions.HawkCoolTime.GetFloat();
                DurationTime = CustomOptions.HawkDurationTime.GetFloat();
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
                ImpostorLight = CustomOptions.EgoistImpostorLight.GetBool();
                UseVent = CustomOptions.EgoistUseVent.GetBool();
                UseSabo = CustomOptions.EgoistUseSabo.GetBool();
                UseKill = CustomOptions.EgoistUseKill.GetBool();
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
                Count = CustomOptions.NiceRedRidingHoodCount.GetInt();
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
                Count = CustomOptions.EvilEraserMaxCount.GetInt() - 1;
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
                IsAliveWin = CustomOptions.WorkpersonIsAliveWin.GetBool();
            }
        }
        public static class Magaziner
        {
            public static List<PlayerControl> MagazinerPlayer;
            public static Color32 color = ImpostorRed;
            public static int MyPlayerCount;
            public static float SetTime;
            public static bool IsOKSet;
            private static Sprite GetbuttonSprite;
            private static Sprite AddbuttonSprite;
            public static Sprite GetGetButtonSprite()
            {
                if (GetbuttonSprite) return GetbuttonSprite;
                GetbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MagazinerGetButton.png", 115f);
                return GetbuttonSprite;
            }
            public static Sprite GetAddButtonSprite()
            {
                if (AddbuttonSprite) return AddbuttonSprite;
                AddbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MagazinerAddButton.png", 115f);
                return AddbuttonSprite;
            }
            public static void ClearAndReload()
            {
                MagazinerPlayer = new();
                MyPlayerCount = 0;
                SetTime = CustomOptions.MagazinerSetKillTime.GetFloat();
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
                AddVote = CustomOptions.MayorVoteCount.GetInt();
            }
        }
        public static class Truelover
        {
            public static List<PlayerControl> trueloverPlayer;
            public static Color32 color = Lovers.color;
            public static bool IsCreate;
            public static List<int> CreatePlayers;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.trueloverloveButton.png", 115f);
                return buttonSprite;
            }
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
                SuicideTime = CustomOptions.SerialKillerSuicideTime.GetFloat();
                KillTime = CustomOptions.SerialKillerKillTime.GetFloat();
                SuicideDefaultTime = SuicideTime;
                IsMeetingReset = CustomOptions.SerialKillerIsMeetingReset.GetBool();
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
                KillCoolTime = CustomOptions.OverKillerKillCoolTime.GetFloat();
                KillCount = CustomOptions.OverKillerKillCount.GetInt();
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
                    OneKillXP = CustomOptions.LevelingerOneKillXP.GetInt();
                    UpLevelXp = CustomOptions.LevelingerUpLevelXP.GetInt();
                    GetPowerData = new();
                    for (int i = 0; i < 5; i++)
                    {
                        string getdata = "";
                        if (i == 0) { getdata = CustomOptions.LevelingerLevelOneGetPower.GetString(); }
                        else if (i == 1) { getdata = CustomOptions.LevelingerLevelTwoGetPower.GetString(); }
                        else if (i == 2) { getdata = CustomOptions.LevelingerLevelThreeGetPower.GetString(); }
                        else if (i == 3) { getdata = CustomOptions.LevelingerLevelFourGetPower.GetString(); }
                        else if (i == 4) { getdata = CustomOptions.LevelingerLevelFiveGetPower.GetString(); }
                        GetPowerData.Add(GetLevelPowerType(getdata));
                    }
                    IsUseOKRevive = CustomOptions.LevelingerReviveXP.GetBool();
                    ReviveUseXP = CustomOptions.LevelingerUseXPRevive.GetInt();
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
                    return name == CustomOptions.LevelingerTexts[0]
                        ? LevelPowerTypes.None
                        : name == CustomOptions.LevelingerTexts[1]
                        ? LevelPowerTypes.Keep
                        : name == CustomOptions.LevelingerTexts[2]
                        ? LevelPowerTypes.Pursuer
                        : name == CustomOptions.LevelingerTexts[3]
                        ? LevelPowerTypes.Teleporter
                        : name == CustomOptions.LevelingerTexts[4]
                        ? LevelPowerTypes.Sidekick
                        : name == CustomOptions.LevelingerTexts[5]
                            ? LevelPowerTypes.SpeedBooster
                            : name == CustomOptions.LevelingerTexts[6] ? LevelPowerTypes.Moving : LevelPowerTypes.None;
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
                CoolTime = CustomOptions.EvilMovingCoolTime.GetFloat();
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
                KillCoolTime = CustomOptions.SideKillerKillCoolTime.GetFloat();
                MadKillerCoolTime = CustomOptions.SideKillerMadKillerKillCoolTime.GetFloat();
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
                KillCoolTime = CustomOptions.SurvivorKillCoolTime.GetFloat();
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
                AddVote = CustomOptions.MadMayorVoteCount.GetInt();
                IsImpostorCheck = CustomOptions.MadMayorIsCheckImpostor.GetBool();
                IsUseVent = CustomOptions.MadMayorIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadMayorIsImpostorLight.GetBool();
                int Common = CustomOptions.MadMayorCommonTask.GetInt();
                int Long = CustomOptions.MadMayorLongTask.GetInt();
                int Short = CustomOptions.MadMayorShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MadMayorCheckImpostorTask.GetString().Replace("%", "")) / 100f));
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
                CoolTime = CustomOptions.NiceHawkCoolTime.GetFloat();
                DurationTime = CustomOptions.NiceHawkDurationTime.GetFloat();
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
                IsUseVent = CustomOptions.MadStuntManIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadStuntManIsImpostorLight.GetBool();
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
                IsUseVent = CustomOptions.MadHawkIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadHawkIsImpostorLight.GetBool();
                MadHawkPlayer = new();
                CoolTime = CustomOptions.MadHawkCoolTime.GetFloat();
                DurationTime = CustomOptions.MadHawkDurationTime.GetFloat();
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
                IsUseVent = CustomOptions.MadJesterIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadJesterIsImpostorLight.GetBool();
                IsMadJesterTaskClearWin = CustomOptions.IsMadJesterTaskClearWin.GetBool();
                int Common = CustomOptions.MadJesterCommonTask.GetInt();
                int Long = CustomOptions.MadJesterLongTask.GetInt();
                int Short = CustomOptions.MadJesterShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
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
                DefaultTurn = CustomOptions.FalseChargesExileTurn.GetInt();
                CoolTime = CustomOptions.FalseChargesCoolTime.GetFloat();
            }
        }
        public static class NiceTeleporter
        {
            public static List<PlayerControl> NiceTeleporterPlayer;
            public static Color32 color = new(0, 0, 128, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static DateTime ButtonTimer;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                NiceTeleporterPlayer = new();
                CoolTime = CustomOptions.NiceTeleporterCoolTime.GetFloat();
                DurationTime = CustomOptions.NiceTeleporterDurationTime.GetFloat();
            }
        }
        public static class Celebrity
        {
            public static List<PlayerControl> CelebrityPlayer;
            public static Color32 color = Color.yellow;
            public static bool ChangeRoleView;
            public static List<PlayerControl> ViewPlayers;
            public static void ClearAndReload()
            {
                CelebrityPlayer = new();
                ChangeRoleView = CustomOptions.CelebrityChangeRoleView.GetBool();
                ViewPlayers = new();
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
            public static void ClearAndReload()
            {
                VampirePlayer = new();
                target = null;
                KillDelay = CustomOptions.VampireKillDelay.GetFloat();
                Timer = 0;
                KillTimer = DateTime.Now;
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
                IsUseVent = CustomOptions.FoxIsUseVent.GetBool();
                UseReport = CustomOptions.FoxReport.GetBool();
                IsImpostorLight = CustomOptions.FoxIsImpostorLight.GetBool();
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
                KillCoolTime = CustomOptions.DarkKillerKillCoolTime.GetFloat();
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
                limitSoulDuration = CustomOptions.SeerLimitSoulDuration.GetBool();
                soulDuration = CustomOptions.SeerSoulDuration.GetFloat();
                mode = CustomOptions.SeerMode.GetSelection();
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
                limitSoulDuration = CustomOptions.MadSeerLimitSoulDuration.GetBool();
                soulDuration = CustomOptions.MadSeerSoulDuration.GetFloat();
                mode = CustomOptions.MadSeerMode.GetSelection();

                IsImpostorCheck = CustomOptions.MadSeerIsCheckImpostor.GetBool();
                IsUseVent = CustomOptions.MadSeerIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadSeerIsImpostorLight.GetBool();
                int Common = CustomOptions.MadSeerCommonTask.GetInt();
                int Long = CustomOptions.MadSeerLongTask.GetInt();
                int Short = CustomOptions.MadSeerShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MadSeerCheckImpostorTask.GetString().Replace("%", "")) / 100f));
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
            public static void ClearAndReload()
            {
                EvilSeerPlayer = new();
                deadBodyPositions = new();
                limitSoulDuration = CustomOptions.EvilSeerLimitSoulDuration.GetBool();
                soulDuration = CustomOptions.EvilSeerSoulDuration.GetFloat();
                mode = CustomOptions.EvilSeerMode.GetSelection();
            }
        }
        public static class RemoteSheriff
        {
            public static List<PlayerControl> RemoteSheriffPlayer;
            public static Color32 color = new(255, 255, 0, byte.MaxValue);
            public static float CoolTime;
            public static bool IsNeutralKill;
            public static bool IsLoversKill;
            public static bool IsMadRoleKill;
            public static bool MadRoleKill;
            public static float KillMaxCount;
            public static Dictionary<int, int> KillCount;
            public static bool IsKillTeleport;
            public static float KillCoolTime;
            public static void ClearAndReload()
            {
                RemoteSheriffPlayer = new();
                CoolTime = CustomOptions.RemoteSheriffCoolTime.GetFloat();
                IsNeutralKill = CustomOptions.RemoteSheriffNeutralKill.GetBool();
                IsLoversKill = CustomOptions.RemoteSheriffLoversKill.GetBool();
                IsMadRoleKill = CustomOptions.RemoteSheriffMadRoleKill.GetBool();
                MadRoleKill = CustomOptions.RemoteSheriffMadRoleKill.GetBool();
                KillMaxCount = CustomOptions.RemoteSheriffKillMaxCount.GetFloat();
                KillCount = new();
                IsKillTeleport = CustomOptions.RemoteSheriffIsKillTeleportSetting.GetBool();
                KillCoolTime = CustomOptions.RemoteSheriffCoolTime.GetFloat();
            }
        }
        public static class TeleportingJackal
        {
            public static List<PlayerControl> TeleportingJackalPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static float KillCoolDown;
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsImpostorLight;
            public static float CoolTime;
            public static float DurationTime;
            public static DateTime ButtonTimer;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                TeleportingJackalPlayer = new();
                KillCoolDown = CustomOptions.TeleportingJackalKillCoolDown.GetFloat();
                IsUseVent = CustomOptions.TeleportingJackalUseVent.GetBool();
                IsUseSabo = CustomOptions.TeleportingJackalUseSabo.GetBool();
                IsImpostorLight = CustomOptions.TeleportingJackalIsImpostorLight.GetBool();
                CoolTime = CustomOptions.TeleportingJackalCoolTime.GetFloat();
                DurationTime = CustomOptions.TeleportingJackalDurationTime.GetFloat();
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
                IsUseVent = CustomOptions.MadMakerIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadMakerIsImpostorLight.GetBool();
                IsCreateMadmate = false;
                CreatePlayers = new();
            }
        }
        public static class Demon
        {
            public static List<PlayerControl> DemonPlayer;
            public static Dictionary<byte, List<PlayerControl>> CurseDatas;
            public static Color32 color = new(110, 0, 165, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsCheckImpostor;
            public static bool IsAliveWin;
            public static float CoolTime;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.DemonButton.png", 115f);
                return buttonSprite;
            }

            public static void ClearAndReload()
            {
                DemonPlayer = new();
                CurseDatas = new Dictionary<byte, List<PlayerControl>>();
                IsUseVent = CustomOptions.DemonIsUseVent.GetBool();
                CoolTime = CustomOptions.DemonCoolTime.GetFloat();
                IsCheckImpostor = CustomOptions.DemonIsCheckImpostor.GetBool();
                IsAliveWin = CustomOptions.DemonIsAliveWin.GetBool();
            }
        }
        public static class TaskManager
        {
            public static List<PlayerControl> TaskManagerPlayer;
            public static Color32 color = new(153, 255, 255, byte.MaxValue);
            public static void ClearAndReload()
            {
                TaskManagerPlayer = new();
                int Common = CustomOptions.TaskManagerCommonTask.GetInt();
                int Long = CustomOptions.TaskManagerLongTask.GetInt();
                int Short = CustomOptions.TaskManagerShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
            }
        }
        public static class SeerFriends
        {
            public static List<PlayerControl> SeerFriendsPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);

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
                limitSoulDuration = CustomOptions.SeerFriendsLimitSoulDuration.GetBool();
                soulDuration = CustomOptions.SeerFriendsSoulDuration.GetFloat();
                mode = CustomOptions.SeerFriendsMode.GetSelection();

                IsJackalCheck = CustomOptions.SeerFriendsIsCheckJackal.GetBool();
                IsUseVent = CustomOptions.SeerFriendsIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.SeerFriendsIsImpostorLight.GetBool();
                int Common = CustomOptions.SeerFriendsCommonTask.GetInt();
                int Long = CustomOptions.SeerFriendsLongTask.GetInt();
                int Short = CustomOptions.SeerFriendsShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptions.SeerFriendsCheckJackalTask.GetString().Replace("%", "")) / 100f));
            }
        }
        public static class JackalSeer
        {
            public static List<PlayerControl> JackalSeerPlayer;
            public static List<PlayerControl> SidekickSeerPlayer;
            public static List<PlayerControl> FakeSidekickSeerPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);

            public static List<Vector3> deadBodyPositions;
            public static float soulDuration;
            public static bool limitSoulDuration;
            public static int mode;

            public static float KillCoolDown;
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsImpostorLight;
            public static bool CreateSidekick;
            public static bool NewJackalCreateSidekick;
            public static bool IsCreateSidekick;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JackalSeerSidekickButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                JackalSeerPlayer = new();
                SidekickSeerPlayer = new();
                FakeSidekickSeerPlayer = new();

                deadBodyPositions = new();
                limitSoulDuration = CustomOptions.JackalSeerLimitSoulDuration.GetBool();
                soulDuration = CustomOptions.JackalSeerSoulDuration.GetFloat();
                mode = CustomOptions.JackalSeerMode.GetSelection();

                KillCoolDown = CustomOptions.JackalSeerKillCoolDown.GetFloat();
                IsUseVent = CustomOptions.JackalSeerUseVent.GetBool();
                IsUseSabo = CustomOptions.JackalSeerUseSabo.GetBool();
                IsImpostorLight = CustomOptions.JackalSeerIsImpostorLight.GetBool();
                CreateSidekick = CustomOptions.JackalSeerCreateSidekick.GetBool();
                IsCreateSidekick = CustomOptions.JackalSeerCreateSidekick.GetBool();
                NewJackalCreateSidekick = CustomOptions.JackalSeerNewJackalCreateSidekick.GetBool();
            }
        }
        public static class Assassin
        {
            public static List<PlayerControl> AssassinPlayer;
            public static Color32 color = ImpostorRed;
            public static List<byte> MeetingEndPlayers;
            public static PlayerControl TriggerPlayer;
            public static PlayerControl DeadPlayer;
            public static bool IsImpostorWin;
            public static void ClearAndReload()
            {
                AssassinPlayer = new();
                MeetingEndPlayers = new();
                TriggerPlayer = null;
                DeadPlayer = null;
                IsImpostorWin = false;
            }
        }
        public static class Marine
        {
            public static List<PlayerControl> MarinePlayer;
            public static Color32 color = new(175, 223, 228, byte.MaxValue);
            public static void ClearAndReload()
            {
                MarinePlayer = new();
            }
        }
        public static class Arsonist
        {
            public static List<PlayerControl> ArsonistPlayer;
            public static Dictionary<byte, List<PlayerControl>> DouseDatas;
            public static Color32 color = new(238, 112, 46, byte.MaxValue);
            public static bool IsUseVent;
            public static float CoolTime;
            public static float DurationTime;
            public static bool TriggerArsonistWin;
            public static bool IsDouse;
            public static PlayerControl DouseTarget;
            private static Sprite DousebuttonSprite;
            private static Sprite IgnitebuttonSprite;
            public static Sprite GetDouseButtonSprite()
            {
                if (DousebuttonSprite) return DousebuttonSprite;
                DousebuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ArsonistDouse.png", 115f);
                return DousebuttonSprite;
            }
            public static Sprite GetIgniteButtonSprite()
            {
                if (IgnitebuttonSprite) return IgnitebuttonSprite;
                IgnitebuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ArsonistIgnite.png", 115f);
                return IgnitebuttonSprite;
            }

            public static void ClearAndReload()
            {
                ArsonistPlayer = new();
                DouseDatas = new Dictionary<byte, List<PlayerControl>>();
                IsUseVent = CustomOptions.ArsonistIsUseVent.GetBool();
                CoolTime = CustomOptions.ArsonistCoolTime.GetFloat();
                DurationTime = CustomOptions.ArsonistDurationTime.GetFloat();
                TriggerArsonistWin = true;
                IsDouse = false;
                DouseTarget = null;
            }
        }
        public static class Chief
        {
            public static List<PlayerControl> ChiefPlayer;
            public static List<byte> SheriffPlayer;
            public static Color32 color = new(255, 255, 0, byte.MaxValue);
            public static bool IsCreateSheriff;
            public static float CoolTime;
            public static bool IsNeutralKill;
            public static bool IsLoversKill;
            public static bool IsMadRoleKill;
            public static bool MadRoleKill;
            public static int KillLimit;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ChiefSidekickButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                ChiefPlayer = new();
                SheriffPlayer = new();
                IsCreateSheriff = false;
                CoolTime = CustomOptions.ChiefSheriffCoolTime.GetFloat();
                IsNeutralKill = CustomOptions.ChiefIsNeutralKill.GetBool();
                IsLoversKill = CustomOptions.ChiefIsLoversKill.GetBool();
                IsMadRoleKill = CustomOptions.ChiefIsMadRoleKill.GetBool();
                KillLimit = CustomOptions.ChiefKillLimit.GetInt();
            }
        }
        public static class Cleaner
        {
            public static List<PlayerControl> CleanerPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static int CleanMaxCount;
            public static float KillCoolTime;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CleanerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                CleanerPlayer = new();
                CoolTime = CustomOptions.CleanerCoolDown.GetFloat();
                KillCoolTime = CustomOptions.CleanerKillCoolTime.GetFloat();
            }
        }
        public static class MadCleaner
        {
            public static List<PlayerControl> MadCleanerPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static bool IsUseVent;
            public static bool IsImpostorLight;

            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CleanerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                MadCleanerPlayer = new();
                CoolTime = CustomOptions.MadCleanerCoolDown.GetFloat();
                IsUseVent = CustomOptions.MadCleanerIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MadCleanerIsImpostorLight.GetBool();
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
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SamuraiButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                SamuraiPlayer = new();
                KillCoolTime = CustomOptions.SamuraiKillCoolTime.GetFloat();
                SwordCoolTime = CustomOptions.SamuraiSwordCoolTime.GetFloat();
                UseVent = CustomOptions.SamuraiVent.GetBool();
                UseSabo = CustomOptions.SamuraiSabo.GetBool();
                Sword = false;
                SwordedPlayer = new();
            }
        }
        public static class MayorFriends
        {
            public static List<PlayerControl> MayorFriendsPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsImpostorLight;
            public static bool IsJackalCheck;
            public static int JackalCheckTask;
            public static int AddVote;
            public static void ClearAndReload()
            {
                MayorFriendsPlayer = new();
                IsJackalCheck = CustomOptions.MayorFriendsIsCheckJackal.GetBool();
                IsUseVent = CustomOptions.MayorFriendsIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.MayorFriendsIsImpostorLight.GetBool();
                int Common = CustomOptions.MayorFriendsCommonTask.GetInt();
                int Long = CustomOptions.MayorFriendsLongTask.GetInt();
                int Short = CustomOptions.MayorFriendsShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MayorFriendsCheckJackalTask.GetString().Replace("%", "")) / 100f));
                AddVote = CustomOptions.MayorFriendsVoteCount.GetInt();
            }
        }
        public static class VentMaker
        {
            public static List<PlayerControl> VentMakerPlayer;
            public static Color32 color = ImpostorRed;
            public static Vent Vent;
            public static int VentCount;
            public static bool IsMakeVent;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.VentMakerButton.png", 115f);
                return buttonSprite;
            }
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
            public static Color32 color = Color.blue;
            public static int LimitCount;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.GhostMechanicRepairButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                GhostMechanicPlayer = new();
                LimitCount = CustomOptions.GhostMechanicRepairLimit.GetInt();
            }
        }
        public static class EvilHacker
        {
            public static List<PlayerControl> EvilHackerPlayer;
            public static Color32 color = ImpostorRed;
            public static bool IsCreateMadmate;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                byte mapId = PlayerControl.GameOptions.MapId;
                UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
                if (mapId is 0 or 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
                else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
                else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
                buttonSprite = button.Image;
                return buttonSprite; //GMHからの引用
            }
            public static void ClearAndReload()
            {
                EvilHackerPlayer = new();
                IsCreateMadmate = CustomOptions.EvilHackerMadmateSetting.GetBool();
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
            public static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PositionSwapperButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                PositionSwapperPlayer = new();
                CoolTime = CustomOptions.PositionSwapperCoolTime.GetFloat();
                SwapCount = CustomOptions.PositionSwapperSwapCount.GetInt();
            }
        }

        public static class Tuna
        {
            public static List<PlayerControl> TunaPlayer;
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static Dictionary<byte, Vector3> Position;
            public static float Timer;
            public static float StoppingTime;
            public static bool IsUseVent;
            public static Dictionary<byte, float> Timers;
            public static bool IsMeetingEnd;
            public static bool IsTunaAddWin;
            public static void ClearAndReload()
            {
                TunaPlayer = new();
                Position = new Dictionary<byte, Vector3>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers) Position[p.PlayerId] = new Vector3(9999f, 9999f, 9999f);
                StoppingTime = CustomOption.CustomOptions.TunaStoppingTime.GetFloat();
                if (Mode.ModeHandler.IsMode(Mode.ModeId.Default)) Timer = StoppingTime;
                IsUseVent = CustomOptions.TunaIsUseVent.GetBool();
                IsTunaAddWin = CustomOptions.TunaIsAddWin.GetBool();
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
                NotImpostorExiled = CustomOptions.BlackCatNotImpostorExiled.GetBool();
                IsImpostorCheck = CustomOptions.BlackCatIsCheckImpostor.GetBool();
                IsUseVent = CustomOptions.BlackCatIsUseVent.GetBool();
                IsImpostorLight = CustomOptions.BlackCatIsImpostorLight.GetBool();
                int Common = CustomOptions.BlackCatCommonTask.GetInt();
                int Long = CustomOptions.BlackCatLongTask.GetInt();
                int Short = CustomOptions.BlackCatShortTask.GetInt();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.BlackCatCheckImpostorTask.GetString().Replace("%", "")) / 100f));
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
                KillCoolTime = CustomOptions.SecretlyKillerKillCoolTime.GetFloat();
                IsKillCoolChange = CustomOptions.SecretlyKillerIsKillCoolTimeChange.GetBool();
                IsBlackOutKillCharge = CustomOptions.SecretlyKillerIsBlackOutKillCharge.GetBool();
                SecretlyKillLimit = CustomOptions.SecretlyKillerSecretKillLimit.GetInt();
                SecretlyKillCoolTime = CustomOptions.SecretlyKillerSecretKillCoolTime.GetFloat();
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
                CanUseVent = CustomOptions.SpyCanUseVent.GetBool();
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
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.KunoichiKunaiButton.png", 115f);
                return buttonSprite;
            }
            private static Sprite HidebuttonSprite;
            public static Sprite GetHideButtonSprite()
            {
                if (HidebuttonSprite) return HidebuttonSprite;
                HidebuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.KunoichiHideButton.png", 115f);
                return HidebuttonSprite;
            }
            public static void ClearAndReload()
            {
                HideKunai = CustomOptions.KunoichiHideKunai.GetBool();
                OldPosition = new();
                StopTime = 0;
                HideTime = CustomOptions.KunoichiIsHide.GetBool() ? CustomOptions.KunoichiHideTime.GetFloat() : -1;
                IsWaitAndPressTheButtonToHide = CustomOptions.KunoichiIsWaitAndPressTheButtonToHide.GetBool();
                IsHideButton = false;
                KunoichiPlayer = new();
                KillCoolTime = CustomOptions.KunoichiCoolTime.GetFloat();
                KillKunai = CustomOptions.KunoichiKillKunai.GetInt();
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
            public static float MainCoolTime;
            public static float SubCoolTime;
            public static bool CanUseSabo;
            public static bool CanUseVent;
            public static void ClearAndReload()
            {
                DoubleKillerPlayer = new();
                MainCoolTime = CustomOptions.MainKillCoolTime.GetFloat();
                SubCoolTime = CustomOptions.SubKillCoolTime.GetFloat();
                CanUseSabo = CustomOptions.DoubleKillerSabo.GetBool();
                CanUseVent = CustomOptions.DoubleKillerVent.GetBool();
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
                KillCoolTime = CustomOptions.SmasherKillCoolTime.GetFloat();
                SmashOn = false;
            }
        }
        public static class SuicideWisher
        {
            public static List<PlayerControl> SuicideWisherPlayer;
            public static Color32 color = ImpostorRed;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SuicideWisherButton.png", 115f);
                return buttonSprite;
            }
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
                IsAddWin = CustomOptions.NeetIsAddWin.GetBool();
            }
        }
        public static class FastMaker
        {
            public static List<PlayerControl> FastMakerPlayer;
            public static Color32 color = ImpostorRed;
            public static bool IsCreatedMadMate;
            public static List<int> CreatePlayers;
            public static void ClearAndReload()
            {
                FastMakerPlayer = new();
                IsCreatedMadMate = false;
                CreatePlayers = new();
            }
        }
        public static class ToiletFan
        {
            public static List<PlayerControl> ToiletFanPlayer;
            public static Color32 color = new(116, 80, 48, byte.MaxValue);
            public static float ToiletCool;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ToiletFanButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                ToiletFanPlayer = new();
                ToiletCool = CustomOptions.ToiletFanCoolTime.GetFloat();
            }
        }
        public static class EvilButtoner
        {
            public static List<PlayerControl> EvilButtonerPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static float SkillCount;
            public static Dictionary<int, int> SkillCountSHR;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ButtonerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                EvilButtonerPlayer = new();
                CoolTime = CustomOptions.EvilButtonerCoolTime.GetFloat();
                SkillCount = CustomOptions.EvilButtonerCount.GetFloat();
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
                CoolTime = CustomOptions.NiceButtonerCoolTime.GetFloat();
                SkillCount = CustomOptions.NiceButtonerCount.GetFloat();
                SkillCountSHR = new();
            }
        }
        public static class Finder
        {
            public static List<PlayerControl> FinderPlayer;
            public static Color32 color = ImpostorRed;
            public static int CheckMadmateKillCount;
            public static int KillCount;
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
                CheckMadmateKillCount = CustomOptions.FinderCheckMadmateSetting.GetInt();
                KillCount = 0;
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
                CoolTime = CustomOptions.RevolutionistCoolTime.GetFloat();
                TouchTime = CustomOptions.RevolutionistTouchTime.GetFloat();
                RevolutionedPlayerId = new();
                _revolutionedPlayer = new PlayerControl[] { };
                IsAddWin = CustomOptions.RevolutionistAddWin.GetBool();
                IsAddWinAlive = CustomOptions.RevolutionistAddWinIsAlive.GetBool();
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
                VoteCount = CustomOptions.DictatorVoteCount.GetInt();
                SubExileLimit = CustomOptions.DictatorSubstituteExile.GetBool() ? CustomOptions.DictatorSubstituteExileLimit.GetInt() : 0;
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
                VentDeathChance = CustomOptions.SpelunkerVentDeathChance.GetSelection();
                LadderDeathChance = CustomOptions.SpelunkerLadderDeadChance.GetSelection();
                CommsOrLightdownDeathTime = CustomOptions.SpelunkerIsDeathCommsOrPowerdown.GetBool() ? CustomOptions.SpelunkerDeathCommsOrPowerdownTime.GetFloat() : -1f;
                CommsOrLightdownTime = 0f;
                LiftDeathChance = CustomOptions.SpelunkerLiftDeathChance.GetSelection();
                Neutral.Spelunker.DeathPosition = null;
                DoorOpenChance = CustomOptions.SpelunkerDoorOpenChance.GetSelection();
            }
        }

        public static class SuicidalIdeation
        {
            public static List<PlayerControl> SuicidalIdeationPlayer;
            public static Color32 color = new(71, 71, 71, byte.MaxValue);
            public static bool SuicidalIdeationWinText;
            public static float TimeLeft;
            public static DateTime ButtonTimer;
            public static int CompletedTask;
            public static float AddTimeLeft;
            public static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SuicidalIdeationButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                SuicidalIdeationPlayer = new();
                SuicidalIdeationWinText = CustomOptions.SuicidalIdeationWinText.GetBool();
                TimeLeft = CustomOptions.SuicidalIdeationTimeLeft.GetFloat();
                AddTimeLeft = CustomOptions.SuicidalIdeationAddTimeLeft.GetFloat();
                ButtonTimer = DateTime.Now;
                CompletedTask = 0;
            }
        }
        public static class Hitman
        {
            public static List<PlayerControl> HitmanPlayer;
            public static Color32 color = new(86, 41, 18, byte.MaxValue);
            public static float KillCoolTime;
            public static int OutMissionLimit;
            public static PlayerControl Target;
            public static float ChangeTargetTime;
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
                KillCoolTime = CustomOptions.HitmanKillCoolTime.GetFloat();
                if (CustomOptions.HitmanIsOutMission.GetBool())
                {
                    OutMissionLimit = CustomOptions.HitmanOutMissionLimit.GetInt();
                }
                else
                {
                    OutMissionLimit = -1;
                }
                ChangeTargetTime = CustomOptions.HitmanChangeTargetTime.GetFloat();
                UpdateTime = ChangeTargetTime;
                cooldownText = null;
                WinKillCount = CustomOptions.HitmanWinKillCount.GetInt();
                if (TargetArrow != null && TargetArrow.arrow != null)
                {
                    UnityEngine.Object.Destroy(TargetArrow.arrow);
                }
                TargetArrow = null;
                if (CustomOptions.HitmanIsArrowView.GetBool())
                {
                    ArrowUpdateTimeDefault = CustomOptions.HitmanArrowUpdateTime.GetFloat();
                }
                else
                {
                    ArrowUpdateTimeDefault = -1f;
                }
                ArrowUpdateTime = ArrowUpdateTimeDefault;
            }
        }
        public static class Matryoshka
        {
            public static List<PlayerControl> MatryoshkaPlayer;
            public static Color32 color = ImpostorRed;
            public static int WearLimit;
            public static bool WearReport;
            public static float WearDefaultTime;
            public static float WearTime;
            public static float AddKillCoolTime;
            public static float MyKillCoolTime;
            public static float CoolTime;
            public static bool IsLocalOn => !Datas.Keys.All(data => data != CachedPlayer.LocalPlayer.PlayerId || Datas[data].Item1 == null);
            public static Dictionary<byte, (DeadBody, float)> Datas;
            public static Sprite PutOnButtonSprite
            {
                get
                {
                    if (_PutOnButtonSprite == null)
                    {
                        _PutOnButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MatryoshkaPutOnButton.png", 115f);
                    }
                    return _PutOnButtonSprite;
                }
            }
            public static Sprite _PutOnButtonSprite;
            public static Sprite TakeOffButtonSprite
            {
                get
                {
                    if (_TakeOffButtonSprite == null)
                    {
                        _TakeOffButtonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MatryoshkaTakeOffButton.png", 115f);
                    }
                    return _TakeOffButtonSprite;
                }
            }
            public static Sprite _TakeOffButtonSprite;
            public static void ClearAndReload()
            {
                MatryoshkaPlayer = new();
                WearLimit = CustomOptions.MatryoshkaWearLimit.GetInt();
                WearReport = CustomOptions.MatryoshkaWearReport.GetBool();
                WearDefaultTime = CustomOptions.MatryoshkaWearTime.GetFloat();
                AddKillCoolTime = CustomOptions.MatryoshkaAddKillCoolTime.GetFloat();
                WearTime = 0;
                Datas = new();
                CoolTime = CustomOptions.MatryoshkaCoolTime.GetFloat();
                MyKillCoolTime = PlayerControl.GameOptions.killCooldown;
            }
        }
        public static class Nun
        {
            public static List<PlayerControl> NunPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.NunButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                NunPlayer = new();
                CoolTime = CustomOptions.NunCoolTime.GetFloat();
            }
        }
        public static class PartTimer
        {
            public static List<PlayerControl> PartTimerPlayer;
            public static Color32 color = new(0, 255, 0, byte.MaxValue);
            public static int DeathDefaultTurn;
            public static int DeathTurn;
            public static float CoolTime;
            public static bool IsCheckTargetRole;
            public static Dictionary<byte, byte> Datas;
            public static bool IsLocalOn
            {
                get
                {
                    return Datas.ContainsKey(CachedPlayer.LocalPlayer.PlayerId);
                }
            }
            public static PlayerControl CurrentTarget
            {
                get
                {
                    return IsLocalOn ? ModHelpers.PlayerById(Datas[CachedPlayer.LocalPlayer.PlayerId]) : null;
                }
            }
            public static Dictionary<PlayerControl, PlayerControl> PlayerDatas
            {
                get
                {
                    if (_playerDatas.Count != Datas.Count)
                    {
                        Dictionary<PlayerControl, PlayerControl> newdic = new();
                        foreach (var data in Datas)
                        {
                            newdic.Add(ModHelpers.PlayerById(data.Key), ModHelpers.PlayerById(data.Value));
                        }
                        _playerDatas = newdic;
                    }
                    return _playerDatas;
                }
            }
            private static Dictionary<PlayerControl, PlayerControl> _playerDatas;
            public static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PartTimerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                PartTimerPlayer = new();
                DeathTurn = DeathDefaultTurn = CustomOptions.PartTimerDeathTurn.GetInt();
                CoolTime = CustomOptions.PartTimerCoolTime.GetFloat();
                IsCheckTargetRole = CustomOptions.PartTimerIsCheckTargetRole.GetBool();
                Datas = new();
                _playerDatas = new();
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
            public static Dictionary<CrewMate.Painter.ActionType, List<Vector2>> ActionDatas;
            public static List<Footprint> Prints;
            public static Dictionary<CrewMate.Painter.ActionType, bool> IsEnables;
            public static bool IsLocalActionSend;
            public static bool IsDeathFootpointBig;
            public static bool IsFootprintMeetingDestroy;
            public static float CoolTime;
            public static PlayerControl CurrentTarget;
            public static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PainterButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                PainterPlayer = new();
                CoolTime = CustomOptions.PainterCoolTime.GetFloat();
                ActionDatas = new();
                IsEnables = new();
                foreach (CrewMate.Painter.ActionType type in Enum.GetValues(typeof(CrewMate.Painter.ActionType)))
                {
                    ActionDatas[type] = new();
                }
                Prints = new();
                CurrentTarget = null;
                IsLocalActionSend = false;
                IsEnables[CrewMate.Painter.ActionType.TaskComplete] = CustomOptions.PainterIsTaskCompleteFootprint.GetBool();
                IsEnables[CrewMate.Painter.ActionType.SabotageRepair] = CustomOptions.PainterIsSabotageRepairFootprint.GetBool();
                IsEnables[CrewMate.Painter.ActionType.InVent] = CustomOptions.PainterIsInVentFootprint.GetBool();
                IsEnables[CrewMate.Painter.ActionType.ExitVent] = CustomOptions.PainterIsExitVentFootprint.GetBool();
                IsEnables[CrewMate.Painter.ActionType.CheckVital] = CustomOptions.PainterIsCheckVitalFootprint.GetBool();
                IsEnables[CrewMate.Painter.ActionType.CheckAdmin] = CustomOptions.PainterIsCheckAdminFootprint.GetBool();
                IsEnables[CrewMate.Painter.ActionType.Death] = CustomOptions.PainterIsDeathFootprint.GetBool();
                IsDeathFootpointBig = CustomOptions.PainterIsDeathFootprintBig.GetBool();
                IsFootprintMeetingDestroy = CustomOptions.PainterIsFootprintMeetingDestroy.GetBool();
            }
        }
        public static class Psychometrist
        {
            public static List<PlayerControl> PsychometristPlayer;
            public static Color32 color = new(238, 130, 238, byte.MaxValue);
            public static float CoolTime;
            public static float ReadTime;
            public static bool IsCheckDeathTime;
            public static int DeathTimeDeviation;
            public static bool IsCheckDeathReason;
            public static bool IsCheckFootprints;
            public static float CanCheckFootprintsTime;
            public static bool IsReportCheckedReportDeadbody;
            //(source, target) : Vector2
            public static Dictionary<(byte, byte), (List<Vector2>, bool)> FootprintsPosition;
            public static Dictionary<(byte, byte), float> FootprintsDeathTime;
            public static Dictionary<(byte, byte), List<Footprint>> FootprintObjects;
            public static float UpdateTime;
            public static float Distance = 0.5f;
            //(死体, テキスト, 誤差)
            public static List<(DeadBody, TextMeshPro, int)> DeathTimeTexts;
            public static DeadBody CurrentTarget;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PsychometristButton.png", 115f);
                return buttonSprite;
            }

            public static void ClearAndReload()
            {
                PsychometristPlayer = new();
                UpdateTime = 0.1f;
                CurrentTarget = null;
                DeathTimeTexts = new();
                FootprintsPosition = new();
                FootprintObjects = new();
                FootprintsDeathTime = new();
                CoolTime = CustomOptions.PsychometristCoolTime.GetFloat();
                ReadTime = CustomOptions.PsychometristReadTime.GetFloat();
                IsCheckDeathTime = CustomOptions.PsychometristIsCheckDeathReason.GetBool();
                DeathTimeDeviation = CustomOptions.PsychometristDeathTimeDeviation.GetInt();
                IsCheckDeathReason = CustomOptions.PsychometristIsCheckDeathReason.GetBool();
                IsCheckFootprints = CustomOptions.PsychometristIsCheckFootprints.GetBool();
                CanCheckFootprintsTime = CustomOptions.PsychometristCanCheckFootprintsTime.GetFloat();
                IsReportCheckedReportDeadbody = CustomOptions.PsychometristIsReportCheckedDeadBody.GetBool();
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
            public static Color32 color = new(0, 255, 255, byte.MaxValue);
            public static float CoolTime;
            public static float BonusCoolTime;
            public static int BonusCount;
            public static List<byte> PhotedPlayerIds;
            public static bool IsPhotographerShared;
            public static bool IsImpostorVision;
            public static bool IsNotification;
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
            public static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PhotographerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {

                PhotographerPlayer = new();
                PhotedPlayerIds = new();
                _photedPlayer = new();
                IsPhotographerShared = false;
                CoolTime = CustomOptions.PhotographerCoolTime.GetFloat();
                BonusCount = (CustomOptions.PhotographerIsBonus.GetBool() ? CustomOptions.PhotographerBonusCount.GetInt() : -1);
                BonusCoolTime = CustomOptions.PhotographerBonusCoolTime.GetFloat();
                IsImpostorVision = CustomOptions.PhotographerIsImpostorVision.GetBool();
                IsNotification = CustomOptions.PhotographerIsNotification.GetBool();
            }
        }
        public static class Stefinder
        {
            public static List<PlayerControl> StefinderPlayer;
            public static Color32 color = new(0, 255, 0, byte.MaxValue);
            public static int KillCoolDown;
            public static bool UseVent;
            public static bool UseSabo;
            public static bool IsKill;
            public static bool SoloWin;
            public static List<byte> IsKillPlayer;
            public static PlayerControl target;
            public static DateTime ButtonTimer;
            public static void ClearAndReload()
            {
                StefinderPlayer = new();
                KillCoolDown = CustomOptions.StefinderKillCoolDown.GetInt();
                UseVent = CustomOptions.StefinderVent.GetBool();
                UseSabo = CustomOptions.StefinderSabo.GetBool();
                SoloWin = CustomOptions.StefinderSoloWin.GetBool();
                IsKill = false;
                IsKillPlayer = new();
            }
        }
        public static class Slugger
        {
            public static List<PlayerControl> SluggerPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static float ChargeTime;
            public static bool IsMultiKill;
            private static Sprite buttonSprite;
            public static Sprite GetButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SluggerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                SluggerPlayer = new();
                CoolTime = CustomOptions.SluggerCoolTime.GetFloat();
                ChargeTime = CustomOptions.SluggerChargeTime.GetFloat();
                IsMultiKill = CustomOptions.SluggerIsMultiKill.GetBool();
            }
        }
        //新ロールクラス
        public static class Quarreled
        {
            public static List<List<PlayerControl>> QuarreledPlayer;
            public static Color32 color = new(210, 105, 30, byte.MaxValue);
            public static bool IsQuarreledWin;
            public static void ClearAndReload()
            {
                QuarreledPlayer = new List<List<PlayerControl>>();
            }
        }
        public static class Lovers
        {
            public static List<List<PlayerControl>> LoversPlayer;
            public static Color32 color = new(255, 105, 180, byte.MaxValue);
            public static bool SameDie;
            public static bool AliveTaskCount;
            public static void ClearAndReload()
            {
                LoversPlayer = new List<List<PlayerControl>>();
                SameDie = CustomOptions.LoversSameDie.GetBool();
                AliveTaskCount = CustomOptions.LoversAliveTaskCount.GetBool();
            }
        }
    }
}
