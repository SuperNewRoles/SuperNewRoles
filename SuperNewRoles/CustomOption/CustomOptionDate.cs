using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using static System.Drawing.Color;
using System.Text;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Roles;

namespace SuperNewRoles.CustomOption
{
    public class CustomOptions
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

        public static string[] rates4 = new string[] {"0%","25%","50%","75%","100%"};

        public static string[] presets = new string[] { "preset1", "preset2", "preset3", "preset4", "preset5", "preset6", "preset7", "preset8", "preset9", "preset10" };
        public static CustomOption presetSelection;

        public static CustomOption specialOptions;
        public static CustomOption hideSettings;

        public static CustomOption crewmateRolesCountMax;
        public static CustomOption impostorRolesCountMax;
        public static CustomOption neutralRolesCountMax;

        public static CustomOption IsDebugMode;

        public static CustomOption DisconnectNotPCOption;

        public static CustomOption DetectiveRate;
        public static CustomOption DetectivePlayerCount;

        public static CustomRoleOption SoothSayerOption;
        public static CustomOption SoothSayerPlayerCount;
        public static CustomOption SoothSayerDisplayMode;
        public static CustomOption SoothSayerMaxCount;

        public static CustomRoleOption JesterOption;
        public static CustomOption JesterPlayerCount;
        public static CustomOption JesterIsVent;
        public static CustomOption JesterIsSabotage;
        public static CustomOption JesterIsWinCleartask;

        public static CustomRoleOption LighterOption;
        public static CustomOption LighterPlayerCount;
        public static CustomOption LighterCoolTime;
        public static CustomOption LighterDurationTime;
        public static CustomOption LighterUpVision;

        public static CustomRoleOption EvilLighterOption;
        public static CustomOption EvilLighterPlayerCount;
        public static CustomOption EvilLighterCoolTime;
        public static CustomOption EvilLighterDurationTime;

        public static CustomRoleOption EvilScientistOption;
        public static CustomOption EvilScientistPlayerCount;
        public static CustomOption EvilScientistCoolTime;
        public static CustomOption EvilScientistDurationTime;

        public static CustomRoleOption SheriffOption;
        public static CustomOption SheriffPlayerCount;
        public static CustomOption SheriffCoolTime;
        public static CustomOption SheriffMadMateKill;
        public static CustomOption SheriffNeutralKill;
        public static CustomOption SheriffKillMaxCount;

        public static CustomRoleOption MeetingSheriffOption;
        public static CustomOption MeetingSheriffPlayerCount;
        public static CustomOption MeetingSheriffMadMateKill;
        public static CustomOption MeetingSheriffNeutralKill;
        public static CustomOption MeetingSheriffKillMaxCount;
        public static CustomOption MeetingSheriffOneMeetingMultiKill;

        public static CustomRoleOption JackalOption;
        public static CustomOption JackalPlayerCount;
        public static CustomOption JackalKillCoolDown;
        public static CustomOption JackalUseVent;
        public static CustomOption JackalUseSabo;
        public static CustomOption JackalCreateSidekick;
        public static CustomOption JackalNewJackalCreateSidekick;

        public static CustomRoleOption TeleporterOption;
        public static CustomOption TeleporterPlayerCount;
        public static CustomOption TeleporterCoolTime;
        public static CustomOption TeleporterDurationTime;

        public static CustomRoleOption SpiritMediumOption;
        public static CustomOption SpiritMediumPlayerCount;
        public static CustomOption SpiritMediumDisplayMode;
        public static CustomOption SpiritMediumMaxCount;

        public static CustomRoleOption SpeedBoosterOption;
        public static CustomOption SpeedBoosterPlayerCount;
        public static CustomOption SpeedBoosterCoolTime;
        public static CustomOption SpeedBoosterDurationTime;
        public static CustomOption SpeedBoosterSpeed;

        public static CustomRoleOption EvilSpeedBoosterOption;
        public static CustomOption EvilSpeedBoosterPlayerCount;
        public static CustomOption EvilSpeedBoosterCoolTime;
        public static CustomOption EvilSpeedBoosterDurationTime;
        public static CustomOption EvilSpeedBoosterSpeed;
        public static CustomOption EvilSpeedBoosterIsNotSpeedBooster;

        public static CustomRoleOption TaskerOption;
        public static CustomOption TaskerPlayerCount;
        public static CustomOption TaskerAmount;
        public static CustomOption TaskerIsKill;

        public static CustomRoleOption DoorrOption;
        public static CustomOption DoorrPlayerCount;
        public static CustomOption DoorrCoolTime;

        public static CustomRoleOption EvilDoorrOption;
        public static CustomOption EvilDoorrPlayerCount;
        public static CustomOption EvilDoorrCoolTime;

        public static CustomRoleOption SealdorOption;
        public static CustomOption SealdorPlayerCount;
        public static CustomOption SealdorCoolTime;
        public static CustomOption SealdorDurationTime;

        public static CustomRoleOption FreezerOption;
        public static CustomOption FreezerPlayerCount;
        public static CustomOption FreezerCoolTime;
        public static CustomOption FreezerDurationTime;

        public static CustomRoleOption SpeederOption;
        public static CustomOption SpeederPlayerCount;
        public static CustomOption SpeederCoolTime;
        public static CustomOption SpeederDurationTime;

