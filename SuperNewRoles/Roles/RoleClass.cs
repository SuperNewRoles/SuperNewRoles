using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomObject;
using TMPro;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Sabotage;
using SuperNewRoles.Patch;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch]
    public static class RoleClass
    {

        public static bool IsMeeting;
        public static bool IsCoolTimeSetted;
        public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);
        public static Color ImpostorRed = Palette.ImpostorRed;
        public static Color CrewmateWhite = Color.white;
        public static Color FoxPurple = Palette.Purple;
        public static bool IsStart;

        public static void ClearAndReloadRoles()
        {
            RoleHelpers.DeadCaches = new Dictionary<byte, bool>();
            LateTask.Tasks = new List<LateTask>();
            LateTask.AddTasks = new List<LateTask>();
            BotManager.AllBots = new List<PlayerControl>();
            IsMeeting = false;
            IsCoolTimeSetted = false;
            IsStart = false;
            Map.Data.ClearAndReloads();
            SabotageManager.ClearAndReloads();
            Madmate.CheckedImpostor = new List<byte>();
            Roles.MadMayor.CheckedImpostor = new List<byte>();
            Roles.MadSeer.CheckedImpostor = new List<byte>();
            Roles.JackalFriends.CheckedJackal = new List<byte>();
            Mode.BattleRoyal.main.VentData = new Dictionary<byte, int?>();
            EndGame.FinalStatusPatch.FinalStatusData.ClearFinalStatusData();
            Mode.ModeHandler.ClearAndReload();
            MapRemodeling.AdditionalVents.ClearAndReload();
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
            truelover.ClearAndReload();
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
            //ロールクリア
            Quarreled.ClearAndReload();
            Lovers.ClearAndReload();
            MapOptions.MapOption.ClearAndReload();
            ChacheManager.Load();
        }
        public static void NotRole()
        {

        }
        public static class SoothSayer
        {
            public static List<PlayerControl> SoothSayerPlayer;
            public static bool DisplayMode;
            public static int Count;
            public static Color32 color = new Color32(190, 86, 235, byte.MaxValue);
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SoothSayerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                SoothSayerPlayer = new List<PlayerControl>();
                DisplayMode = CustomOptions.SoothSayerDisplayMode.getBool();
                Count = (int)CustomOptions.SoothSayerMaxCount.getFloat();
            }

        }
        public static class Jester
        {
            public static List<PlayerControl> JesterPlayer;
            public static bool IsJesterWin;
            public static Color32 color = new Color32(255, 165, 0, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsJesterTaskClearWin;
            public static void ClearAndReload()
            {
                IsJesterWin = false;
                JesterPlayer = new List<PlayerControl>();
                IsUseSabo = CustomOptions.JesterIsSabotage.getBool();
                IsUseVent = CustomOptions.JesterIsVent.getBool();
                IsJesterTaskClearWin = CustomOptions.JesterIsWinCleartask.getBool();
            }

        }
        public static class Lighter
        {
            public static List<PlayerControl> LighterPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static bool IsLightOn;
            public static float UpVision;
            public static float DefaultCrewVision;
            public static DateTime ButtonTimer;

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.LighterLightOnButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                LighterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.LighterCoolTime.getFloat();
                DurationTime = CustomOptions.LighterDurationTime.getFloat();
                UpVision = CustomOptions.LighterUpVision.getFloat();
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
                EvilLighterPlayer = new List<PlayerControl>();
                //CoolTime = CustomOptions.EvilLighterCoolTime.getFloat();
                //DurationTime = CustomOptions.EvilLighterDurationTime.getFloat();
            }

        }
        public static class EvilScientist
        {
            public static List<PlayerControl> EvilScientistPlayer;
            public static Color32 color = RoleClass.ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.EvilScientistButton.png.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                EvilScientistPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilScientistCoolTime.getFloat();
                DurationTime = CustomOptions.EvilScientistDurationTime.getFloat();
            }

        }
        public static class Sheriff
        {
            public static List<PlayerControl> SheriffPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static PlayerControl currentTarget;
            public static float CoolTime;
            public static bool IsNeutralKill;
            public static bool IsLoversKill;
            public static bool IsMadRoleKill;
            public static bool MadRoleKill;
            public static float KillMaxCount;
            public static Dictionary<int, int> KillCount;
            public static DateTime ButtonTimer;

            private static Sprite buttonSprite;

            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }

            public static void ClearAndReload()
            {
                SheriffPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SheriffCoolTime.getFloat();
                IsNeutralKill = CustomOptions.SheriffNeutralKill.getBool();
                IsLoversKill = CustomOptions.SheriffLoversKill.getBool();
                IsMadRoleKill = CustomOptions.SheriffMadRoleKill.getBool();
                MadRoleKill = CustomOptions.SheriffMadRoleKill.getBool();
                KillMaxCount = CustomOptions.SheriffKillMaxCount.getFloat();
                KillCount = new Dictionary<int, int>();
            }

        }
        public static class MeetingSheriff
        {
            public static List<PlayerControl> MeetingSheriffPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static bool NeutralKill;
            public static bool MadRoleKill;
            public static float KillMaxCount;
            public static bool OneMeetingMultiKill;

            private static Sprite buttonSprite;

            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                MeetingSheriffPlayer = new List<PlayerControl>();
                NeutralKill = CustomOptions.MeetingSheriffNeutralKill.getBool();
                MadRoleKill = CustomOptions.MeetingSheriffMadRoleKill.getBool();
                MadRoleKill = CustomOptions.MeetingSheriffMadRoleKill.getBool();
                KillMaxCount = CustomOptions.MeetingSheriffKillMaxCount.getFloat();
                OneMeetingMultiKill = CustomOptions.MeetingSheriffOneMeetingMultiKill.getBool();
            }

        }
        public static class Jackal
        {
            public static List<PlayerControl> JackalPlayer;
            public static List<PlayerControl> SidekickPlayer;
            public static List<PlayerControl> FakeSidekickPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
            public static float KillCoolDown;
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsImpostorLight;
            public static bool CreateSidekick;
            public static bool NewJackalCreateSidekick;
            public static bool IsCreateSidekick;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.JackalSidekickButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                JackalPlayer = new List<PlayerControl>();
                SidekickPlayer = new List<PlayerControl>();
                FakeSidekickPlayer = new List<PlayerControl>();
                KillCoolDown = CustomOptions.JackalKillCoolDown.getFloat();
                IsUseVent = CustomOptions.JackalUseVent.getBool();
                IsUseSabo = CustomOptions.JackalUseSabo.getBool();
                IsImpostorLight = CustomOptions.JackalIsImpostorLight.getBool();
                CreateSidekick = CustomOptions.JackalCreateSidekick.getBool();
                IsCreateSidekick = CustomOptions.JackalCreateSidekick.getBool();
                NewJackalCreateSidekick = CustomOptions.JackalNewJackalCreateSidekick.getBool();
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
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                TeleporterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.TeleporterCoolTime.getFloat();
                DurationTime = CustomOptions.TeleporterDurationTime.getFloat();
            }

        }
        public static class SpiritMedium
        {
            public static List<PlayerControl> SpiritMediumPlayer;
            public static Color32 color = new Color32(0, 191, 255, byte.MaxValue);
            public static bool DisplayMode;
            public static float MaxCount;

            public static void ClearAndReload()
            {
                SpiritMediumPlayer = new List<PlayerControl>();
                DisplayMode = CustomOptions.SpiritMediumDisplayMode.getBool();
                MaxCount = CustomOptions.SpiritMediumMaxCount.getFloat();
            }

        }
        public static class SpeedBooster
        {
            public static List<PlayerControl> SpeedBoosterPlayer;
            public static Color32 color = new Color32(100, 149, 237, byte.MaxValue);
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
                SpeedBoostButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return SpeedBoostButtonSprite;
            }

            public static void ClearAndReload()
            {
                SpeedBoosterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SpeedBoosterCoolTime.getFloat();
                DurationTime = CustomOptions.SpeedBoosterDurationTime.getFloat();
                Speed = CustomOptions.SpeedBoosterSpeed.getFloat();
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
            public static float Speed { get { return CustomOptions.EvilSpeedBoosterSpeed.getFloat(); } }
            public static bool IsSpeedBoost;
            public static DateTime ButtonTimer;
            public static Dictionary<int, bool> IsBoostPlayers;
            public static void ClearAndReload()
            {
                ButtonTimer = DateTime.Now;
                EvilSpeedBoosterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilSpeedBoosterCoolTime.getFloat();
                DurationTime = CustomOptions.EvilSpeedBoosterDurationTime.getFloat();
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
                TaskerPlayer = new List<PlayerControl>();
                //IsKill = CustomOptions.TaskerIsKill.getBool();
                //TaskCount = CustomOptions.TaskerAmount.getFloat();
            }
        }
        public static class Doorr
        {
            public static List<PlayerControl> DoorrPlayer;
            public static Color32 color = new Color32(205, 133, 63, byte.MaxValue);
            public static float CoolTime;
            public static DateTime ButtonTimer;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.DoorrDoorButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                ButtonTimer = DateTime.Now;
                DoorrPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.DoorrCoolTime.getFloat();
            }
        }
        public static class EvilDoorr
        {
            public static List<PlayerControl> EvilDoorrPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static void ClearAndReload()
            {
                EvilDoorrPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilDoorrCoolTime.getFloat();
            }
        }
        public static class Shielder
        {
            public static List<PlayerControl> ShielderPlayer;
            public static Color32 color = new Color32(100, 149, 237, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static Dictionary<byte, bool> IsShield;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.ShielderButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                ShielderPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.ShielderCoolTime.getFloat();
                DurationTime = CustomOptions.ShielderDurationTime.getFloat();
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
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.FreezerButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                FreezerPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.FreezerCoolTime.getFloat();
                DurationTime = CustomOptions.FreezerDurationTime.getFloat();
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
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedDownButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                SpeederPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SpeederCoolTime.getFloat();
                DurationTime = CustomOptions.SpeederDurationTime.getFloat();
                IsSpeedDown = false;
            }
        }
        public static class Guesser
        {
            public static List<PlayerControl> GuesserPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static void ClearAndReload()
            {
                GuesserPlayer = new List<PlayerControl>();
            }
        }
        public static class EvilGuesser
        {
            public static List<PlayerControl> EvilGuesserPlayer;
            public static Color32 color = ImpostorRed;
            public static void ClearAndReload()
            {
                EvilGuesserPlayer = new List<PlayerControl>();
            }
        }
        public static class Vulture
        {
            public static List<PlayerControl> VulturePlayer;
            public static Color32 color = new Color32(205, 133, 63, byte.MaxValue);
            public static int DeadBodyMaxCount;
            public static float CoolTime;
            public static int DeadBodyCount;
            public static bool IsUseVent;
            public static bool ShowArrows;
            public static Arrow Arrow;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.VultureButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                VulturePlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.VultureCoolDown.getFloat();
                DeadBodyCount = (int)CustomOptions.VultureDeadBodyMaxCount.getFloat();
                IsUseVent = CustomOptions.VultureIsUseVent.getBool();
                ShowArrows = CustomOptions.VultureShowArrows.getBool();
                RoleClass.Vulture.Arrow = null;
            }
        }
        public static class NiceScientist
        {
            public static List<PlayerControl> NiceScientistPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static DateTime ButtonTimer;
            public static bool IsScientist;
            private static Sprite buttonSprite;
            public static Dictionary<int, bool> IsScientistPlayers;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.NiceScientistButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                NiceScientistPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.NiceScientistCoolTime.getFloat();
                DurationTime = CustomOptions.NiceScientistDurationTime.getFloat();
                ButtonTimer = DateTime.Now;
                IsScientist = false;
                IsScientistPlayers = new Dictionary<int, bool>();
            }
        }
        public static class Clergyman
        {
            public static List<PlayerControl> ClergymanPlayer;
            public static Color32 color = new Color32(255, 0, 255, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static bool IsLightOff;
            public static float DownImpoVision;
            public static float DefaultImpoVision;
            public static DateTime ButtonTimer;
            public static DateTime OldButtonTimer;
            public static float OldButtonTime;

            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.ClergymanLightOutButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                ClergymanPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.ClergymanCoolTime.getFloat();
                DurationTime = CustomOptions.ClergymanDurationTime.getFloat();
                IsLightOff = false;
                DownImpoVision = CustomOptions.ClergymanDownVision.getFloat();
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
                MadMatePlayer = new List<PlayerControl>();
                IsImpostorCheck = CustomOptions.MadMateIsCheckImpostor.getBool();
                IsUseVent = CustomOptions.MadMateIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadMateIsImpostorLight.getBool();
                int Common = (int)CustomOptions.MadMateCommonTask.getFloat();
                int Long = (int)CustomOptions.MadMateLongTask.getFloat();
                int Short = (int)CustomOptions.MadMateShortTask.getFloat();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MadMateCheckImpostorTask.getString().Replace("%", "")) / 100f));
            }
        }
        public static class Bait
        {
            public static List<PlayerControl> BaitPlayer;
            public static Color32 color = new Color32(222, 184, 135, byte.MaxValue);
            public static bool Reported;
            public static List<int> ReportedPlayer;
            public static float ReportTime = 0f;

            public static void ClearAndReload()
            {
                BaitPlayer = new List<PlayerControl>();
                Reported = false;
                ReportTime = CustomOptions.BaitReportTime.getFloat();
                ReportedPlayer = new List<int>();
            }
        }
        public static class HomeSecurityGuard
        {
            public static List<PlayerControl> HomeSecurityGuardPlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);

            public static void ClearAndReload()
            {
                HomeSecurityGuardPlayer = new List<PlayerControl>();
            }
        }
        public static class StuntMan
        {
            public static List<PlayerControl> StuntManPlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            public static Dictionary<int, int> GuardCount;

            public static void ClearAndReload()
            {
                StuntManPlayer = new List<PlayerControl>();
                GuardCount = new Dictionary<int, int>();
            }
        }
        public static class Moving
        {
            public static List<PlayerControl> MovingPlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            public static float CoolTime;
            public static DateTime ButtonTimer;
            public static Vector3 setpostion;
            private static Sprite nosetbuttonSprite;
            private static Sprite setbuttonSprite;
            public static Sprite getNoSetButtonSprite()
            {
                if (setbuttonSprite) return setbuttonSprite;
                setbuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.MovingLocationSetButton.png", 115f);
                return setbuttonSprite;
            }
            public static Sprite getSetButtonSprite()
            {
                if (nosetbuttonSprite) return nosetbuttonSprite;
                nosetbuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.MovingTpButton.png", 115f);
                return nosetbuttonSprite;
            }
            public static void ClearAndReload()
            {
                MovingPlayer = new List<PlayerControl>();
                setpostion = new Vector3(0, 0, 0);
                CoolTime = CustomOptions.MovingCoolTime.getFloat();
            }
        }
        public static class Opportunist
        {
            public static List<PlayerControl> OpportunistPlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            public static void ClearAndReload()
            {
                OpportunistPlayer = new List<PlayerControl>();
            }
        }
        public static class NiceGambler
        {
            public static List<PlayerControl> NiceGamblerPlayer;
            //public static int Num;
            public static Color32 color = new Color32(218, 112, 214, byte.MaxValue);
            public static void ClearAndReload()
            {
                NiceGamblerPlayer = new List<PlayerControl>();
                //Num = (int)CustomOptions.NiceGamblerUseCount.getFloat();
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
                EvilGamblerPlayer = new List<PlayerControl>();
                IsSuc = false;
                SucCool = (int)CustomOptions.EvilGamblerSucTime.getFloat();
                NotSucCool = (int)CustomOptions.EvilGamblerNotSucTime.getFloat();
                var temp = CustomOptions.EvilGamblerSucpar.getString().Replace("0%", "");
                if (temp == "")
                {
                    SucPar = 0;
                }
                else
                {
                    SucPar = int.Parse(temp);
                }
            }
            public static bool GetSuc()
            {
                var a = new List<string>();
                for (int i = 0; i < SucPar; i++)
                {
                    a.Add("Suc");
                }
                for (int i = 0; i < 10 - SucPar; i++)
                {
                    a.Add("No");
                }
                if (ModHelpers.GetRandom<string>(a) == "Suc")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static class Bestfalsecharge
        {
            public static List<PlayerControl> BestfalsechargePlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            public static bool IsOnMeeting;
            public static void ClearAndReload()
            {
                BestfalsechargePlayer = new List<PlayerControl>();
                IsOnMeeting = false;
            }
        }
        public static class Researcher
        {
            public static List<PlayerControl> ResearcherPlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            //public static Vector3 SamplePosition;
            private static List<Vector3> SamplePoss = new List<Vector3>() {
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
                ResearcherPlayer = new List<PlayerControl>();
                OKSamplePlayers = new List<PlayerControl>();
                GetSamplePlayers = new List<PlayerControl>();
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
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SelfBomberBomButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                SelfBomberPlayer = new List<PlayerControl>();
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
                GodPlayer = new List<PlayerControl>();
                IsVoteView = CustomOptions.GodViewVote.getBool();
                IsTaskEndWin = CustomOptions.GodIsEndTaskWin.getBool();
            }
        }
        public static class AllCleaner
        {
            public static List<PlayerControl> AllCleanerPlayer;
            public static Color32 color = ImpostorRed;
            public static void ClearAndReload()
            {
                AllCleanerPlayer = new List<PlayerControl>();
            }
        }
        public static class NiceNekomata
        {
            public static List<PlayerControl> NiceNekomataPlayer;
            public static Color32 color = new Color32(244, 164, 96, byte.MaxValue);
            public static bool IsChain;
            public static void ClearAndReload()
            {
                NiceNekomataPlayer = new List<PlayerControl>();
                IsChain = CustomOptions.NiceNekomataIsChain.getBool();
            }
        }
        public static class EvilNekomata
        {
            public static List<PlayerControl> EvilNekomataPlayer;
            public static Color32 color = ImpostorRed;
            public static void ClearAndReload()
            {
                EvilNekomataPlayer = new List<PlayerControl>();
            }
        }
        public static class JackalFriends
        {
            public static List<PlayerControl> JackalFriendsPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsImpostorLight;
            public static bool IsJackalCheck;
            public static int JackalCheckTask;
            public static void ClearAndReload()
            {
                JackalFriendsPlayer = new List<PlayerControl>();

                IsJackalCheck = CustomOptions.JackalFriendsIsCheckJackal.getBool();
                IsUseVent = CustomOptions.JackalFriendsIsUseVent.getBool();
                IsImpostorLight = CustomOptions.JackalFriendsIsImpostorLight.getBool();
                int Common = (int)CustomOptions.JackalFriendsCommonTask.getFloat();
                int Long = (int)CustomOptions.JackalFriendsLongTask.getFloat();
                int Short = (int)CustomOptions.JackalFriendsShortTask.getFloat();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptions.JackalFriendsCheckJackalTask.getString().Replace("%", "")) / 100f));
                Roles.JackalFriends.CheckedJackal = new List<byte>();
            }
        }
        public static class Doctor
        {
            public static List<PlayerControl> DoctorPlayer;
            public static Color32 color = new Color32(102, 102, 255, byte.MaxValue);
            public static bool MyPanelFlag;
            public static Minigame Vital;
            private static Sprite VitalSprite;
            public static Sprite getVitalsSprite()
            {
                if (VitalSprite) return VitalSprite;
                VitalSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
                return VitalSprite;
            }
            public static void ClearAndReload()
            {
                DoctorPlayer = new List<PlayerControl>();
                MyPanelFlag = false;
                Vital = null;
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
            public static Sprite getButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.CountChangerButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                CountChangerPlayer = new List<PlayerControl>();
                ChangeData = new Dictionary<int, int>();
                Setdata = new Dictionary<int, int>();
                Count = (int)CustomOptions.CountChangerMaxCount.getFloat();
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
                PursuerPlayer = new List<PlayerControl>();
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
                MinimalistPlayer = new List<PlayerControl>();
                KillCoolTime = CustomOptions.MinimalistKillCoolTime.getFloat();
                UseVent = CustomOptions.MinimalistVent.getBool();
                UseSabo = CustomOptions.MinimalistSabo.getBool();
                UseReport = CustomOptions.MinimalistReport.getBool();
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
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.HawkHawkEye.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                HawkPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.HawkCoolTime.getFloat();
                DurationTime = CustomOptions.HawkDurationTime.getFloat();
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
            public static Color32 color = new Color32(192, 192, 192, byte.MaxValue);
            public static bool ImpostorLight;
            public static bool UseVent;
            public static bool UseSabo;
            public static bool UseKill;
            public static void ClearAndReload()
            {
                EgoistPlayer = new List<PlayerControl>();
                ImpostorLight = CustomOptions.EgoistImpostorLight.getBool();
                UseVent = CustomOptions.EgoistUseVent.getBool();
                UseSabo = CustomOptions.EgoistUseSabo.getBool();
                UseKill = CustomOptions.EgoistUseKill.getBool();
            }
        }
        public static class NiceRedRidingHood
        {
            public static List<PlayerControl> NiceRedRidingHoodPlayer;
            public static Color32 color = new Color32(250, 128, 114, byte.MaxValue);
            public static int Count;
            public static Vector3? deadbodypos;
            public static void ClearAndReload()
            {
                NiceRedRidingHoodPlayer = new List<PlayerControl>();
                Count = (int)CustomOptions.NiceRedRidingHoodCount.getFloat();
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
                EvilEraserPlayer = new List<PlayerControl>();
                Counts = new Dictionary<int, int>();
                Count = ((int)CustomOptions.EvilEraserMaxCount.getFloat()) - 1;
            }
        }
        public static class Workperson
        {
            public static List<PlayerControl> WorkpersonPlayer;
            public static Color32 color = new Color32(210, 180, 140, byte.MaxValue);
            public static bool IsAliveWin;
            public static void ClearAndReload()
            {
                WorkpersonPlayer = new List<PlayerControl>();
                IsAliveWin = CustomOptions.WorkpersonIsAliveWin.getBool();
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
            public static Sprite getGetButtonSprite()
            {
                if (GetbuttonSprite) return GetbuttonSprite;
                GetbuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.MagazinerGetButton.png", 115f);
                return GetbuttonSprite;
            }
            public static Sprite getAddButtonSprite()
            {
                if (AddbuttonSprite) return AddbuttonSprite;
                AddbuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.MagazinerAddButton.png", 115f);
                return AddbuttonSprite;
            }
            public static void ClearAndReload()
            {
                MagazinerPlayer = new List<PlayerControl>();
                MyPlayerCount = 0;
                SetTime = CustomOptions.MagazinerSetKillTime.getFloat();
                IsOKSet = true;
            }
        }
        public static class Mayor
        {
            public static List<PlayerControl> MayorPlayer;
            public static Color32 color = new Color32(0, 128, 128, byte.MaxValue);
            public static int AddVote;
            public static void ClearAndReload()
            {
                MayorPlayer = new List<PlayerControl>();
                AddVote = (int)CustomOptions.MayorVoteCount.getFloat();
            }
        }
        public static class truelover
        {
            public static List<PlayerControl> trueloverPlayer;
            public static Color32 color = Lovers.color;
            public static bool IsCreate;
            public static List<int> CreatePlayers;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.trueloverloveButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                trueloverPlayer = new List<PlayerControl>();
                IsCreate = false;
                CreatePlayers = new List<int>();
            }
        }
        public static class Technician
        {
            public static List<PlayerControl> TechnicianPlayer;
            public static Color32 color = Color.blue;
            public static void ClearAndReload()
            {
                TechnicianPlayer = new List<PlayerControl>();
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
                SerialKillerPlayer = new List<PlayerControl>();
                SuicideTime = CustomOptions.SerialKillerSuicideTime.getFloat();
                KillTime = CustomOptions.SerialKillerKillTime.getFloat();
                SuicideDefaultTime = SuicideTime;
                IsMeetingReset = CustomOptions.SerialKillerIsMeetingReset.getBool();
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
                OverKillerPlayer = new List<PlayerControl>();
                KillCoolTime = CustomOptions.OverKillerKillCoolTime.getFloat();
                KillCount = (int)CustomOptions.OverKillerKillCount.getFloat();
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
                    LevelingerPlayer = new List<PlayerControl>();
                    ThisXP = 0;
                    IsCreateMadmate = false;
                    OneKillXP = (int)CustomOptions.LevelingerOneKillXP.getFloat();
                    UpLevelXp = (int)CustomOptions.LevelingerUpLevelXP.getFloat();
                    GetPowerData = new List<LevelPowerTypes>();
                    for (int i = 0; i < 5; i++)
                    {
                        string getdata = "";
                        if (i == 0)
                        {
                            getdata = CustomOptions.LevelingerLevelOneGetPower.getString();
                        }
                        else if (i == 1)
                        {
                            getdata = CustomOptions.LevelingerLevelTwoGetPower.getString();
                        }
                        else if (i == 2)
                        {
                            getdata = CustomOptions.LevelingerLevelThreeGetPower.getString();
                        }
                        else if (i == 3)
                        {
                            getdata = CustomOptions.LevelingerLevelFourGetPower.getString();
                        }
                        else if (i == 4)
                        {
                            getdata = CustomOptions.LevelingerLevelFiveGetPower.getString();
                        }
                        GetPowerData.Add(GetLevelPowerType(getdata));
                    }
                    IsUseOKRevive = CustomOptions.LevelingerReviveXP.getBool();
                    ReviveUseXP = (int)CustomOptions.LevelingerUseXPRevive.getFloat();
                }
                catch (Exception e)
                {
                }
            }
            public static bool IsPower(LevelPowerTypes power)
            {
                return GetThisPower() == power;
            }
            public static LevelPowerTypes GetThisPower(int Level = 0, PlayerControl player = null)
            {
                if (player == null) player = PlayerControl.LocalPlayer;
                if (!player.isRole(CustomRPC.RoleId.Levelinger)) return LevelPowerTypes.None;
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
                    if (name == CustomOptions.LevelingerTexts[0])
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("ab");
                        return LevelPowerTypes.None;
                    }
                    else if (name == CustomOptions.LevelingerTexts[1])
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("ac");
                        return LevelPowerTypes.Keep;
                    }
                    else if (name == CustomOptions.LevelingerTexts[2])
                    {
                        return LevelPowerTypes.Pursuer;
                    }
                    else if (name == CustomOptions.LevelingerTexts[3])
                    {
                        return LevelPowerTypes.Teleporter;
                    }
                    else if (name == CustomOptions.LevelingerTexts[4])
                    {
                        return LevelPowerTypes.Sidekick;
                    }
                    else if (name == CustomOptions.LevelingerTexts[5])
                    {
                        return LevelPowerTypes.SpeedBooster;
                    }
                    else if (name == CustomOptions.LevelingerTexts[6])
                    {
                        return LevelPowerTypes.Moving;
                    }
                    else
                    {
                        return LevelPowerTypes.None;
                    }
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
                EvilMovingPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilMovingCoolTime.getFloat();
            }
        }
        public static class Amnesiac
        {
            public static List<PlayerControl> AmnesiacPlayer;
            public static Color32 color = new Color32(125, 125, 125, byte.MaxValue);
            public static void ClearAndReload()
            {
                AmnesiacPlayer = new List<PlayerControl>();
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
                SideKillerPlayer = new List<PlayerControl>();
                MadKillerPlayer = new List<PlayerControl>();
                MadKillerPair = new Dictionary<byte, byte>();
                KillCoolTime = CustomOptions.SideKillerKillCoolTime.getFloat();
                MadKillerCoolTime = CustomOptions.SideKillerMadKillerKillCoolTime.getFloat();
                IsCreateMadKiller = false;
                IsUpMadKiller = false;
            }
            public static PlayerControl getSidePlayer(PlayerControl p)
            {
                if (MadKillerPair.ContainsKey(p.PlayerId))
                {
                    return ModHelpers.playerById(MadKillerPair[p.PlayerId]);
                }
                else if (MadKillerPair.ContainsValue(p.PlayerId))
                {
                    var key = MadKillerPair.GetKey(p.PlayerId);
                    if (key == null) return null;
                    return ModHelpers.playerById((byte)key);
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
                SurvivorPlayer = new List<PlayerControl>();
                KillCoolTime = CustomOptions.SurvivorKillCoolTime.getFloat();
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
                MadMayorPlayer = new List<PlayerControl>();
                AddVote = (int)CustomOptions.MadMayorVoteCount.getFloat();
                IsImpostorCheck = CustomOptions.MadMayorIsCheckImpostor.getBool();
                IsUseVent = CustomOptions.MadMayorIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadMayorIsImpostorLight.getBool();
                int Common = (int)CustomOptions.MadMayorCommonTask.getFloat();
                int Long = (int)CustomOptions.MadMayorLongTask.getFloat();
                int Short = (int)CustomOptions.MadMayorShortTask.getFloat();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MadMayorCheckImpostorTask.getString().Replace("%", "")) / 100f));
                Roles.MadMayor.CheckedImpostor = new List<byte>();
            }
        }
        public static class NiceHawk
        {
            public static List<PlayerControl> NiceHawkPlayer;
            public static Color32 color = new Color32(226, 162, 0, byte.MaxValue);
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
                NiceHawkPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.NiceHawkCoolTime.getFloat();
                DurationTime = CustomOptions.NiceHawkDurationTime.getFloat();
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
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            public static void ClearAndReload()
            {
                BakeryPlayer = new List<PlayerControl>();
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
                MadStuntManPlayer = new List<PlayerControl>();
                IsUseVent = CustomOptions.MadStuntManIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadStuntManIsImpostorLight.getBool();
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
                MadHawkPlayer = new List<PlayerControl>();
                IsUseVent = CustomOptions.MadHawkIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadHawkIsImpostorLight.getBool();
                MadHawkPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.MadHawkCoolTime.getFloat();
                DurationTime = CustomOptions.MadHawkDurationTime.getFloat();
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
                MadJesterPlayer = new List<PlayerControl>();
                IsMadJesterWin = false;
                IsUseVent = CustomOptions.MadJesterIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadJesterIsImpostorLight.getBool();
                IsMadJesterTaskClearWin = CustomOptions.IsMadJesterTaskClearWin.getBool();
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
                FalseChargesPlayer = new List<PlayerControl>();
                AllTurns = new Dictionary<byte, int>();
                FalseChargePlayers = new Dictionary<byte, byte>();
                FalseChargePlayer = 255;
                Turns = 255;
                DefaultTurn = (int)CustomOptions.FalseChargesExileTurn.getFloat();
                CoolTime = CustomOptions.FalseChargesCoolTime.getFloat();

            }
        }
        public static class NiceTeleporter
        {
            public static List<PlayerControl> NiceTeleporterPlayer;
            public static Color32 color = new Color32(0, 0, 128, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static DateTime ButtonTimer;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                NiceTeleporterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.NiceTeleporterCoolTime.getFloat();
                DurationTime = CustomOptions.NiceTeleporterDurationTime.getFloat();
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
                CelebrityPlayer = new List<PlayerControl>();
                ChangeRoleView = CustomOptions.CelebrityChangeRoleView.getBool();
                ViewPlayers = new List<PlayerControl>();
            }
        }
        public static class Nocturnality
        {
            public static List<PlayerControl> NocturnalityPlayer;
            public static Color32 color = new Color32(255, 0, 255, byte.MaxValue);
            public static void ClearAndReload()
            {
                NocturnalityPlayer = new List<PlayerControl>();
            }
        }
        public static class Observer
        {
            public static List<PlayerControl> ObserverPlayer;
            public static Color32 color = new Color32(127, 127, 127, byte.MaxValue);
            public static bool IsVoteView;
            public static void ClearAndReload()
            {
                ObserverPlayer = new List<PlayerControl>();
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
                VampirePlayer = new List<PlayerControl>();
                target = null;
                KillDelay = CustomOptions.VampireKillDelay.getFloat();
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
                FoxPlayer = new List<PlayerControl>();
                KillGuard = new Dictionary<int, int>();
                IsUseVent = CustomOptions.FoxIsUseVent.getBool();
                UseReport = CustomOptions.FoxReport.getBool();
                IsImpostorLight = CustomOptions.FoxIsImpostorLight.getBool();
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
                DarkKillerPlayer = new List<PlayerControl>();
                KillCoolTime = CustomOptions.DarkKillerKillCoolTime.getFloat();
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
                SeerPlayer = new List<PlayerControl>();
                deadBodyPositions = new List<Vector3>();
                limitSoulDuration = CustomOptions.SeerLimitSoulDuration.getBool();
                soulDuration = CustomOptions.SeerSoulDuration.getFloat();
                mode = CustomOptions.SeerMode.getSelection();
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
                MadSeerPlayer = new List<PlayerControl>();
                deadBodyPositions = new List<Vector3>();
                limitSoulDuration = CustomOptions.MadSeerLimitSoulDuration.getBool();
                soulDuration = CustomOptions.MadSeerSoulDuration.getFloat();
                mode = CustomOptions.MadSeerMode.getSelection();

                IsImpostorCheck = CustomOptions.MadSeerIsCheckImpostor.getBool();
                IsUseVent = CustomOptions.MadSeerIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadSeerIsImpostorLight.getBool();
                int Common = (int)CustomOptions.MadSeerCommonTask.getFloat();
                int Long = (int)CustomOptions.MadSeerLongTask.getFloat();
                int Short = (int)CustomOptions.MadSeerShortTask.getFloat();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                ImpostorCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MadSeerCheckImpostorTask.getString().Replace("%", "")) / 100f));
                Roles.MadSeer.CheckedImpostor = new List<byte>();
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
                EvilSeerPlayer = new List<PlayerControl>();
                deadBodyPositions = new List<Vector3>();
                limitSoulDuration = CustomOptions.EvilSeerLimitSoulDuration.getBool();
                soulDuration = CustomOptions.EvilSeerSoulDuration.getFloat();
                mode = CustomOptions.EvilSeerMode.getSelection();
            }
        }
        public static class RemoteSheriff
        {
            public static List<PlayerControl> RemoteSheriffPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
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
                RemoteSheriffPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.RemoteSheriffCoolTime.getFloat();
                IsNeutralKill = CustomOptions.RemoteSheriffNeutralKill.getBool();
                IsLoversKill = CustomOptions.RemoteSheriffLoversKill.getBool();
                IsMadRoleKill = CustomOptions.RemoteSheriffMadRoleKill.getBool();
                MadRoleKill = CustomOptions.RemoteSheriffMadRoleKill.getBool();
                KillMaxCount = CustomOptions.RemoteSheriffKillMaxCount.getFloat();
                KillCount = new Dictionary<int, int>();
                IsKillTeleport = CustomOptions.RemoteSheriffIsKillTeleportSetting.getBool();
                KillCoolTime = CustomOptions.RemoteSheriffCoolTime.getFloat();
            }
        }
        public static class TeleportingJackal
        {
            public static List<PlayerControl> TeleportingJackalPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
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
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                TeleportingJackalPlayer = new List<PlayerControl>();
                KillCoolDown = CustomOptions.TeleportingJackalKillCoolDown.getFloat();
                IsUseVent = CustomOptions.TeleportingJackalUseVent.getBool();
                IsUseSabo = CustomOptions.TeleportingJackalUseSabo.getBool();
                IsImpostorLight = CustomOptions.TeleportingJackalIsImpostorLight.getBool();
                CoolTime = CustomOptions.TeleportingJackalCoolTime.getFloat();
                DurationTime = CustomOptions.TeleportingJackalDurationTime.getFloat();
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
                MadMakerPlayer = new List<PlayerControl>();
                IsUseVent = CustomOptions.MadMakerIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadMakerIsImpostorLight.getBool();
                IsCreateMadmate = false;
                CreatePlayers = new List<int>();
            }
        }

        public static class Demon
        {
            public static List<PlayerControl> DemonPlayer;
            public static Dictionary<byte, List<PlayerControl>> CurseDatas;
            public static Color32 color = new Color32(110, 0, 165, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsCheckImpostor;
            public static bool IsAliveWin;
            public static float CoolTime;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.DemonButton.png", 115f);
                return buttonSprite;
            }

            public static void ClearAndReload()
            {
                DemonPlayer = new List<PlayerControl>();
                CurseDatas = new Dictionary<byte, List<PlayerControl>>();
                IsUseVent = CustomOptions.DemonIsUseVent.getBool();
                CoolTime = CustomOptions.DemonCoolTime.getFloat();
                IsCheckImpostor = CustomOptions.DemonIsCheckImpostor.getBool();
                IsAliveWin = CustomOptions.DemonIsAliveWin.getBool();
            }
        }

        public static class TaskManager
        {
            public static List<PlayerControl> TaskManagerPlayer;
            public static Color32 color = new Color32(153, 255, 255, byte.MaxValue);
            public static void ClearAndReload()
            {
                TaskManagerPlayer = new List<PlayerControl>();
                int Common = (int)CustomOptions.TaskManagerCommonTask.getFloat();
                int Long = (int)CustomOptions.TaskManagerLongTask.getFloat();
                int Short = (int)CustomOptions.TaskManagerShortTask.getFloat();
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
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);

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
                SeerFriendsPlayer = new List<PlayerControl>();

                deadBodyPositions = new List<Vector3>();
                limitSoulDuration = CustomOptions.SeerFriendsLimitSoulDuration.getBool();
                soulDuration = CustomOptions.SeerFriendsSoulDuration.getFloat();
                mode = CustomOptions.SeerFriendsMode.getSelection();

                IsJackalCheck = CustomOptions.SeerFriendsIsCheckJackal.getBool();
                IsUseVent = CustomOptions.SeerFriendsIsUseVent.getBool();
                IsImpostorLight = CustomOptions.SeerFriendsIsImpostorLight.getBool();
                int Common = (int)CustomOptions.SeerFriendsCommonTask.getFloat();
                int Long = (int)CustomOptions.SeerFriendsLongTask.getFloat();
                int Short = (int)CustomOptions.SeerFriendsShortTask.getFloat();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptions.SeerFriendsCheckJackalTask.getString().Replace("%", "")) / 100f));
            }
        }

        public static class JackalSeer
        {
            public static List<PlayerControl> JackalSeerPlayer;
            public static List<PlayerControl> SidekickSeerPlayer;
            public static List<PlayerControl> FakeSidekickSeerPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);

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
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.JackalSeerSidekickButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                JackalSeerPlayer = new List<PlayerControl>();
                SidekickSeerPlayer = new List<PlayerControl>();
                FakeSidekickSeerPlayer = new List<PlayerControl>();

                deadBodyPositions = new List<Vector3>();
                limitSoulDuration = CustomOptions.JackalSeerLimitSoulDuration.getBool();
                soulDuration = CustomOptions.JackalSeerSoulDuration.getFloat();
                mode = CustomOptions.JackalSeerMode.getSelection();

                KillCoolDown = CustomOptions.JackalSeerKillCoolDown.getFloat();
                IsUseVent = CustomOptions.JackalSeerUseVent.getBool();
                IsUseSabo = CustomOptions.JackalSeerUseSabo.getBool();
                IsImpostorLight = CustomOptions.JackalSeerIsImpostorLight.getBool();
                CreateSidekick = CustomOptions.JackalSeerCreateSidekick.getBool();
                IsCreateSidekick = CustomOptions.JackalSeerCreateSidekick.getBool();
                NewJackalCreateSidekick = CustomOptions.JackalSeerNewJackalCreateSidekick.getBool();
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
                AssassinPlayer = new List<PlayerControl>();
                MeetingEndPlayers = new List<byte>();
                TriggerPlayer = null;
                DeadPlayer = null;
                IsImpostorWin = false;
            }
        }
        public static class Marine
        {
            public static List<PlayerControl> MarinePlayer;
            public static Color32 color = new Color32(175, 223, 228, byte.MaxValue);
            public static void ClearAndReload()
            {
                MarinePlayer = new List<PlayerControl>();
            }
        }

        public static class Arsonist
        {
            public static List<PlayerControl> ArsonistPlayer;
            public static Dictionary<byte, List<PlayerControl>> DouseDatas;
            public static Color32 color = new Color32(238, 112, 46, byte.MaxValue);
            public static bool IsUseVent;
            public static float CoolTime;
            public static float DurationTime;
            public static bool TriggerArsonistWin;
            public static bool IsDouse;
            public static PlayerControl DouseTarget;
            private static Sprite DousebuttonSprite;
            private static Sprite IgnitebuttonSprite;
            public static Sprite getDouseButtonSprite()
            {
                if (DousebuttonSprite) return DousebuttonSprite;
                DousebuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.ArsonistDouse.png", 115f);
                return DousebuttonSprite;
            }
            public static Sprite getIgniteButtonSprite()
            {
                if (IgnitebuttonSprite) return IgnitebuttonSprite;
                IgnitebuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.ArsonistIgnite.png", 115f);
                return IgnitebuttonSprite;
            }

            public static void ClearAndReload()
            {
                ArsonistPlayer = new List<PlayerControl>();
                DouseDatas = new Dictionary<byte, List<PlayerControl>>();
                IsUseVent = CustomOptions.ArsonistIsUseVent.getBool();
                CoolTime = CustomOptions.ArsonistCoolTime.getFloat();
                DurationTime = CustomOptions.ArsonistDurationTime.getFloat();
                TriggerArsonistWin = true;
                IsDouse = false;
                DouseTarget = null;
            }
        }

        public static class Chief
        {
            public static List<PlayerControl> ChiefPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static bool IsCreateSheriff;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.ChiefSidekickButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                ChiefPlayer = new List<PlayerControl>();
                IsCreateSheriff = false;
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
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.CleanerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                CleanerPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.CleanerCoolDown.getFloat();
                KillCoolTime = CustomOptions.CleanerKillCoolTime.getFloat();

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
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.CleanerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                MadCleanerPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.MadCleanerCoolDown.getFloat();
                IsUseVent = CustomOptions.MadCleanerIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MadCleanerIsImpostorLight.getBool();
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
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SamuraiButton.png", 115f);
                return ButtonSprite;
            }
            public static void ClearAndReload()
            {
                SamuraiPlayer = new List<PlayerControl>();
                KillCoolTime = CustomOptions.SamuraiKillCoolTime.getFloat();
                SwordCoolTime = CustomOptions.SamuraiSwordCoolTime.getFloat();
                UseVent = CustomOptions.SamuraiVent.getBool();
                UseSabo = CustomOptions.SamuraiSabo.getBool();
                Sword = false;
                SwordedPlayer = new List<byte>();
            }
        }
        public static class MayorFriends
        {
            public static List<PlayerControl> MayorFriendsPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsImpostorLight;
            public static bool IsJackalCheck;
            public static int JackalCheckTask;
            public static int AddVote;
            public static void ClearAndReload()
            {
                MayorFriendsPlayer = new List<PlayerControl>();
                IsJackalCheck = CustomOptions.MayorFriendsIsCheckJackal.getBool();
                IsUseVent = CustomOptions.MayorFriendsIsUseVent.getBool();
                IsImpostorLight = CustomOptions.MayorFriendsIsImpostorLight.getBool();
                int Common = (int)CustomOptions.MayorFriendsCommonTask.getFloat();
                int Long = (int)CustomOptions.MayorFriendsLongTask.getFloat();
                int Short = (int)CustomOptions.MayorFriendsShortTask.getFloat();
                int AllTask = Common + Long + Short;
                if (AllTask == 0)
                {
                    Common = PlayerControl.GameOptions.NumCommonTasks;
                    Long = PlayerControl.GameOptions.NumLongTasks;
                    Short = PlayerControl.GameOptions.NumShortTasks;
                }
                JackalCheckTask = (int)(AllTask * (int.Parse(CustomOptions.MayorFriendsCheckJackalTask.getString().Replace("%", "")) / 100f));
                AddVote = (int)CustomOptions.MayorFriendsVoteCount.getFloat();
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
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.VentMakerButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                VentMakerPlayer = new List<PlayerControl>();
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
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.GhostMechanicRepairButton.png", 115f);
                return buttonSprite;
            }
            public static void ClearAndReload()
            {
                GhostMechanicPlayer = new List<PlayerControl>();
                LimitCount = (int)CustomOptions.GhostMechanicRepairLimit.getFloat();
            }
        }
        public static class EvilHacker
        {
            public static List<PlayerControl> EvilHackerPlayer;
            public static Color32 color = ImpostorRed;
            public static bool IsCreateMadmate;
            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                byte mapId = PlayerControl.GameOptions.MapId;
                UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
                if (mapId == 0 || mapId == 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
                else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
                else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
                buttonSprite = button.Image;
                return buttonSprite; //GMHからの引用
            }
            public static void ClearAndReload()
            {
                EvilHackerPlayer = new List<PlayerControl>();
                IsCreateMadmate = CustomOptions.EvilHackerMadmateSetting.getBool();
            }
        }
        public static class HauntedWolf
        {
            public static List<PlayerControl> HauntedWolfPlayer;
            public static Color32 color = new Color32(50, 0, 25, byte.MaxValue);
            public static void ClearAndReload()
            {
                HauntedWolfPlayer = new List<PlayerControl>();
            }
        }
        //新ロールクラス
        public static class Quarreled
        {
            public static List<List<PlayerControl>> QuarreledPlayer;
            public static Color32 color = new Color32(210, 105, 30, byte.MaxValue);
            public static bool IsQuarreledWin;
            public static void ClearAndReload()
            {
                QuarreledPlayer = new List<List<PlayerControl>>();
            }
        }
        public static class Lovers
        {
            public static List<List<PlayerControl>> LoversPlayer;
            public static Color32 color = new Color32(255, 105, 180, byte.MaxValue);
            public static bool SameDie;
            public static bool AliveTaskCount;
            public static void ClearAndReload()
            {
                LoversPlayer = new List<List<PlayerControl>>();
                SameDie = CustomOptions.LoversSameDie.getBool();
                AliveTaskCount = CustomOptions.LoversAliveTaskCount.getBool();
            }
        }
    }
}

