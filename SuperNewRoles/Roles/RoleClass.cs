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
            EndGame.FinalStatusPatch.FinalStatusData.ClearFinalStatusData();
            SoothSayer.clearAndReload();
            Jester.clearAndReload();
            Lighter.clearAndReload();
            EvilLighter.clearAndReload();
            EvilScientist.clearAndReload();
            Sheriff.clearAndReload();
            MeetingSheriff.clearAndReload();
            Jackal.clearAndReload();
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
            //ロールクリア
            Quarreled.ClearAndReload();
            MapOptions.MapOption.ClearAndReload();
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
            public static void clearAndReload()
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
            public static Color32 color = new Color32(255, 255, 255, byte.MaxValue);
            public static bool IsUseVent;
            public static bool IsUseSabo;
            public static bool IsJesterTaskClearWin;
            public static void clearAndReload()
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
            //public static float CoolTime;
            //public static float DurationTime;

            public static void clearAndReload()
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
            //public static float CoolTime;
            //public static float DurationTime;

            public static void clearAndReload()
            {
                EvilScientistPlayer = new List<PlayerControl>();
                //CoolTime = CustomOptions.EvilScientistCoolTime.getFloat();
                //DurationTime = CustomOptions.EvilScientistDurationTime.getFloat();
            }

        }
        public static class Sheriff
        {
            public static List<PlayerControl> SheriffPlayer;
            public static Color32 color = new Color32(255, 255, 0, byte.MaxValue);
            public static PlayerControl currentTarget;
            public static float CoolTime;
            public static bool IsMadMateKill;
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
                IsMadMateKill = CustomOptions.SheriffMadMateKill.getBool();
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

            private static Sprite buttonSprite;

            public static Sprite getButtonSprite()
            {
                if (buttonSprite) return buttonSprite;
                buttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SheriffKillButton.png", 115f);
                return buttonSprite;
            }
            public static void clearAndReload()
            {
                MeetingSheriffPlayer = new List<PlayerControl>();
                MadMateKill = CustomOptions.MeetingSheriffMadMateKill.getBool();
                KillMaxCount = CustomOptions.MeetingSheriffKillMaxCount.getFloat();
                OneMeetingMultiKill = CustomOptions.MeetingSheriffOneMeetingMultiKill.getBool();
            }

        }
        public static class Jackal
        {
            public static List<PlayerControl> JackalPlayer;
            public static List<PlayerControl> SidekickPlayer;
            public static Color32 color = new Color32(0,255,255, byte.MaxValue);
            public static float KillCoolDown;
            public static bool IsUseVent;
            public static bool IsUseSabo;
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
            public static void clearAndReload()
            {
                JackalPlayer = new List<PlayerControl>();
                SidekickPlayer = new List<PlayerControl>();
                KillCoolDown = CustomOptions.JackalKillCoolDown.getFloat();
                IsUseVent = CustomOptions.JackalUseVent.getBool();
                IsUseVent = CustomOptions.JackalUseSabo.getBool();
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
            public static DateTime ButtonTimer;
            public static bool IsSpeedBoost;
            public static Dictionary<int, bool> IsBoostPlayers;
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
                Speed = CustomOptions.SpeedBoosterSpeed.getFloat() / 100f;
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
            public static void clearAndReload()
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

            public static void clearAndReload()
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
            public static void clearAndReload()
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
            //public static float CoolTime;
            //public static float DurationTime;
            public static void clearAndReload()
            {
                SealdorPlayer = new List<PlayerControl>();
                //CoolTime = CustomOptions.SealdorCoolTime.getFloat();
                //DurationTime = CustomOptions.SealdorDurationTime.getFloat();
            }
        }
        public static class Freezer
        {
            public static List<PlayerControl> FreezerPlayer;
            public static Color32 color = ImpostorRed;
            //public static float CoolTime;
            //public static float DurationTime;
            private static Sprite ButtonSprite;
            public static Sprite GetButtonSprite()
            {
                if (ButtonSprite) return ButtonSprite;
                ButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);
                return ButtonSprite;
            }
            public static void clearAndReload()
            {
                FreezerPlayer = new List<PlayerControl>();
                //CoolTime = CustomOptions.FreezerCoolTime.getFloat();
                //DurationTime = CustomOptions.FreezerDurationTime.getFloat();
            }
        }

        public static class Speeder
        {
            public static List<PlayerControl> SpeederPlayer;
            public static Color32 color = ImpostorRed;
            //public static float CoolTime;
            //public static float DurationTime;
            public static void clearAndReload()
            {
                SpeederPlayer = new List<PlayerControl>();
                //CoolTime = CustomOptions.SpeederCoolTime.getFloat();
                //DurationTime = CustomOptions.SpeederDurationTime.getFloat();
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
            public static float OldButtonTime;

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
                OldButtonTimer = DateTime.Now;
                OldButtonTime = Clergyman.DurationTime;
            }
        }
        public static class MadMate
        {
            public static List<PlayerControl> MadMatePlayer;
            public static Color32 color = ImpostorRed;
            public static bool IsImpostorCheck;
            public static bool IsUseVent;
            public static void clearAndReload()
            {
                MadMatePlayer = new List<PlayerControl>();
                IsImpostorCheck = CustomOptions.MadMateIsCheckImpostor.getBool();
                IsUseVent = CustomOptions.MadMateIsUseVent.getBool();
            }
        }
        public static class Bait
        {
            public static List<PlayerControl> BaitPlayer;
            public static Color32 color = new Color32(222, 184, 135, byte.MaxValue);
            public static bool Reported;
            public static float ReportTime = 0f;

            public static void ClearAndReload()
            {
                BaitPlayer = new List<PlayerControl>();
                Reported = false;
                ReportTime = CustomOptions.BaitReportTime.getFloat();
            }
        }
        public static class HomeSecurityGuard
        {
            public static List<PlayerControl> HomeSecurityGuardPlayer;
            public static Color32 color = new Color32(0,255,0, byte.MaxValue);

            public static void ClearAndReload()
            {
                HomeSecurityGuardPlayer = new List<PlayerControl>();
            }
        }
        public static class StuntMan
        {
            public static List<PlayerControl> StuntManPlayer;
            public static Color32 color = new Color32(0, 255, 0, byte.MaxValue);
            public static Dictionary<int,int> GuardCount;

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
                setpostion = new Vector3(0,0,0);
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
            public static Color32 color = ImpostorRed;
            public static void ClearAndReload()
            {
                EvilGamblerPlayer = new List<PlayerControl>();
                SucCool = (int)CustomOptions.EvilGamblerSucTime.getFloat();
                NotSucCool = (int)CustomOptions.EvilGamblerNotSucTime.getFloat();
                var temp = CustomOptions.EvilGamblerSucpar.getString().Replace("0%", "");
                if (temp == "")
                {
                    SucPar = 0;
                } else
                {
                    SucPar = int.Parse(temp);
                }
            }
            //ロールクラス
            public static bool GetSuc() {
                var a = new List<string>();
                for (int i = 0; i < SucPar; i++) {
                    a.Add("Suc");
                }
                for (int i = 0; i < 10 - SucPar; i++)
                {
                    a.Add("No");
                }
                SuperNewRolesPlugin.Logger.LogInfo(a);
                if (ModHelpers.GetRandom<string>(a) == "Suc")
                {
                    return true;
                }
                else {
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
                SuperNewRolesPlugin.Logger.LogInfo("mapid"+Constants.MapNames[PlayerControl.GameOptions.MapId]);
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
            public static void ClearAndReload()
            {
                GodPlayer = new List<PlayerControl>();
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
            public static bool IsJackalCheck;
            public static bool IsUseVent;
            public static void ClearAndReload()
            {
                JackalFriendsPlayer = new List<PlayerControl>();
                IsJackalCheck = CustomOptions.JackalFriendsIsCheckJackal.getBool();
                IsUseVent = CustomOptions.JackalFriendsIsUseVent.getBool();
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
                VitalSprite = HudManager.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
                return VitalSprite;
            }
            public static void ClearAndReload()
            {
                DoctorPlayer = new List<PlayerControl>();
                MyPanelFlag = false;
                Vital = null;
            }
        }
        //新ロールクラス
        public static class Quarreled
        {
            public static List<List<PlayerControl>> QuarreledPlayer;
            public static Color32 color = new Color32(210,105, 30, byte.MaxValue);
            public static bool IsQuarreledWin;
            public static void ClearAndReload()
            {
                QuarreledPlayer = new List<List<PlayerControl>>();
            }
        }
    }
}