        public static CustomRoleOption GuesserOption;
        public static CustomOption GuesserPlayerCount;
        public static CustomOption GuesserShortOneMeetingCount;
        public static CustomOption GuesserShortMaxCount;

        public static CustomRoleOption EvilGuesserOption;
        public static CustomOption EvilGuesserPlayerCount;
        public static CustomOption EvilGuesserShortOneMeetingCount;
        public static CustomOption EvilGuesserShortMaxCount;

        public static CustomRoleOption VultureOption;
        public static CustomOption VulturePlayerCount;
        public static CustomOption VultureCoolDown;
        public static CustomOption VultureDeadBodyMaxCount;

        public static CustomRoleOption NiceScientistOption;
        public static CustomOption NiceScientistPlayerCount;
        public static CustomOption NiceScientistCoolTime;
        public static CustomOption NiceScientistDurationTime;

        public static CustomRoleOption ClergymanOption;
        public static CustomOption ClergymanPlayerCount;
        public static CustomOption ClergymanCoolTime;
        public static CustomOption ClergymanDurationTime;
        public static CustomOption ClergymanDownVision;

        public static CustomRoleOption MadMateOption;
        public static CustomOption MadMatePlayerCount;
        public static CustomOption MadMateIsCheckImpostor;
        public static CustomOption MadMateCheckImpostorTask;
        public static CustomOption MadMateIsUseVent;
        public static CustomOption MadMateIsImpostorLight;

        public static CustomRoleOption BaitOption;
        public static CustomOption BaitPlayerCount;
        public static CustomOption BaitReportTime;

        public static CustomRoleOption HomeSecurityGuardOption;
        public static CustomOption HomeSecurityGuardPlayerCount;

        public static CustomRoleOption StuntManOption;
        public static CustomOption StuntManPlayerCount;
        public static CustomOption StuntManMaxGuardCount;

        public static CustomRoleOption MovingOption;
        public static CustomOption MovingPlayerCount;
        public static CustomOption MovingCoolTime;

        public static CustomRoleOption OpportunistOption;
        public static CustomOption OpportunistPlayerCount;

        public static CustomRoleOption NiceGamblerOption;
        public static CustomOption NiceGamblerPlayerCount;
        public static CustomOption NiceGamblerUseCount;

        public static CustomRoleOption EvilGamblerOption;
        public static CustomOption EvilGamblerPlayerCount;
        public static CustomOption EvilGamblerSucTime;
        public static CustomOption EvilGamblerNotSucTime;
        public static CustomOption EvilGamblerSucpar;

        public static CustomRoleOption BestfalsechargeOption;
        public static CustomOption BestfalsechargePlayerCount;

        public static CustomRoleOption ResearcherOption;
        public static CustomOption ResearcherPlayerCount;
        public static CustomOption ResearcherOneTurnSample;

        public static CustomRoleOption SelfBomberOption;
        public static CustomOption SelfBomberPlayerCount;
        public static CustomOption SelfBomberScope;

        public static CustomRoleOption GodOption;
        public static CustomOption GodPlayerCount;
        public static CustomOption GodViewVote;
        public static CustomOption GodIsEndTaskWin;

        public static CustomRoleOption AllCleanerOption;
        public static CustomOption AllCleanerPlayerCount;
        public static CustomOption AllCleanerCount;

        public static CustomRoleOption NiceNekomataOption;
        public static CustomOption NiceNekomataPlayerCount;
        public static CustomOption NiceNekomataIsChain;

        public static CustomRoleOption EvilNekomataOption;
        public static CustomOption EvilNekomataPlayerCount;

        public static CustomRoleOption JackalFriendsOption;
        public static CustomOption JackalFriendsPlayerCount;
        public static CustomOption JackalFriendsIsCheckJackal;
        public static CustomOption JackalFriendsIsUseVent;

        public static CustomRoleOption DoctorOption;
        public static CustomOption DoctorPlayerCount;

        public static CustomRoleOption CountChangerOption;
        public static CustomOption CountChangerPlayerCount;
        public static CustomOption CountChangerMaxCount;
        public static CustomOption CountChangerNextTurn;

        public static CustomRoleOption PursuerOption;
        public static CustomOption PursuerPlayerCount;

        public static CustomRoleOption MinimalistOption;
        public static CustomOption MinimalistPlayerCount;
        public static CustomOption MinimalistKillCoolTime;
        public static CustomOption MinimalistVent;
        public static CustomOption MinimalistSabo;
        public static CustomOption MinimalistReport;

        public static CustomRoleOption HawkOption;
        public static CustomOption HawkPlayerCount;
        public static CustomOption HawkCoolTime;
        public static CustomOption HawkDurationTime;

        public static CustomRoleOption EgoistOption;
        public static CustomOption EgoistPlayerCount;
        public static CustomOption EgoistUseVent;
        public static CustomOption EgoistUseSabo;
        public static CustomOption EgoistImpostorLight;

        public static CustomRoleOption NiceRedRidingHoodOption;
        public static CustomOption NiceRedRidingHoodPlayerCount;
        public static CustomOption NiceRedRidingHoodCount;

