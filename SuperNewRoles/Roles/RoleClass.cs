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

namespace SuperNewRoles.Roles
{
    [HarmonyPatch]
    public static class RoleClass
    {


        public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);
        public static Color ImpostorRed = Palette.ImpostorRed;
        public static Color CrewmateWhite = Color.white;

        public static void clearAndReloadRoles()
        {
            SuperNewRolesPlugin.Logger.LogInfo("くりああんどりろーどろーるず");
            SoothSayer.clearAndReload();
            Jester.clearAndReload();
            Lighter.clearAndReload();
            EvilLighter.clearAndReload();
            EvilScientist.clearAndReload();
            Sheriff.clearAndReload();
            MeetingSheriff.clearAndReload();
            AllKiller.clearAndReload();
            Teleporter.clearAndReload();
            SpiritMedium.clearAndReload();
            SpeedBooster.clearAndReload();
            EvilSpeedBooster.clearAndReload();
            Tasker.clearAndReload();
            Doorr.clearAndReload();
            EvilDoorr.clearAndReload();
            Sealdor.clearAndReload();
            Speeder.clearAndReload();
            Freezer.clearAndReload();
            Guesser.clearAndReload();
            EvilGuesser.clearAndReload();
            Vulture.clearAndReload();
            NiceScientist.clearAndReload();
            Clergyman.clearAndReload();
            MadMate.clearAndReload();
        }
        public static void NotRole()
        {

        }
        public static class SoothSayer
        {
            public static List<PlayerControl> SoothSayerPlayer;
            public static bool DisplayMode;
            public static Color32 color = new Color32(190, 86, 235, byte.MaxValue);

            public static void clearAndReload()
            {
                SoothSayerPlayer = new List<PlayerControl>();
                DisplayMode = CustomOptions.SoothSayerDisplayMode.getBool();
            }

        }
        public static class Jester
        {
            public static List<PlayerControl> JesterPlayer;
            public static bool IsJesterWin;
            public static Color32 color = new Color32(255, 255, 255, byte.MaxValue);

            public static void clearAndReload()
            {
                IsJesterWin = false;
                JesterPlayer = new List<PlayerControl>();
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
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }
            public static void clearAndReload()
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
            public static float CoolTime;
            public static float DurationTime;

            public static void clearAndReload()
            {
                EvilLighterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilLighterCoolTime.getFloat();
                DurationTime = CustomOptions.EvilLighterDurationTime.getFloat();
            }

        }
        public static class EvilScientist
        {
            public static List<PlayerControl> EvilScientistPlayer;
            public static Color32 color = RoleClass.ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;

            public static void clearAndReload()
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
            public static bool MadMateKill;
            public static float KillMaxCount;
            public static DateTime ButtonTimer;

            private static Sprite buttonSprite;

            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }

            public static void clearAndReload()
            {
                SheriffPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SheriffCoolTime.getFloat();
                MadMateKill = CustomOptions.SheriffMadMateKill.getBool();
                KillMaxCount = CustomOptions.SheriffKillMaxCount.getFloat();
            }

        }
        public static class MeetingSheriff
        {
            public static List<PlayerControl> MeetingSheriffPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static bool MadMateKill;
            public static float KillMaxCount;
            public static bool OneMeetingMultiKill;

            public static void clearAndReload()
            {
                MeetingSheriffPlayer = new List<PlayerControl>();
                MadMateKill = CustomOptions.SheriffMadMateKill.getBool();
                KillMaxCount = CustomOptions.SheriffKillMaxCount.getFloat();
                OneMeetingMultiKill = CustomOptions.MeetingSheriffOneMeetingMultiKill.getBool();
            }

        }
        public static class AllKiller
        {
            public static List<PlayerControl> AllKillerPlayer;
            public static Color32 color = new Color32(69, 69, 69, byte.MaxValue);
            public static float KillCoolDown;
            public static bool CreateSideNewKiller;
            public static bool NewAllKillerCreateSideNewKiller;

            public static void clearAndReload()
            {
                AllKillerPlayer = new List<PlayerControl>();
                KillCoolDown = CustomOptions.AllKillerKillCoolDown.getFloat();
                CreateSideNewKiller = CustomOptions.AllKillerCreateSideNewKiller.getBool();
                NewAllKillerCreateSideNewKiller = CustomOptions.AllKillerNewAllKillerCreateSideNewKiller.getBool();
            }

        }
        public static class Teleporter
        {
            public static List<PlayerControl> TeleporterPlayer;
            public static Color32 color = RoleClass.ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;

            public static void clearAndReload()
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

            public static void clearAndReload()
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
            public static float DefaultSpeed;
            public static DateTime ButtonTimer;
            public static bool IsSpeedBoost;
            public static Sprite GetSpeedBoostButtonSprite() {
                if (SpeedBoostButtonSprite) return SpeedBoostButtonSprite;
                SpeedBoostButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return SpeedBoostButtonSprite;
            }
            
            public static void clearAndReload()
            {
                SpeedBoosterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SpeedBoosterCoolTime.getFloat();
                DurationTime = CustomOptions.SpeedBoosterDurationTime.getFloat();
                Speed = CustomOptions.SpeedBoosterSpeed.getFloat();
                DefaultSpeed = PlayerControl.GameOptions.PlayerSpeedMod;
                IsSpeedBoost = false;
            }
        }
        public static class EvilSpeedBooster
        {
            public static List<PlayerControl> EvilSpeedBoosterPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;
            public static float Speed;
            public static float DefaultSpeed;
            public static bool IsSpeedBoost;
            public static DateTime ButtonTimer;

            public static void clearAndReload()
            {
                ButtonTimer = DateTime.Now;
                EvilSpeedBoosterPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilSpeedBoosterCoolTime.getFloat();
                DurationTime = CustomOptions.EvilSpeedBoosterDurationTime.getFloat();
                Speed = CustomOptions.EvilSpeedBoosterSpeed.getFloat();
                DefaultSpeed = PlayerControl.GameOptions.PlayerSpeedMod;
                IsSpeedBoost = false;
            }
        }
        public static class Tasker
        {
            public static List<PlayerControl> TaskerPlayer;
            public static Color32 color = ImpostorRed;
            public static bool IsKill;
            public static float TaskCount;

            public static void clearAndReload()
            {
                TaskerPlayer = new List<PlayerControl>();
                IsKill = CustomOptions.TaskerIsKill.getBool();
                TaskCount = CustomOptions.TaskerAmount.getFloat();
            }
        }
        public static class Doorr
        {
            public static List<PlayerControl> DoorrPlayer;
            public static Color32 color = new Color32(205, 133, 63, byte.MaxValue);
            public static float CoolTime;
            public static void clearAndReload()
            {
                DoorrPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.DoorrCoolTime.getFloat();
            }
        }
        public static class EvilDoorr
        {
            public static List<PlayerControl> EvilDoorrPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static void clearAndReload()
            {
                SuperNewRolesPlugin.Logger.LogInfo("EvilDoorrクリアーーーーーー！！！！！");
                EvilDoorrPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.EvilDoorrCoolTime.getFloat();
            }
        }
        public static class Sealdor
        {
            public static List<PlayerControl> SealdorPlayer;
            public static Color32 color = new Color32(100, 149, 237, byte.MaxValue);
            public static float CoolTime;
            public static float DurationTime;
            public static void clearAndReload()
            {
                SealdorPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SealdorCoolTime.getFloat();
                DurationTime = CustomOptions.SealdorDurationTime.getFloat();
            }
        }
        public static class Freezer
        {
            public static List<PlayerControl> FreezerPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;
            public static void clearAndReload()
            {
                FreezerPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.FreezerCoolTime.getFloat();
                DurationTime = CustomOptions.FreezerDurationTime.getFloat();
            }
        }

        public static class Speeder
        {
            public static List<PlayerControl> SpeederPlayer;
            public static Color32 color = ImpostorRed;
            public static float CoolTime;
            public static float DurationTime;
            public static void clearAndReload()
            {
                SpeederPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.SpeederCoolTime.getFloat();
                DurationTime = CustomOptions.SpeederDurationTime.getFloat();
            }
        }
        public static class Guesser
        {
            public static List<PlayerControl> GuesserPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static void clearAndReload()
            {
                GuesserPlayer = new List<PlayerControl>();
            }
        }
        public static class EvilGuesser
        {
            public static List<PlayerControl> EvilGuesserPlayer;
            public static Color32 color = ImpostorRed;
            public static void clearAndReload()
            {
                EvilGuesserPlayer = new List<PlayerControl>();
            }
        }
        public static class Vulture
        {
            public static List<PlayerControl> VulturePlayer;
            public static Color32 color = new Color32(205, 133, 63, byte.MaxValue);
            public static void clearAndReload()
            {
                VulturePlayer = new List<PlayerControl>();
            }
        }
        public static class NiceScientist
        {
            public static List<PlayerControl> NiceScientistPlayer;
            public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
            public static void clearAndReload()
            {
                NiceScientistPlayer = new List<PlayerControl>();
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


            private static Sprite buttonSprite;
            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.ClergymanLightOutButton.png", 115f);
                return buttonSprite;
            }
            public static void clearAndReload()
            {
                ClergymanPlayer = new List<PlayerControl>();
                CoolTime = CustomOptions.ClergymanCoolTime.getFloat();
                DurationTime = CustomOptions.ClergymanDurationTime.getFloat();
                IsLightOff = false;
                DownImpoVision = CustomOptions.ClergymanDownVision.getFloat();
                DefaultImpoVision = PlayerControl.GameOptions.ImpostorLightMod;
            }
        }
        public static class MadMate
        {
            public static List<PlayerControl> MadMatePlayer;
            public static Color32 color = ImpostorRed;
            public static void clearAndReload()
            {
                MadMatePlayer = new List<PlayerControl>();
            }
        }

    }
}