        public static CustomRoleOption EvilEraserOption;
        public static CustomOption EvilEraserPlayerCount;
        public static CustomOption EvilEraserMaxCount;

        public static CustomRoleOption WorkpersonOption;
        public static CustomOption WorkpersonPlayerCount;
        public static CustomOption WorkpersonCommonTask;
        public static CustomOption WorkpersonLongTask;
        public static CustomOption WorkpersonShortTask;

        public static CustomRoleOption MagazinerOption;
        public static CustomOption MagazinerPlayerCount;
        public static CustomOption MagazinerSetKillTime;

        public static CustomRoleOption MayorOption;
        public static CustomOption MayorPlayerCount;
        public static CustomOption MayorVoteCount;

        public static CustomRoleOption trueloverOption;
        public static CustomOption trueloverPlayerCount;

        public static CustomRoleOption TechnicianOption;
        public static CustomOption TechnicianPlayerCount;

        public static CustomOption QuarreledOption;
        public static CustomOption QuarreledTeamCount;
        public static CustomOption QuarreledOnlyCrewMate;

        public static CustomOption LoversOption;
        public static CustomOption LoversTeamCount;
        public static CustomOption LoversPar;
        public static CustomOption LoversOnlyCrewMate;
        public static CustomOption LoversSingleTeam;
        public static CustomOption LoversSameDie;
        public static CustomOption LoversAliveTaskCount;
        public static CustomOption LoversDuplicationQuarreled; 

        private static string[] GuesserCount = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
        private static string[] VultureDeadBodyCount = new string[] { "1", "2", "3", "4", "5", "6" };
        public static List<float> CrewPlayers = new List<float> { 1f,1f,15f,1f};
        public static List<float> ImpostorPlayers = new List<float> { 1f, 1f, 5f, 1f };
        public static List<float> QuarreledPlayers = new List<float> { 1f,1f,7f,1f};
        // public static CustomOption ;

        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), ModTranslation.getString(s));
        }


        public static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load()
        {
            presetSelection = CustomOption.Create(0, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingpresetSelection"), presets, null, true);

            specialOptions = new CustomOptionBlank(null);
            hideSettings = CustomOption.Create(2, cs(Color.white, "SettingsHideSetting"), false, specialOptions);

            crewmateRolesCountMax = CustomOption.Create(3, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewRole"), 0f, 0f, 15f, 1f);
            neutralRolesCountMax = CustomOption.Create(4, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralRole"), 0f, 0f, 15f, 1f);
            impostorRolesCountMax = CustomOption.Create(5, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoRole"), 0f, 0f, 3f, 1f);

            if (ConfigRoles.DebugMode.Value) {
                IsDebugMode = CustomOption.Create(159, "デバッグモード", false, null, isHeader: true);
            }

            DisconnectNotPCOption = CustomOption.Create(168, cs(Color.white, "PC以外はキックする"), true,null,isHeader:true);

            //SoothSayerRate = CustomOption.Create(2, cs(SoothSayer.color,"soothName"),rates, null, true);
            Mode.ModeHandler.OptionLoad();

            SoothSayerOption = new CustomRoleOption(6, "SoothSayerName", RoleClass.SoothSayer.color, 1);
            SoothSayerPlayerCount = CustomOption.Create(7, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
            SoothSayerDisplayMode = CustomOption.Create(8, ModTranslation.getString("SoothSayerDisplaySetting"), false, SoothSayerOption);
            SoothSayerMaxCount = CustomOption.Create(9, cs(Color.white, "SoothSayerMaxCountSetting"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);

            JesterOption = new CustomRoleOption(10, "JesterName", RoleClass.Jester.color, 1);
            JesterPlayerCount = CustomOption.Create(11, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JesterOption);
            JesterIsVent = CustomOption.Create(12, ModTranslation.getString("JesterIsVentSetting"), false, JesterOption);
            JesterIsSabotage = CustomOption.Create(13, ModTranslation.getString("JesterIsSabotageSetting"), false, JesterOption);
            JesterIsWinCleartask = CustomOption.Create(113, ModTranslation.getString("JesterIsWinClearTaskSetting"), false, JesterOption);

            LighterOption = new CustomRoleOption(14, "LighterName", RoleClass.Lighter.color, 1);
            LighterPlayerCount = CustomOption.Create(15, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LighterOption);
            LighterCoolTime = CustomOption.Create(16, ModTranslation.getString("LigtherCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, LighterOption, format: "unitSeconds");
            LighterDurationTime = CustomOption.Create(17, ModTranslation.getString("LigtherDurationSetting"), 10f, 1f, 20f, 0.5f, LighterOption, format: "unitSeconds");
            LighterUpVision = CustomOption.Create(204, ModTranslation.getString("LighterUpVisionSetting"), 0.25f, 0f, 5f, 0.25f, LighterOption);

            /**
            EvilLighterOption = new CustomRoleOption(18, "EvilLighterName", RoleClass.ImpostorRed, 1);
            EvilLighterPlayerCount = CustomOption.Create(19, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilLighterOption);
            EvilLighterCoolTime = CustomOption.Create(20, ModTranslation.getString("EvilLigtherCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, EvilLighterOption, format: "unitSeconds");
            EvilLighterDurationTime = CustomOption.Create(21, ModTranslation.getString("EvilLigtherDurationSetting"), 10f, 1f, 20f, 0.5f, EvilLighterOption, format: "unitSeconds");
            
            **/
            EvilScientistOption = new CustomRoleOption(22, "EvilScientistName", RoleClass.ImpostorRed, 1);
            EvilScientistPlayerCount = CustomOption.Create(34, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilScientistOption);
            EvilScientistCoolTime = CustomOption.Create(24, ModTranslation.getString("EvilScientistCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, EvilScientistOption, format: "unitSeconds");
            EvilScientistDurationTime = CustomOption.Create(25, ModTranslation.getString("EvilScientistDurationSetting"), 10f, 2.5f, 20f, 2.5f, EvilScientistOption, format: "unitSeconds");
            
            SheriffOption = new CustomRoleOption(26, "SheriffName", RoleClass.Sheriff.color, 1);
            SheriffPlayerCount = CustomOption.Create(27, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SheriffOption);
            SheriffCoolTime = CustomOption.Create(28, ModTranslation.getString("SheriffCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, SheriffOption, format: "unitSeconds");
            SheriffNeutralKill = CustomOption.Create(173, ModTranslation.getString("SheriffIsKillMadMateSetting"), false, SheriffOption);
            SheriffMadMateKill = CustomOption.Create(29, ModTranslation.getString("SheriffIsKillNeutralSetting"), false, SheriffOption);
            SheriffKillMaxCount = CustomOption.Create(30, ModTranslation.getString("SheriffMaxKillCountSetting"), 1f, 1f, 20f, 1, SheriffOption, format: "unitSeconds");
            
            MeetingSheriffOption = new CustomRoleOption(31, "MeetingSheriffName", RoleClass.MeetingSheriff.color, 1);
            MeetingSheriffPlayerCount = CustomOption.Create(32, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MeetingSheriffOption);
            MeetingSheriffNeutralKill = CustomOption.Create(174, ModTranslation.getString("MeetingSheriffIsKillNeutralSetting"), false, MeetingSheriffOption);
            MeetingSheriffMadMateKill = CustomOption.Create(33, ModTranslation.getString("MeetingSheriffIsKillMadMateSetting"), false, MeetingSheriffOption);
            MeetingSheriffKillMaxCount = CustomOption.Create(201, ModTranslation.getString("MeetingSheriffMaxKillCountSetting"), 1f, 1f, 20f, 1f, MeetingSheriffOption, format: "unitSeconds");
            MeetingSheriffOneMeetingMultiKill = CustomOption.Create(35, ModTranslation.getString("MeetingSheriffMeetingmultipleKillSetting"), false, MeetingSheriffOption);
            
            JackalOption = new CustomRoleOption(36, "JackalName", RoleClass.Jackal.color, 1);
            JackalPlayerCount = CustomOption.Create(37, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalOption);
            JackalKillCoolDown = CustomOption.Create(38, ModTranslation.getString("JackalCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, JackalOption, format: "unitSeconds");
            JackalUseVent = CustomOption.Create(160, ModTranslation.getString("JackalUseVentSetting"), true, JackalOption);
            JackalUseSabo = CustomOption.Create(161, ModTranslation.getString("JackalUseSaboSetting"), false, JackalOption);
            JackalCreateSidekick = CustomOption.Create(39, ModTranslation.getString("JackalCreateSidekickSetting"), false, JackalOption);
            JackalNewJackalCreateSidekick = CustomOption.Create(40, ModTranslation.getString("JackalNewJackalCreateSidekickSetting"), false, JackalOption);

            TeleporterOption = new CustomRoleOption(41, "TeleporterName", RoleClass.ImpostorRed, 1);
            TeleporterPlayerCount = CustomOption.Create(42, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TeleporterOption);
            TeleporterCoolTime = CustomOption.Create(43, ModTranslation.getString("TeleporterCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, TeleporterOption, format: "unitSeconds");
            TeleporterDurationTime = CustomOption.Create(44, ModTranslation.getString("TeleporterTeleportTimeSetting"), 10f, 1f, 20f, 0.5f, TeleporterOption, format: "unitSeconds");
            
            SpiritMediumOption = new CustomRoleOption(45, "SpiritMediumName", RoleClass.SpiritMedium.color, 1);
            SpiritMediumPlayerCount = CustomOption.Create(46, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpiritMediumOption);
            SpiritMediumDisplayMode = CustomOption.Create(47, ModTranslation.getString("SpiritMediumDisplaySetting"), false, SpiritMediumOption);
            SpiritMediumMaxCount = CustomOption.Create(48, cs(Color.white, "SpiritMediumMaxCountSetting"), 2f,1f,15f,1f, SpiritMediumOption);
            
            SpeedBoosterOption = new CustomRoleOption(49, "SpeedBoosterName", RoleClass.SpeedBooster.color, 1);
            SpeedBoosterPlayerCount = CustomOption.Create(50, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeedBoosterOption);
            SpeedBoosterCoolTime = CustomOption.Create(51, ModTranslation.getString("SpeedBoosterCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
            SpeedBoosterDurationTime = CustomOption.Create(52, ModTranslation.getString("SpeedBoosterDurationSetting"), 15f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
            SpeedBoosterSpeed = CustomOption.Create(53, ModTranslation.getString("SpeedBoosterPlusSpeedSetting"), 0.5f, 0.0f, 5f, 0.25f, SpeedBoosterOption, format: "unitSeconds");

            EvilSpeedBoosterOption = new CustomRoleOption(54, "EvilSpeedBoosterName", RoleClass.ImpostorRed, 1);
            EvilSpeedBoosterPlayerCount = CustomOption.Create(55, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EvilSpeedBoosterOption);
            EvilSpeedBoosterCoolTime = CustomOption.Create(56, ModTranslation.getString("EvilSpeedBoosterCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterDurationTime = CustomOption.Create(57, ModTranslation.getString("EvilSpeedBoosterDurationSetting"), 15f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterSpeed = CustomOption.Create(58, ModTranslation.getString("EvilSpeedBoosterPlusSpeedSetting"), 0.5f, 0.0f, 5f, 0.25f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterIsNotSpeedBooster = CustomOption.Create(126, ModTranslation.getString("EvilSpeedBoosterIsNotSpeedBooster"), false, EvilSpeedBoosterOption);
            /**
            TaskerOption = new CustomRoleOption(59, "TaskerName", RoleClass.ImpostorRed, 1);
            TaskerPlayerCount = CustomOption.Create(60, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TaskerOption);
            TaskerAmount = CustomOption.Create(61, ModTranslation.getString("TaskerTaskSetting"), 30f, 2.5f, 60f, 2.5f, TaskerOption);
            TaskerIsKill = CustomOption.Create(62, ModTranslation.getString("TaskerIsKillSetting"), false, TaskerOption);
            **/
            DoorrOption = new CustomRoleOption(63, "DoorrName", RoleClass.Doorr.color, 1);
            DoorrPlayerCount = CustomOption.Create(64, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoorrOption);
            DoorrCoolTime = CustomOption.Create(65, ModTranslation.getString("DoorrCoolTimeSetting"), 30f, 2.5f, 60f, 2.5f, DoorrOption);

            EvilDoorrOption = new CustomRoleOption(66, "EvilDoorrName", RoleClass.ImpostorRed, 1);
            EvilDoorrPlayerCount = CustomOption.Create(67, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilDoorrOption);
            EvilDoorrCoolTime = CustomOption.Create(68, ModTranslation.getString("EvilDoorrCoolTimeSetting"), 30f, 2.5f, 60f, 2.5f, EvilDoorrOption);
            /**
            SealdorOption = new CustomRoleOption(69, "SealdorName", RoleClass.Sealdor.color, 1);
            SealdorPlayerCount = CustomOption.Create(70, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SealdorOption);
            SealdorCoolTime = CustomOption.Create(71, ModTranslation.getString("SealdorCoolTimeSetting"), 1f, 1f, 7f, 1f, SealdorOption, format: "unitCouples");
            SealdorDurationTime = CustomOption.Create(72, ModTranslation.getString("SealdorDurationSetting"), 1f, 1f, 7f, 1f, SealdorOption, format: "unitCouples");
            
            FreezerOption = new CustomRoleOption(73, "FreezerName", RoleClass.ImpostorRed, 1);
            FreezerPlayerCount = CustomOption.Create(74, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FreezerOption);
            FreezerCoolTime = CustomOption.Create(75, ModTranslation.getString("FreezerCoolTimeSetting"), 1f, 1f, 7f, 1f, FreezerOption, format: "unitCouples");
            FreezerDurationTime = CustomOption.Create(76, ModTranslation.getString("FreezerDurationSetting"), 1f, 1f, 7f, 1f, FreezerOption, format: "unitCouples");

            SpeederOption = new CustomRoleOption(77, "SpeederName", RoleClass.ImpostorRed, 1);
            SpeederPlayerCount = CustomOption.Create(78, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeederOption);
            SpeederCoolTime = CustomOption.Create(79, ModTranslation.getString("SpeederCoolTimeSetting"), 1f, 1f, 7f, 1f, SpeederOption, format: "unitCouples");
            SpeederDurationTime = CustomOption.Create(80, ModTranslation.getString("SpeederDurationTimeSetting"), 1f, 1f, 7f, 1f, SpeederOption, format: "unitCouples");

            GuesserOption = new CustomRoleOption(81, "GuesserName", RoleClass.Guesser.color, 1);
            GuesserPlayerCount = CustomOption.Create(82, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GuesserOption);
            GuesserShortMaxCount = CustomOption.Create(83, ModTranslation.getString("GuesserShortMaxCountSetting"), 30f, 2.5f, 60f, 2.5f, GuesserOption);
            GuesserShortOneMeetingCount = CustomOption.Create(84, cs(Color.white, "GuesserOneMeetingShortSetting"), GuesserCount, GuesserOption);

            EvilGuesserOption = new CustomRoleOption(85, "EvilGuesserName", RoleClass.ImpostorRed, 1);
            EvilGuesserPlayerCount = CustomOption.Create(86, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGuesserOption);
            EvilGuesserShortMaxCount = CustomOption.Create(87, ModTranslation.getString("EvilGuesserShortMaxCountSetting"), 30f, 2.5f, 60f, 2.5f, EvilGuesserOption);
            EvilGuesserShortOneMeetingCount = CustomOption.Create(88, cs(Color.white, "EvilGuesserOneMeetingShortSetting"), GuesserCount, EvilGuesserOption);

            VultureOption = new CustomRoleOption(89, "VultureName", RoleClass.Vulture.color, 1);
            VulturePlayerCount = CustomOption.Create(90, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], VultureOption);
            VultureCoolDown = CustomOption.Create(91, ModTranslation.getString("VultureCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, VultureOption, format: "unitSeconds");
            VultureDeadBodyMaxCount = CustomOption.Create(92, cs(Color.white, "VultureDeadBodyCountSetting"), VultureDeadBodyCount, VultureOption);
            **/
            NiceScientistOption = new CustomRoleOption(202, "NiceScientistName", RoleClass.NiceScientist.color, 1);
            NiceScientistPlayerCount = CustomOption.Create(102, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceScientistOption);
            NiceScientistCoolTime = CustomOption.Create(103, ModTranslation.getString("NiceScientistCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, NiceScientistOption, format: "unitSeconds");
            NiceScientistDurationTime = CustomOption.Create(203, ModTranslation.getString("NiceScientistDurationSetting"), 10f, 2.5f, 20f, 2.5f, NiceScientistOption, format: "unitSeconds");
            
            ClergymanOption = new CustomRoleOption(93, "ClergymanName", RoleClass.Clergyman.color, 1);
            ClergymanPlayerCount = CustomOption.Create(94, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ClergymanOption);
            ClergymanCoolTime = CustomOption.Create(95, ModTranslation.getString("ClergymanCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, ClergymanOption, format: "unitSeconds");
            ClergymanDurationTime = CustomOption.Create(96, ModTranslation.getString("ClergymanDurationTimeSetting"), 10f, 1f, 20f, 0.5f, ClergymanOption, format: "unitSeconds");
            ClergymanDownVision = CustomOption.Create(97, ModTranslation.getString("ClergymanDownVisionSetting"), 0.25f, 0f, 5f, 0.25f, ClergymanOption);

            MadMateOption = new CustomRoleOption(98, "MadMateName", RoleClass.ImpostorRed, 1);
            MadMatePlayerCount = CustomOption.Create(99, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMateOption);
            MadMateIsCheckImpostor = CustomOption.Create(100, ModTranslation.getString("MadMateIsCheckImpostorSetting"), false, MadMateOption);
            MadMateCheckImpostorTask = CustomOption.Create(242, ModTranslation.getString("MadMateCheckImpostorTaskSetting"), rates4, MadMateIsCheckImpostor);
            MadMateIsUseVent = CustomOption.Create(120, ModTranslation.getString("MadMateUseVentSetting"), false, MadMateOption);
            MadMateIsImpostorLight = CustomOption.Create(234, ModTranslation.getString("MadMateImpostorLightSetting"), false, MadMateOption);

            BaitOption = new CustomRoleOption(104, "BaitName", RoleClass.Bait.color,1);
            BaitPlayerCount = CustomOption.Create(105, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BaitOption);
            BaitReportTime = CustomOption.Create(114, ModTranslation.getString("BaitReportTimeSetting"), 2f,1f,4f,0.5f,BaitOption);

            HomeSecurityGuardOption = new CustomRoleOption(106, "HomeSecurityGuardName", RoleClass.HomeSecurityGuard.color, 1);
            HomeSecurityGuardPlayerCount = CustomOption.Create(107, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HomeSecurityGuardOption);

            StuntManOption = new CustomRoleOption(108, "StuntManName", RoleClass.StuntMan.color, 1);
            StuntManPlayerCount = CustomOption.Create(109, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StuntManOption);
            StuntManMaxGuardCount = CustomOption.Create(119, ModTranslation.getString("StuntManGuardMaxCountSetting"), 1f, 1f,15f,1f,StuntManOption);

            MovingOption = new CustomRoleOption(110, "MovingName", RoleClass.Moving.color, 1);
            MovingPlayerCount = CustomOption.Create(111, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MovingOption);
            MovingCoolTime = CustomOption.Create(112, cs(Color.white, "MovingCoolDownSetting"),30f , 0f,60f, 2.5f, MovingOption);

            OpportunistOption = new CustomRoleOption(127, "OpportunistName", RoleClass.Opportunist.color, 1);
            OpportunistPlayerCount = CustomOption.Create(125, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], OpportunistOption);

            /**
            NiceGamblerOption = new CustomRoleOption(140, "NiceGamblerName", RoleClass.NiceGambler.color, 1);
            NiceGamblerPlayerCount = CustomOption.Create(134, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceGamblerOption);
            NiceGamblerUseCount = CustomOption.Create(139, cs(Color.white, "NiceGamblerUseCountSetting"), 1f, 1f,15f, 1f, NiceGamblerOption);
            **/
            EvilGamblerOption = new CustomRoleOption(141, "EvilGamblerName", RoleClass.EvilGambler.color, 1);
            EvilGamblerPlayerCount = CustomOption.Create(135, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGamblerOption);
            EvilGamblerSucTime = CustomOption.Create(136, cs(Color.white, "EvilGamblerSucTimeSetting"), 15f, 0f, 60f, 2.5f, EvilGamblerOption);
            EvilGamblerNotSucTime = CustomOption.Create(137, cs(Color.white, "EvilGamblerNotSucTimeSetting"), 15f, 0f, 60f, 2.5f, EvilGamblerOption);
            EvilGamblerSucpar = CustomOption.Create(138, cs(Color.white, "EvilGamblerSucParSetting"), rates , EvilGamblerOption);

            BestfalsechargeOption = new CustomRoleOption(142, "BestfalsechargeName", RoleClass.Bestfalsecharge.color, 1);
            BestfalsechargePlayerCount = CustomOption.Create(143, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BestfalsechargeOption);
            /*
            ResearcherOption = new CustomRoleOption(144, "ResearcherName", RoleClass.Researcher.color, 1);
            ResearcherPlayerCount = CustomOption.Create(145, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ResearcherOption);
            **/
            SelfBomberOption = new CustomRoleOption(146, "SelfBomberName", RoleClass.SelfBomber.color, 1);
            SelfBomberPlayerCount = CustomOption.Create(147, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SelfBomberOption);
            SelfBomberScope = CustomOption.Create(148, cs(Color.white, "SelfBomberScopeSetting"),1f,0.5f,3f,0.5f,SelfBomberOption);

            GodOption = new CustomRoleOption(149, "GodName", RoleClass.God.color, 1);
            GodPlayerCount = CustomOption.Create(150, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GodOption);
            GodViewVote = CustomOption.Create(236, cs(Color.white, "GodViewVoteSetting"), false, GodOption);
            GodIsEndTaskWin = CustomOption.Create(237, cs(Color.white, "GodIsEndTaskWinSetting"), true, GodOption);
            /*
            AllCleanerOption = new CustomRoleOption(151, "AllCleanerName", RoleClass.AllCleaner.color, 1);
            AllCleanerPlayerCount = CustomOption.Create(152, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], AllCleanerOption);
            AllCleanerCount = CustomOption.Create(153, cs(Color.white, "AllCleanerCountSetting"), 1f, 1f, 15f, 1f, AllCleanerOption);
            */
            NiceNekomataOption = new CustomRoleOption(154, "NiceNekomataName", RoleClass.NiceNekomata.color, 1);
            NiceNekomataPlayerCount = CustomOption.Create(155, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceNekomataOption);
            NiceNekomataIsChain = CustomOption.Create(156, ModTranslation.getString("NiceNekomataIsChainSetting"), true, NiceNekomataOption);

            EvilNekomataOption = new CustomRoleOption(157, "EvilNekomataName", RoleClass.EvilNekomata.color, 1);
            EvilNekomataPlayerCount = CustomOption.Create(158, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilNekomataOption);

            JackalFriendsOption = new CustomRoleOption(162, "JackalFriendsName", RoleClass.JackalFriends.color, 1);
            JackalFriendsPlayerCount = CustomOption.Create(163, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalFriendsOption);
            JackalFriendsIsCheckJackal = CustomOption.Create(164, ModTranslation.getString("JackalFriendsIsCheckJackalSetting"), false, JackalFriendsOption);
            JackalFriendsIsUseVent = CustomOption.Create(165, ModTranslation.getString("MadMateUseVentSetting"), false, JackalFriendsOption);
            
            DoctorOption = new CustomRoleOption(166, "DoctorName", RoleClass.Doctor.color, 1);
            DoctorPlayerCount = CustomOption.Create(167, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoctorOption);

            CountChangerOption = new CustomRoleOption(169, "CountChangerName", RoleClass.CountChanger.color, 1);
            CountChangerPlayerCount = CustomOption.Create(170, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CountChangerOption);
            CountChangerMaxCount = CustomOption.Create(171, cs(Color.white, "CountChangerMaxCountSetting"), 1f,1f,15f,1f, CountChangerOption);
            CountChangerNextTurn = CustomOption.Create(172, cs(Color.white, "CountChangerNextTurnSetting"), false, CountChangerOption);

            PursuerOption = new CustomRoleOption(175, "PursuerName", RoleClass.Pursuer.color, 1);
            PursuerPlayerCount = CustomOption.Create(176, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PursuerOption);

            MinimalistOption = new CustomRoleOption(177, "MinimalistName", RoleClass.Minimalist.color, 1);
            MinimalistPlayerCount = CustomOption.Create(178, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MinimalistOption);
            MinimalistKillCoolTime = CustomOption.Create(179, cs(Color.white, "MinimalistKillCoolSetting"), 20f, 2.5f, 60f, 2.5f, MinimalistOption);
            MinimalistVent = CustomOption.Create(180, cs(Color.white, "MinimalistVentSetting"), false, MinimalistOption);
            MinimalistSabo = CustomOption.Create(181, cs(Color.white, "MinimalistSaboSetting"), false, MinimalistOption);
            MinimalistReport = CustomOption.Create(182, cs(Color.white, "MinimalistReportSetting"), true, MinimalistOption);

            HawkOption = new CustomRoleOption(183, "HawkName", RoleClass.Hawk.color, 1);
            HawkPlayerCount = CustomOption.Create(184, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], HawkOption);
            HawkCoolTime = CustomOption.Create(185, ModTranslation.getString("HawkCoolTimeSetting"), 15f, 1f, 7f, 1f, HawkOption, format: "unitCouples");
            HawkDurationTime = CustomOption.Create(186, ModTranslation.getString("HawkDurationTimeSetting"), 1f, 1f, 7f, 1f, HawkOption, format: "unitCouples");

            EgoistOption = new CustomRoleOption(187, "EgoistName", RoleClass.Egoist.color, 1);
            EgoistPlayerCount = CustomOption.Create(188, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EgoistOption);
            EgoistUseVent = CustomOption.Create(189, ModTranslation.getString("EgoistUseVentSetting"), false, EgoistOption);
            EgoistUseSabo = CustomOption.Create(190, ModTranslation.getString("EgoistUseSaboSetting"), false, EgoistOption);
            EgoistImpostorLight = CustomOption.Create(191, cs(Color.white, "EgoistImpostorLightSetting"), false, EgoistOption);

            NiceRedRidingHoodOption = new CustomRoleOption(192, "NiceRedRidingHoodName", RoleClass.NiceRedRidingHood.color, 1);
            NiceRedRidingHoodPlayerCount = CustomOption.Create(193, ModTranslation.getString("SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceRedRidingHoodOption);
            NiceRedRidingHoodCount = CustomOption.Create(194, ModTranslation.getString("NiceRedRidingHoodCount"), 1f, 1f, 15f, 1f, NiceRedRidingHoodOption);

            EvilEraserOption = new CustomRoleOption(210, "EvilEraserName", RoleClass.EvilEraser.color, 1);
            EvilEraserPlayerCount = CustomOption.Create(211, ModTranslation.getString("SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilEraserOption);
            EvilEraserMaxCount = CustomOption.Create(212, ModTranslation.getString("EvilEraserMaxCountSetting"), 1f, 1f, 15f, 1f, EvilEraserOption);

            WorkpersonOption = new CustomRoleOption(213, "WorkpersonName", RoleClass.Workperson.color, 1);
            WorkpersonPlayerCount = CustomOption.Create(214, ModTranslation.getString("SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WorkpersonOption);
            WorkpersonCommonTask = CustomOption.Create(215, ModTranslation.getString("GameCommonTasks"), 2,0,12,1, WorkpersonOption);
            WorkpersonLongTask = CustomOption.Create(216, ModTranslation.getString("GameLongTasks"), 10,0,69,1, WorkpersonOption);
            WorkpersonShortTask = CustomOption.Create(217, ModTranslation.getString("GameShortTasks"), 5,0,45,1, WorkpersonOption);

            MagazinerOption = new CustomRoleOption(218, "MagazinerName", RoleClass.Magaziner.color, 1);
            MagazinerPlayerCount = CustomOption.Create(219, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MagazinerOption);
            MagazinerSetKillTime = CustomOption.Create(220, cs(Color.white, "MagazinerSetTimeSetting"), 0f, 0f, 60f, 2.5f, MagazinerOption);

            MayorOption = new CustomRoleOption(231, "MayorName", RoleClass.Mayor.color, 1);
            MayorPlayerCount = CustomOption.Create(232, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorOption);
            MayorVoteCount = CustomOption.Create(233, cs(Color.white, "MayorVoteCountSetting"), 2f, 1f, 100f, 1f, MayorOption);

            trueloverOption = new CustomRoleOption(239, "trueloverName", RoleClass.truelover.color, 1);
            trueloverPlayerCount = CustomOption.Create(240, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], trueloverOption);

            TechnicianOption = new CustomRoleOption(244, "TechnicianName", RoleClass.Technician.color, 1);
            TechnicianPlayerCount = CustomOption.Create(245, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TechnicianOption);

            QuarreledOption = CustomOption.Create(122, cs(RoleClass.Quarreled.color, "QuarreledName"), false, null, isHeader: true);
            QuarreledTeamCount = CustomOption.Create(124, cs(Color.white, "QuarreledTeamCountSetting"), QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], QuarreledOption);
            QuarreledOnlyCrewMate = CustomOption.Create(123, cs(Color.white, "QuarreledOnlyCrewMateSetting"), false, QuarreledOption);

            LoversOption = CustomOption.Create(221, cs(RoleClass.Lovers.color, "LoversName"), false, null, isHeader: true);
            LoversTeamCount = CustomOption.Create(222, cs(Color.white, "LoversTeamCountSetting"), QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], LoversOption);
            LoversPar = CustomOption.Create(223, cs(Color.white, "LoversParSetting"), rates , LoversOption);
            LoversOnlyCrewMate = CustomOption.Create(224, cs(Color.white, "LoversOnlyCrewMateSetting"), false, LoversOption);
            LoversSingleTeam = CustomOption.Create(225, cs(Color.white, "LoversSingleTeamSetting"), true, LoversOption);
            LoversSameDie = CustomOption.Create(226, cs(Color.white, "LoversSameDieSetting"), true, LoversOption);
            LoversAliveTaskCount = CustomOption.Create(227, cs(Color.white, "LoversAliveTaskCountSetting"), false, LoversOption);
            LoversDuplicationQuarreled = CustomOption.Create(228, cs(Color.white, "LoversDuplicationQuarreledSetting"), true, LoversOption);
        }
    }
}