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
using SuperNewRoles.Patch;

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

        public static CustomOption enableMirroMap;

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
        public static CustomOption JesterCommonTask;
        public static CustomOption JesterShortTask;
        public static CustomOption JesterLongTask;

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
        public static CustomOption SheriffMadRoleKill;
        public static CustomOption SheriffNeutralKill;
        public static CustomOption SheriffLoversKill;
        public static CustomOption SheriffKillMaxCount;

        public static CustomRoleOption MeetingSheriffOption;
        public static CustomOption MeetingSheriffPlayerCount;
        public static CustomOption MeetingSheriffMadRoleKill;
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
        public static CustomOption MadMateCommonTask;
        public static CustomOption MadMateShortTask;
        public static CustomOption MadMateLongTask;
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
        public static CustomOption GodCommonTask;
        public static CustomOption GodShortTask;
        public static CustomOption GodLongTask;

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
        public static CustomOption EgoistUseKill;

        public static CustomRoleOption NiceRedRidingHoodOption;
        public static CustomOption NiceRedRidingHoodPlayerCount;
        public static CustomOption NiceRedRidingHoodCount;

        public static CustomRoleOption EvilEraserOption;
        public static CustomOption EvilEraserPlayerCount;
        public static CustomOption EvilEraserMaxCount;

        public static CustomRoleOption WorkpersonOption;
        public static CustomOption WorkpersonPlayerCount;
        public static CustomOption WorkpersonIsAliveWin;
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

        public static CustomRoleOption SerialKillerOption;
        public static CustomOption SerialKillerPlayerCount;
        public static CustomOption SerialKillerSuicideTime;
        public static CustomOption SerialKillerKillTime;
        public static CustomOption SerialKillerIsMeetingReset;

        public static CustomRoleOption OverKillerOption;
        public static CustomOption OverKillerPlayerCount;
        public static CustomOption OverKillerKillCoolTime;
        public static CustomOption OverKillerKillCount;

        public static CustomRoleOption LevelingerOption;
        public static CustomOption LevelingerPlayerCount;
        public static CustomOption LevelingerOneKillXP;
        public static CustomOption LevelingerUpLevelXP;
        public static CustomOption LevelingerLevelOneGetPower;
        public static CustomOption LevelingerLevelTwoGetPower;
        public static CustomOption LevelingerLevelThreeGetPower;
        public static CustomOption LevelingerLevelFourGetPower;
        public static CustomOption LevelingerLevelFiveGetPower;
        public static CustomOption LevelingerUseXPRevive;
        public static CustomOption LevelingerReviveXP;

        public static CustomRoleOption EvilMovingOption;
        public static CustomOption EvilMovingPlayerCount;
        public static CustomOption EvilMovingCoolTime;

        public static CustomRoleOption AmnesiacOption;
        public static CustomOption AmnesiacPlayerCount;

        public static CustomRoleOption SideKillerOption;
        public static CustomOption SideKillerPlayerCount;
        public static CustomOption SideKillerKillCoolTime;
        public static CustomOption SideKillerMadKillerKillCoolTime;

        public static CustomRoleOption SurvivorOption;
        public static CustomOption SurvivorPlayerCount;
        public static CustomOption SurvivorKillCoolTime;

        public static CustomRoleOption MadMayorOption;
        public static CustomOption MadMayorPlayerCount;
        public static CustomOption MadMayorIsCheckImpostor;
        public static CustomOption MadMayorCommonTask;
        public static CustomOption MadMayorShortTask;
        public static CustomOption MadMayorLongTask;
        public static CustomOption MadMayorCheckImpostorTask;
        public static CustomOption MadMayorIsUseVent;
        public static CustomOption MadMayorIsImpostorLight;
        public static CustomOption MadMayorVoteCount;

        public static CustomRoleOption NiceHawkOption;
        public static CustomOption NiceHawkPlayerCount;
        public static CustomOption NiceHawkCoolTime;
        public static CustomOption NiceHawkDurationTime;

        public static CustomRoleOption MadStuntManOption;
        public static CustomOption MadStuntManPlayerCount;
        public static CustomOption MadStuntManIsUseVent;
        public static CustomOption MadStuntManIsImpostorLight;
        public static CustomOption MadStuntManIsCheckImpostor;
        public static CustomOption MadStuntManCommonTask;
        public static CustomOption MadStuntManShortTask;
        public static CustomOption MadStuntManLongTask;
        public static CustomOption MadStuntManCheckImpostorTask;
        public static CustomOption MadStuntManMaxGuardCount;

        public static CustomRoleOption MadHawkOption;
        public static CustomOption MadHawkPlayerCount;
        public static CustomOption MadHawkCoolTime;
        public static CustomOption MadHawkDurationTime;
        public static CustomOption MadHawkIsUseVent;
        public static CustomOption MadHawkIsImpostorLight;

        public static CustomOption BakeryOption;
        public static CustomOption BakeryPlayerCount;

        public static CustomRoleOption MadJesterOption;
        public static CustomOption MadJesterPlayerCount;
        public static CustomOption MadJesterIsUseVent;
        public static CustomOption MadJesterIsImpostorLight;
        public static CustomOption IsMadJesterTaskClearWin;

        public static CustomRoleOption FalseChargesOption;
        public static CustomOption FalseChargesPlayerCount;
        public static CustomOption FalseChargesExileTurn;
        public static CustomOption FalseChargesCoolTime;

        public static CustomRoleOption NiceTeleporterOption;
        public static CustomOption NiceTeleporterPlayerCount;
        public static CustomOption NiceTeleporterCoolTime;
        public static CustomOption NiceTeleporterDurationTime;

        public static CustomRoleOption CelebrityOption;
        public static CustomOption CelebrityPlayerCount;
        public static CustomOption CelebrityChangeRoleView;
        public static CustomRoleOption NocturnalityOption;
        public static CustomOption NocturnalityPlayerCount;

        public static CustomRoleOption ObserverOption;
        public static CustomOption ObserverPlayerCount;

        public static CustomRoleOption VampireOption;
        public static CustomOption VampirePlayerCount;
        public static CustomOption VampireKillDelay;

        public static CustomRoleOption DarkKillerOption;
        public static CustomOption DarkKillerPlayerCount;
        public static CustomOption DarkKillerKillCoolTime;

        public static CustomRoleOption SeerOption;
        public static CustomOption SeerPlayerCount;
        public static CustomOption SeerMode;
        public static CustomOption SeerModeBoth;
        public static CustomOption SeerModeFlash;
        public static CustomOption SeerModeSouls;
        public static CustomOption SeerLimitSoulDuration;
        public static CustomOption SeerSoulDuration;

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
        public static CustomOption LoversCommonTask;
        public static CustomOption LoversLongTask;
        public static CustomOption LoversShortTask;

        private static string[] GuesserCount = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
        public static string[] LevelingerTexts = new string[] { };
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
            var Levedatas = new List<string>(){ "optionOff", "LevelingerSettingKeep", "PursuerName", "TeleporterName", "SidekickName", "SpeedBoosterName", "MovingName" };
            var LeveTransed = new List<string>();
            foreach (string data in Levedatas)
            {
                LeveTransed.Add(ModTranslation.getString(data));
            }
            LevelingerTexts = LeveTransed.ToArray();
            presetSelection = CustomOption.Create(0, true, CustomOptionType.Generic, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingpresetSelection"), presets, null, true);

            specialOptions = new CustomOptionBlank(null);
            hideSettings = CustomOption.Create(2, true, CustomOptionType.Generic, cs(Color.white, "SettingsHideSetting"), false, specialOptions);

            crewmateRolesCountMax = CustomOption.Create(3, true, CustomOptionType.Generic, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewRole"), 0f, 0f, 15f, 1f);
            neutralRolesCountMax = CustomOption.Create(4, true, CustomOptionType.Generic, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralRole"), 0f, 0f, 15f, 1f);
            impostorRolesCountMax = CustomOption.Create(5, true, CustomOptionType.Generic, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoRole"), 0f, 0f, 3f, 1f);

            enableMirroMap = CustomOption.Create(338, false, CustomOptionType.Generic, "enableMirroMap", false);

            if (ConfigRoles.DebugMode.Value) {
                IsDebugMode = CustomOption.Create(159, true, CustomOptionType.Generic, "デバッグモード", false, null, isHeader: true);
            }

            DisconnectNotPCOption = CustomOption.Create(168, true, CustomOptionType.Generic, cs(Color.white, "PC以外はキックする"), true,null,isHeader:true);

            MapOptions.MapOption.LoadOption();

            //SoothSayerRate = CustomOption.Create(2, cs(SoothSayer.color,"soothName"),rates, null, true);
            Mode.ModeHandler.OptionLoad();

            Sabotage.Options.Load();

            SoothSayerOption = new CustomRoleOption(6, false, CustomOptionType.Crewmate, "SoothSayerName", RoleClass.SoothSayer.color, 1);
            SoothSayerPlayerCount = CustomOption.Create(7, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
            SoothSayerDisplayMode = CustomOption.Create(8, false, CustomOptionType.Crewmate, "SoothSayerDisplaySetting", false, SoothSayerOption);
            SoothSayerMaxCount = CustomOption.Create(9, false, CustomOptionType.Crewmate, "SoothSayerMaxCountSetting", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);

            JesterOption = new CustomRoleOption(10, true, CustomOptionType.Neutral, "JesterName", RoleClass.Jester.color, 1);
            JesterPlayerCount = CustomOption.Create(11, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JesterOption);
            JesterIsVent = CustomOption.Create(12, true, CustomOptionType.Neutral, "JesterIsVentSetting", false, JesterOption);
            JesterIsSabotage = CustomOption.Create(13, true, CustomOptionType.Neutral, "JesterIsSabotageSetting", false, JesterOption);
            JesterIsWinCleartask = CustomOption.Create(113, true, CustomOptionType.Neutral, "JesterIsWinClearTaskSetting", false, JesterOption);
            var jesteroption = SelectTask.TaskSetting(262, 263, 264, JesterIsWinCleartask, CustomOptionType.Neutral, true);
            JesterCommonTask = jesteroption.Item1;
            JesterShortTask = jesteroption.Item2;
            JesterLongTask = jesteroption.Item3;

            LighterOption = new CustomRoleOption(14, false, CustomOptionType.Crewmate, "LighterName", RoleClass.Lighter.color, 1);
            LighterPlayerCount = CustomOption.Create(15, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LighterOption);
            LighterCoolTime = CustomOption.Create(16, false, CustomOptionType.Crewmate, "LigtherCoolDownSetting", 30f, 2.5f, 60f, 2.5f, LighterOption, format: "unitSeconds");
            LighterDurationTime = CustomOption.Create(17, false, CustomOptionType.Crewmate, "LigtherDurationSetting", 10f, 1f, 20f, 0.5f, LighterOption, format: "unitSeconds");
            LighterUpVision = CustomOption.Create(204, false, CustomOptionType.Crewmate, "LighterUpVisionSetting", 0.25f, 0f, 5f, 0.25f, LighterOption);

            /**
            EvilLighterOption = new CustomRoleOption(18, "EvilLighterName", RoleClass.ImpostorRed, 1);
            EvilLighterPlayerCount = CustomOption.Create(19, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilLighterOption);
            EvilLighterCoolTime = CustomOption.Create(20, ModTranslation.getString("EvilLigtherCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, EvilLighterOption, format: "unitSeconds");
            EvilLighterDurationTime = CustomOption.Create(21, ModTranslation.getString("EvilLigtherDurationSetting"), 10f, 1f, 20f, 0.5f, EvilLighterOption, format: "unitSeconds");
            
            **/
            EvilScientistOption = new CustomRoleOption(22, false, CustomOptionType.Impostor, "EvilScientistName", RoleClass.ImpostorRed, 1);
            EvilScientistPlayerCount = CustomOption.Create(34, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilScientistOption);
            EvilScientistCoolTime = CustomOption.Create(24, false, CustomOptionType.Impostor, "EvilScientistCoolDownSetting", 30f, 2.5f, 60f, 2.5f, EvilScientistOption, format: "unitSeconds");
            EvilScientistDurationTime = CustomOption.Create(25, false, CustomOptionType.Impostor, "EvilScientistDurationSetting", 10f, 2.5f, 20f, 2.5f, EvilScientistOption, format: "unitSeconds");
            
            SheriffOption = new CustomRoleOption(26, true, CustomOptionType.Crewmate, "SheriffName", RoleClass.Sheriff.color, 1);
            SheriffPlayerCount = CustomOption.Create(27, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SheriffOption);
            SheriffCoolTime = CustomOption.Create(28, true, CustomOptionType.Crewmate, ModTranslation.getString("SheriffCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, SheriffOption, format: "unitSeconds");
            SheriffNeutralKill = CustomOption.Create(173, true, CustomOptionType.Crewmate, "SheriffIsKillNewtralSetting", false, SheriffOption);
            SheriffLoversKill = CustomOption.Create(258, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, SheriffOption);
            SheriffMadRoleKill = CustomOption.Create(29, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, SheriffOption);
            SheriffKillMaxCount = CustomOption.Create(30, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, SheriffOption, format: "unitSeconds");
            
            MeetingSheriffOption = new CustomRoleOption(31, false, CustomOptionType.Crewmate, "MeetingSheriffName", RoleClass.MeetingSheriff.color, 1);
            MeetingSheriffPlayerCount = CustomOption.Create(32, false, CustomOptionType.Crewmate, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MeetingSheriffOption);
            MeetingSheriffNeutralKill = CustomOption.Create(174, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillNeutralSetting", false, MeetingSheriffOption);
            MeetingSheriffMadRoleKill = CustomOption.Create(33, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillMadRoleSetting", false, MeetingSheriffOption);
            MeetingSheriffKillMaxCount = CustomOption.Create(201, false, CustomOptionType.Crewmate, "MeetingSheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, MeetingSheriffOption, format: "unitSeconds");
            MeetingSheriffOneMeetingMultiKill = CustomOption.Create(35, false, CustomOptionType.Crewmate, "MeetingSheriffMeetingmultipleKillSetting", false, MeetingSheriffOption);
            
            JackalOption = new CustomRoleOption(36, false, CustomOptionType.Neutral, "JackalName", RoleClass.Jackal.color, 1);
            JackalPlayerCount = CustomOption.Create(37, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalOption);
            JackalKillCoolDown = CustomOption.Create(38, false, CustomOptionType.Neutral, "JackalCoolDownSetting", 30f, 2.5f, 60f, 2.5f, JackalOption, format: "unitSeconds");
            JackalUseVent = CustomOption.Create(160, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalOption);
            JackalUseSabo = CustomOption.Create(161, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalOption);
            JackalCreateSidekick = CustomOption.Create(39, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalOption);
            JackalNewJackalCreateSidekick = CustomOption.Create(40, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalOption);

            TeleporterOption = new CustomRoleOption(41, false, CustomOptionType.Impostor, "TeleporterName", RoleClass.ImpostorRed, 1);
            TeleporterPlayerCount = CustomOption.Create(42, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TeleporterOption);
            TeleporterCoolTime = CustomOption.Create(43, false, CustomOptionType.Impostor, "TeleporterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, TeleporterOption, format: "unitSeconds");
            TeleporterDurationTime = CustomOption.Create(44, false, CustomOptionType.Impostor, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleporterOption, format: "unitSeconds");
            
            SpiritMediumOption = new CustomRoleOption(45, false, CustomOptionType.Crewmate, "SpiritMediumName", RoleClass.SpiritMedium.color, 1);
            SpiritMediumPlayerCount = CustomOption.Create(46, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpiritMediumOption);
            SpiritMediumDisplayMode = CustomOption.Create(47, false, CustomOptionType.Crewmate, "SpiritMediumDisplaySetting", false, SpiritMediumOption);
            SpiritMediumMaxCount = CustomOption.Create(48, false, CustomOptionType.Crewmate, "SpiritMediumMaxCountSetting", 2f,1f,15f,1f, SpiritMediumOption);
            
            SpeedBoosterOption = new CustomRoleOption(49, false, CustomOptionType.Crewmate, "SpeedBoosterName", RoleClass.SpeedBooster.color, 1);
            SpeedBoosterPlayerCount = CustomOption.Create(50, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeedBoosterOption);
            SpeedBoosterCoolTime = CustomOption.Create(51, false, CustomOptionType.Crewmate, "SpeedBoosterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
            SpeedBoosterDurationTime = CustomOption.Create(52, false, CustomOptionType.Crewmate, "SpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
            SpeedBoosterSpeed = CustomOption.Create(53, false, CustomOptionType.Crewmate, "SpeedBoosterPlusSpeedSetting", 0.5f, 0.0f, 5f, 0.25f, SpeedBoosterOption, format: "unitSeconds");

            EvilSpeedBoosterOption = new CustomRoleOption(54, false, CustomOptionType.Impostor, "EvilSpeedBoosterName", RoleClass.ImpostorRed, 1);
            EvilSpeedBoosterPlayerCount = CustomOption.Create(55, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EvilSpeedBoosterOption);
            EvilSpeedBoosterCoolTime = CustomOption.Create(56, false, CustomOptionType.Impostor, "EvilSpeedBoosterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterDurationTime = CustomOption.Create(57, false, CustomOptionType.Impostor, "EvilSpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterSpeed = CustomOption.Create(58, false, CustomOptionType.Impostor, "EvilSpeedBoosterPlusSpeedSetting", 0.5f, 0.0f, 5f, 0.25f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterIsNotSpeedBooster = CustomOption.Create(126, false, CustomOptionType.Impostor, "EvilSpeedBoosterIsNotSpeedBooster", false, EvilSpeedBoosterOption);
            /**
            TaskerOption = new CustomRoleOption(59, "TaskerName", RoleClass.ImpostorRed, 1);
            TaskerPlayerCount = CustomOption.Create(60, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TaskerOption);
            TaskerAmount = CustomOption.Create(61, ModTranslation.getString("TaskerTaskSetting"), 30f, 2.5f, 60f, 2.5f, TaskerOption);
            TaskerIsKill = CustomOption.Create(62, ModTranslation.getString("TaskerIsKillSetting"), false, TaskerOption);
            **/
            DoorrOption = new CustomRoleOption(63, false, CustomOptionType.Crewmate, "DoorrName", RoleClass.Doorr.color, 1);
            DoorrPlayerCount = CustomOption.Create(64, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoorrOption);
            DoorrCoolTime = CustomOption.Create(65, false, CustomOptionType.Crewmate, "DoorrCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoorrOption);

            EvilDoorrOption = new CustomRoleOption(66, false, CustomOptionType.Impostor, "EvilDoorrName", RoleClass.ImpostorRed, 1);
            EvilDoorrPlayerCount = CustomOption.Create(67, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilDoorrOption);
            EvilDoorrCoolTime = CustomOption.Create(68, false, CustomOptionType.Impostor, "EvilDoorrCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, EvilDoorrOption);
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
            NiceScientistOption = new CustomRoleOption(202, false, CustomOptionType.Crewmate, "NiceScientistName", RoleClass.NiceScientist.color, 1);
            NiceScientistPlayerCount = CustomOption.Create(102, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceScientistOption);
            NiceScientistCoolTime = CustomOption.Create(103, false, CustomOptionType.Crewmate, "NiceScientistCoolDownSetting", 30f, 2.5f, 60f, 2.5f, NiceScientistOption, format: "unitSeconds");
            NiceScientistDurationTime = CustomOption.Create(203, false, CustomOptionType.Crewmate, "NiceScientistDurationSetting", 10f, 2.5f, 20f, 2.5f, NiceScientistOption, format: "unitSeconds");
            
            ClergymanOption = new CustomRoleOption(93, false, CustomOptionType.Crewmate, "ClergymanName", RoleClass.Clergyman.color, 1);
            ClergymanPlayerCount = CustomOption.Create(94, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ClergymanOption);
            ClergymanCoolTime = CustomOption.Create(95, false, CustomOptionType.Crewmate, "ClergymanCoolDownSetting", 30f, 2.5f, 60f, 2.5f, ClergymanOption, format: "unitSeconds");
            ClergymanDurationTime = CustomOption.Create(96, false, CustomOptionType.Crewmate, "ClergymanDurationTimeSetting", 10f, 1f, 20f, 0.5f, ClergymanOption, format: "unitSeconds");
            ClergymanDownVision = CustomOption.Create(97, false, CustomOptionType.Crewmate, "ClergymanDownVisionSetting", 0.25f, 0f, 5f, 0.25f, ClergymanOption);

            MadMateOption = new CustomRoleOption(98, true, CustomOptionType.Crewmate, "MadMateName", RoleClass.ImpostorRed, 1);
            MadMatePlayerCount = CustomOption.Create(99, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMateOption);
            MadMateIsCheckImpostor = CustomOption.Create(100, true, CustomOptionType.Crewmate, "MadMateIsCheckImpostorSetting", false, MadMateOption);
            var madmateoption = SelectTask.TaskSetting(259,260,261,MadMateIsCheckImpostor, CustomOptionType.Crewmate, true);
            MadMateCommonTask = madmateoption.Item1;
            MadMateShortTask = madmateoption.Item2;
            MadMateLongTask = madmateoption.Item3;
            //MadMateIsNotTask = madmateoption.Item4;
            MadMateCheckImpostorTask = CustomOption.Create(242, true, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, MadMateIsCheckImpostor);
            MadMateIsUseVent = CustomOption.Create(120, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadMateOption);
            MadMateIsImpostorLight = CustomOption.Create(234, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadMateOption);

            BaitOption = new CustomRoleOption(104, true, CustomOptionType.Crewmate, "BaitName", RoleClass.Bait.color,1);
            BaitPlayerCount = CustomOption.Create(105, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BaitOption);
            BaitReportTime = CustomOption.Create(114, true, CustomOptionType.Crewmate, "BaitReportTimeSetting", 2f,1f,4f,0.5f,BaitOption);

            HomeSecurityGuardOption = new CustomRoleOption(106, true, CustomOptionType.Crewmate, "HomeSecurityGuardName", RoleClass.HomeSecurityGuard.color, 1);
            HomeSecurityGuardPlayerCount = CustomOption.Create(107, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HomeSecurityGuardOption);

            StuntManOption = new CustomRoleOption(108, true, CustomOptionType.Crewmate, "StuntManName", RoleClass.StuntMan.color, 1);
            StuntManPlayerCount = CustomOption.Create(109, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StuntManOption);
            StuntManMaxGuardCount = CustomOption.Create(119, true, CustomOptionType.Crewmate, "StuntManGuardMaxCountSetting", 1f, 1f,15f,1f,StuntManOption);

            MovingOption = new CustomRoleOption(110, false, CustomOptionType.Crewmate, "MovingName", RoleClass.Moving.color, 1);
            MovingPlayerCount = CustomOption.Create(111, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MovingOption);
            MovingCoolTime = CustomOption.Create(112, false, CustomOptionType.Crewmate, "MovingCoolDownSetting", 30f, 0f, 60f, 2.5f, MovingOption);

            OpportunistOption = new CustomRoleOption(127, true, CustomOptionType.Neutral, "OpportunistName", RoleClass.Opportunist.color, 1);
            OpportunistPlayerCount = CustomOption.Create(125, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], OpportunistOption);

            /**
            NiceGamblerOption = new CustomRoleOption(140, "NiceGamblerName", RoleClass.NiceGambler.color, 1);
            NiceGamblerPlayerCount = CustomOption.Create(134, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceGamblerOption);
            NiceGamblerUseCount = CustomOption.Create(139, cs(Color.white, "NiceGamblerUseCountSetting"), 1f, 1f,15f, 1f, NiceGamblerOption);
            **/
            EvilGamblerOption = new CustomRoleOption(141, true, CustomOptionType.Impostor, "EvilGamblerName", RoleClass.EvilGambler.color, 1);
            EvilGamblerPlayerCount = CustomOption.Create(135, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGamblerOption);
            EvilGamblerSucTime = CustomOption.Create(136, true, CustomOptionType.Impostor, "EvilGamblerSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
            EvilGamblerNotSucTime = CustomOption.Create(137, true, CustomOptionType.Impostor, "EvilGamblerNotSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
            EvilGamblerSucpar = CustomOption.Create(138, true, CustomOptionType.Impostor, "EvilGamblerSucParSetting", rates , EvilGamblerOption);

            BestfalsechargeOption = new CustomRoleOption(142, true, CustomOptionType.Crewmate, "BestfalsechargeName", RoleClass.Bestfalsecharge.color, 1);
            BestfalsechargePlayerCount = CustomOption.Create(143, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BestfalsechargeOption);
            /*
            ResearcherOption = new CustomRoleOption(144, "ResearcherName", RoleClass.Researcher.color, 1);
            ResearcherPlayerCount = CustomOption.Create(145, cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ResearcherOption);
            **/
            SelfBomberOption = new CustomRoleOption(146, false, CustomOptionType.Impostor, "SelfBomberName", RoleClass.SelfBomber.color, 1);
            SelfBomberPlayerCount = CustomOption.Create(147, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SelfBomberOption);
            SelfBomberScope = CustomOption.Create(148, false, CustomOptionType.Impostor, "SelfBomberScopeSetting",1f,0.5f,3f,0.5f,SelfBomberOption);

            GodOption = new CustomRoleOption(149, true, CustomOptionType.Neutral, "GodName", RoleClass.God.color, 1);
            GodPlayerCount = CustomOption.Create(150, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GodOption);
            GodViewVote = CustomOption.Create(236, true, CustomOptionType.Neutral, "GodViewVoteSetting", false, GodOption);
            GodIsEndTaskWin = CustomOption.Create(237, true, CustomOptionType.Neutral, "GodIsEndTaskWinSetting", true, GodOption);
            var godoption = SelectTask.TaskSetting(265, 266, 267,GodIsEndTaskWin, CustomOptionType.Neutral, true);
            GodCommonTask = godoption.Item1;
            GodShortTask = godoption.Item2;
            GodLongTask = godoption.Item3;
            /*
            AllCleanerOption = new CustomRoleOption(151, "AllCleanerName", RoleClass.AllCleaner.color, 1);
            AllCleanerPlayerCount = CustomOption.Create(152, cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], AllCleanerOption);
            AllCleanerCount = CustomOption.Create(153, cs(Color.white, "AllCleanerCountSetting"), 1f, 1f, 15f, 1f, AllCleanerOption);
            */
            NiceNekomataOption = new CustomRoleOption(154, true, CustomOptionType.Crewmate, "NiceNekomataName", RoleClass.NiceNekomata.color, 1);
            NiceNekomataPlayerCount = CustomOption.Create(155, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceNekomataOption);
            NiceNekomataIsChain = CustomOption.Create(156, true, CustomOptionType.Crewmate, "NiceNekomataIsChainSetting", true, NiceNekomataOption);

            EvilNekomataOption = new CustomRoleOption(157, true, CustomOptionType.Impostor, "EvilNekomataName", RoleClass.EvilNekomata.color, 1);
            EvilNekomataPlayerCount = CustomOption.Create(158, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilNekomataOption);

            JackalFriendsOption = new CustomRoleOption(162, false, CustomOptionType.Crewmate, "JackalFriendsName", RoleClass.JackalFriends.color, 1);
            JackalFriendsPlayerCount = CustomOption.Create(163, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalFriendsOption);
            JackalFriendsIsCheckJackal = CustomOption.Create(164, false, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, JackalFriendsOption);
            JackalFriendsIsUseVent = CustomOption.Create(165, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, JackalFriendsOption);
            
            DoctorOption = new CustomRoleOption(166, false, CustomOptionType.Crewmate, "DoctorName", RoleClass.Doctor.color, 1);
            DoctorPlayerCount = CustomOption.Create(167, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoctorOption);

            CountChangerOption = new CustomRoleOption(169, false, CustomOptionType.Impostor, "CountChangerName", RoleClass.CountChanger.color, 1);
            CountChangerPlayerCount = CustomOption.Create(170, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CountChangerOption);
            CountChangerMaxCount = CustomOption.Create(171, false, CustomOptionType.Impostor, "CountChangerMaxCountSetting", 1f,1f,15f,1f, CountChangerOption);
            CountChangerNextTurn = CustomOption.Create(172, false, CustomOptionType.Impostor, "CountChangerNextTurnSetting", false, CountChangerOption);

            PursuerOption = new CustomRoleOption(175, false, CustomOptionType.Impostor, "PursuerName", RoleClass.Pursuer.color, 1);
            PursuerPlayerCount = CustomOption.Create(176, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PursuerOption);

            MinimalistOption = new CustomRoleOption(177, true, CustomOptionType.Impostor, "MinimalistName", RoleClass.Minimalist.color, 1);
            MinimalistPlayerCount = CustomOption.Create(178, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MinimalistOption);
            MinimalistKillCoolTime = CustomOption.Create(179, true, CustomOptionType.Impostor, "MinimalistKillCoolSetting", 20f, 2.5f, 60f, 2.5f, MinimalistOption);
            MinimalistVent = CustomOption.Create(180, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, MinimalistOption);
            MinimalistSabo = CustomOption.Create(181, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, MinimalistOption);
            MinimalistReport = CustomOption.Create(182, true, CustomOptionType.Impostor, "MinimalistReportSetting", true, MinimalistOption);

            HawkOption = new CustomRoleOption(183, false, CustomOptionType.Impostor, "HawkName", RoleClass.Hawk.color, 1);
            HawkPlayerCount = CustomOption.Create(184, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], HawkOption);
            HawkCoolTime = CustomOption.Create(185, false, CustomOptionType.Impostor, "HawkCoolTimeSetting", 15f, 1f, 120f, 2.5f, HawkOption, format: "unitCouples");
            HawkDurationTime = CustomOption.Create(186, false, CustomOptionType.Impostor, "HawkDurationTimeSetting", 5f, 1f, 60f, 2.5f, HawkOption, format: "unitCouples");

            EgoistOption = new CustomRoleOption(187, true, CustomOptionType.Neutral, "EgoistName", RoleClass.Egoist.color, 1);
            EgoistPlayerCount = CustomOption.Create(188, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EgoistOption);
            EgoistUseVent = CustomOption.Create(189, true, CustomOptionType.Neutral, "EgoistUseVentSetting", false, EgoistOption);
            EgoistUseSabo = CustomOption.Create(190, true, CustomOptionType.Neutral, "EgoistUseSaboSetting", false, EgoistOption);
            EgoistImpostorLight = CustomOption.Create(191, true, CustomOptionType.Neutral, "EgoistImpostorLightSetting", false, EgoistOption);
            EgoistUseKill = CustomOption.Create(288, true, CustomOptionType.Neutral, "EgoistUseKillSetting", false, EgoistOption);

            NiceRedRidingHoodOption = new CustomRoleOption(192, false, CustomOptionType.Crewmate, "NiceRedRidingHoodName", RoleClass.NiceRedRidingHood.color, 1);
            NiceRedRidingHoodPlayerCount = CustomOption.Create(193, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceRedRidingHoodOption);
            NiceRedRidingHoodCount = CustomOption.Create(194, false, CustomOptionType.Crewmate, "NiceRedRidingHoodCount", 1f, 1f, 15f, 1f, NiceRedRidingHoodOption);

            EvilEraserOption = new CustomRoleOption(210, false, CustomOptionType.Impostor, "EvilEraserName", RoleClass.EvilEraser.color, 1);
            EvilEraserPlayerCount = CustomOption.Create(211, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilEraserOption);
            EvilEraserMaxCount = CustomOption.Create(212, false, CustomOptionType.Impostor, "EvilEraserMaxCountSetting", 1f, 1f, 15f, 1f, EvilEraserOption);

            WorkpersonOption = new CustomRoleOption(213, true, CustomOptionType.Neutral, "WorkpersonName", RoleClass.Workperson.color, 1);
            WorkpersonPlayerCount = CustomOption.Create(214, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WorkpersonOption);
            WorkpersonIsAliveWin = CustomOption.Create(271, true, CustomOptionType.Neutral, "WorkpersonIsAliveWinSetting", false, WorkpersonOption);
            WorkpersonCommonTask = CustomOption.Create(215, true, CustomOptionType.Neutral, "GameCommonTasks", 2,0,12,1, WorkpersonOption);
            WorkpersonLongTask = CustomOption.Create(216, true, CustomOptionType.Neutral, "GameLongTasks", 10,0,69,1, WorkpersonOption);
            WorkpersonShortTask = CustomOption.Create(217, true, CustomOptionType.Neutral, "GameShortTasks", 5,0,45,1, WorkpersonOption);

            MagazinerOption = new CustomRoleOption(218, false, CustomOptionType.Impostor, "MagazinerName", RoleClass.Magaziner.color, 1);
            MagazinerPlayerCount = CustomOption.Create(219, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MagazinerOption);
            MagazinerSetKillTime = CustomOption.Create(220, false, CustomOptionType.Impostor, "MagazinerSetTimeSetting", 0f, 0f, 60f, 2.5f, MagazinerOption);

            MayorOption = new CustomRoleOption(231, true, CustomOptionType.Crewmate, "MayorName", RoleClass.Mayor.color, 1);
            MayorPlayerCount = CustomOption.Create(232, true, CustomOptionType.Crewmate,"SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorOption);
            MayorVoteCount = CustomOption.Create(233, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorOption);

            trueloverOption = new CustomRoleOption(239, true, CustomOptionType.Neutral, "trueloverName", RoleClass.truelover.color, 1);
            trueloverPlayerCount = CustomOption.Create(240, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], trueloverOption);

            TechnicianOption = new CustomRoleOption(244, true, CustomOptionType.Crewmate, "TechnicianName", RoleClass.Technician.color, 1);
            TechnicianPlayerCount = CustomOption.Create(245, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TechnicianOption);

            SerialKillerOption = new CustomRoleOption(249, true, CustomOptionType.Impostor, "SerialKillerName", RoleClass.SerialKiller.color, 1);
            SerialKillerPlayerCount = CustomOption.Create(250, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SerialKillerOption);
            SerialKillerSuicideTime = CustomOption.Create(251, true, CustomOptionType.Impostor, "SerialKillerSuicideTimeSetting", 60f, 0f, 180f, 2.5f, SerialKillerOption);
            SerialKillerKillTime = CustomOption.Create(252, true, CustomOptionType.Impostor, "SerialKillerKillTimeSetting", 15f,0f,60f,2.5f,SerialKillerOption);
            SerialKillerIsMeetingReset = CustomOption.Create(253, true, CustomOptionType.Impostor, "SerialKillerIsMeetingResetSetting", true, SerialKillerOption);

            OverKillerOption = new CustomRoleOption(254, true, CustomOptionType.Impostor, "OverKillerName", RoleClass.OverKiller.color, 1);
            OverKillerPlayerCount = CustomOption.Create(255, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], OverKillerOption);
            OverKillerKillCoolTime = CustomOption.Create(257, true, CustomOptionType.Impostor, "OverKillerKillCoolTimeSetting", 45f, 0f, 60f, 2.5f, OverKillerOption);
            OverKillerKillCount = CustomOption.Create(256, true, CustomOptionType.Impostor, "OverKillerKillCountSetting", 30f, 1f, 60f, 1f, OverKillerOption);

            LevelingerOption = new CustomRoleOption(272, false, CustomOptionType.Impostor, "LevelingerName", RoleClass.Levelinger.color, 1);
            LevelingerPlayerCount = CustomOption.Create(273, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], LevelingerOption);
            LevelingerOneKillXP = CustomOption.Create(274, false, CustomOptionType.Impostor, "LevelingerOneKillXPSetting", 1f, 0f, 10f, 1f, LevelingerOption);
            LevelingerUpLevelXP = CustomOption.Create(275, false, CustomOptionType.Impostor, "LevelingerUpLevelXPSetting", 2f, 1f, 50f, 1f, LevelingerOption);
            LevelingerLevelOneGetPower = CustomOption.Create(276, false, CustomOptionType.Impostor, "1" + ModTranslation.getString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelTwoGetPower = CustomOption.Create(277, false, CustomOptionType.Impostor, "2" + ModTranslation.getString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelThreeGetPower = CustomOption.Create(278, false, CustomOptionType.Impostor, "3" + ModTranslation.getString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelFourGetPower = CustomOption.Create(279, false, CustomOptionType.Impostor, "4" + ModTranslation.getString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelFiveGetPower = CustomOption.Create(280, false, CustomOptionType.Impostor, "5" +ModTranslation.getString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerReviveXP = CustomOption.Create(281, false, CustomOptionType.Impostor, "LevelingerReviveXPSetting", false, LevelingerOption);
            LevelingerUseXPRevive = CustomOption.Create(282, false, CustomOptionType.Impostor, "LevelingerUseXPReviveSetting", 5f,0f,20f,1f, LevelingerReviveXP);

            EvilMovingOption = new CustomRoleOption(283, false, CustomOptionType.Impostor, "EvilMovingName", RoleClass.EvilMoving.color, 1);
            EvilMovingPlayerCount = CustomOption.Create(284, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilMovingOption);
            EvilMovingCoolTime = CustomOption.Create(285, false, CustomOptionType.Impostor, "MovingCoolDownSetting", 30f, 0f, 60f, 2.5f, EvilMovingOption);

            AmnesiacOption = new CustomRoleOption(286, false, CustomOptionType.Neutral, "AmnesiacName", RoleClass.Amnesiac.color, 1);
            AmnesiacPlayerCount = CustomOption.Create(287, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AmnesiacOption);

            SideKillerOption = new CustomRoleOption(289, false, CustomOptionType.Impostor, "SideKillerName", RoleClass.SideKiller.color, 1);
            SideKillerPlayerCount = CustomOption.Create(290, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SideKillerOption);
            SideKillerKillCoolTime = CustomOption.Create(291, false, CustomOptionType.Impostor, "SideKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);
            SideKillerMadKillerKillCoolTime = CustomOption.Create(292, false, CustomOptionType.Impostor, "SideKillerMadKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);

            SurvivorOption = new CustomRoleOption(293, false, CustomOptionType.Impostor, "SurvivorName", RoleClass.Survivor.color, 1);
            SurvivorPlayerCount = CustomOption.Create(294, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SurvivorOption);
            SurvivorKillCoolTime = CustomOption.Create(295, false, CustomOptionType.Impostor, "SurvivorKillCoolTimeSetting", 15f, 0f, 75f, 2.5f, SurvivorOption);

            MadMayorOption = new CustomRoleOption(301, true, CustomOptionType.Crewmate, "MadMayorName", RoleClass.ImpostorRed, 1);
            MadMayorPlayerCount = CustomOption.Create(302, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMayorOption);
            MadMayorVoteCount = CustomOption.Create(303, true, CustomOptionType.Crewmate, "MadMayorVoteCountSetting", 2f, 1f, 100f, 1f, MadMayorOption);
            MadMayorIsCheckImpostor = CustomOption.Create(304, true, CustomOptionType.Crewmate, "MadMayorIsCheckImpostorSetting", false, MadMayorOption);
            var madmayoroption = SelectTask.TaskSetting(305, 306, 307, MadMayorIsCheckImpostor, CustomOptionType.Crewmate, true);
            MadMayorCommonTask = madmayoroption.Item1;
            MadMayorShortTask = madmayoroption.Item2;
            MadMayorLongTask = madmayoroption.Item3;
            MadMayorCheckImpostorTask = CustomOption.Create(308, true, CustomOptionType.Crewmate, "MadMayorCheckImpostorTaskSetting", rates4, MadMayorIsCheckImpostor);
            MadMayorIsUseVent = CustomOption.Create(309, true, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadMayorOption);
            MadMayorIsImpostorLight = CustomOption.Create(310, true, CustomOptionType.Crewmate, "MadMayorImpostorLightSetting", false, MadMayorOption);

            NiceHawkOption = new CustomRoleOption(311, false, CustomOptionType.Crewmate, "NiceHawkName", RoleClass.NiceHawk.color, 1);
            NiceHawkPlayerCount = CustomOption.Create(312, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceHawkOption);
            NiceHawkCoolTime = CustomOption.Create(313, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 1f, 120f, 2.5f, NiceHawkOption, format: "unitCouples");
            NiceHawkDurationTime = CustomOption.Create(314, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 2.5f, NiceHawkOption, format: "unitCouples");

            MadStuntManOption = new CustomRoleOption(315, false, CustomOptionType.Crewmate, "MadStuntManName", RoleClass.ImpostorRed, 1);
            MadStuntManPlayerCount = CustomOption.Create(316, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadStuntManOption);
            MadStuntManIsUseVent = CustomOption.Create(317, false, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadStuntManOption);
            MadStuntManIsImpostorLight = CustomOption.Create(318, false, CustomOptionType.Crewmate, "MadStuntManImpostorLightSetting", false, MadStuntManOption);

            MadHawkOption = new CustomRoleOption(319, false, CustomOptionType.Crewmate, "MadHawkName", RoleClass.ImpostorRed, 1);
            MadHawkPlayerCount = CustomOption.Create(320, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadHawkOption);
            MadHawkCoolTime = CustomOption.Create(321, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 1f, 120f, 2.5f, MadHawkOption, format: "unitCouples");
            MadHawkDurationTime = CustomOption.Create(322, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 2.5f, MadHawkOption, format: "unitCouples");
            MadHawkIsUseVent = CustomOption.Create(323, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadHawkOption);
            MadHawkIsImpostorLight = CustomOption.Create(324, false, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadHawkOption);

            BakeryOption = new CustomRoleOption(325, false, CustomOptionType.Crewmate, "BakeryName", RoleClass.Bakery.color, 1);
            BakeryPlayerCount = CustomOption.Create(328, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BakeryOption);

            MadJesterOption = new CustomRoleOption(329, true, CustomOptionType.Crewmate,"MadJesterName", RoleClass.MadJester.color, 1);
            MadJesterPlayerCount = CustomOption.Create(330, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadJesterOption);
            MadJesterIsUseVent = CustomOption.Create(331, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadJesterOption);
            MadJesterIsImpostorLight = CustomOption.Create(326, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadJesterOption);
            IsMadJesterTaskClearWin = CustomOption.Create(327, true, CustomOptionType.Crewmate, "JesterIsWinClearTaskSetting", false, MadJesterOption);

            FalseChargesOption = new CustomRoleOption(339, true, CustomOptionType.Neutral, "FalseChargesName", RoleClass.FalseCharges.color, 1);
            FalseChargesPlayerCount = CustomOption.Create(340, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FalseChargesOption);
            FalseChargesExileTurn = CustomOption.Create(341, true, CustomOptionType.Neutral, "FalseChargesExileTurn", 2f,1f,10f,1f, FalseChargesOption);
            FalseChargesCoolTime = CustomOption.Create(342, true, CustomOptionType.Neutral, "FalseChargesCoolTime", 15f, 0f, 75f, 2.5f, FalseChargesOption);

            NiceTeleporterOption = new CustomRoleOption(343, false, CustomOptionType.Crewmate, "NiceTeleporterName", RoleClass.NiceTeleporter.color, 1);
            NiceTeleporterPlayerCount = CustomOption.Create(344, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceTeleporterOption);
            NiceTeleporterCoolTime = CustomOption.Create(345, false, CustomOptionType.Crewmate, "NiceTeleporterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, NiceTeleporterOption, format: "unitSeconds");
            NiceTeleporterDurationTime = CustomOption.Create(346, false, CustomOptionType.Crewmate, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, NiceTeleporterOption, format: "unitSeconds");

            CelebrityOption = new CustomRoleOption(347, true, CustomOptionType.Crewmate, "CelebrityName", RoleClass.Celebrity.color, 1);
            CelebrityPlayerCount = CustomOption.Create(348, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], CelebrityOption);
            CelebrityChangeRoleView = CustomOption.Create(349, true, CustomOptionType.Crewmate, "CelebrityChangeRoleViewSetting", false, CelebrityOption);

            NocturnalityOption = new CustomRoleOption(350, true, CustomOptionType.Crewmate, "NocturnalityName", RoleClass.Nocturnality.color, 1);
            NocturnalityPlayerCount = CustomOption.Create(351, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NocturnalityOption);

            ObserverOption = new CustomRoleOption(356, true, CustomOptionType.Crewmate, "ObserverName", RoleClass.Observer.color, 1);
            ObserverPlayerCount = CustomOption.Create(357, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ObserverOption);

            VampireOption = new CustomRoleOption(358, false, CustomOptionType.Impostor, "VampireName", RoleClass.Vampire.color, 1);
            VampirePlayerCount = CustomOption.Create(359, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VampireOption);
            VampireKillDelay = CustomOption.Create(360, false, CustomOptionType.Impostor, "VampireKillDelay", 0f, 1f, 60f, 0.5f, VampireOption, format: "unitSeconds");

            DarkKillerOption = new CustomRoleOption(361, false, CustomOptionType.Impostor, "DarkKillerName", RoleClass.DarkKiller.color, 1);
            DarkKillerPlayerCount = CustomOption.Create(362, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DarkKillerOption);
            DarkKillerKillCoolTime = CustomOption.Create(363, false, CustomOptionType.Impostor, "DarkKillerKillCoolSetting", 20f, 2.5f, 60f, 2.5f, DarkKillerOption);

            SeerOption = new CustomRoleOption(364, false, CustomOptionType.Crewmate, "SeerName", RoleClass.Seer.color, 1);
            SeerPlayerCount = CustomOption.Create(365, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerOption);
            SeerMode = CustomOption.Create(366, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerOption);
            SeerLimitSoulDuration = CustomOption.Create(367, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerOption);
            SeerSoulDuration = CustomOption.Create(368, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerLimitSoulDuration, format: "unitCouples");

            QuarreledOption = CustomOption.Create(122, false, CustomOptionType.Neutral, cs(RoleClass.Quarreled.color, "QuarreledName"), false, null, isHeader: true);
            QuarreledTeamCount = CustomOption.Create(124, false, CustomOptionType.Neutral, "QuarreledTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], QuarreledOption);
            QuarreledOnlyCrewMate = CustomOption.Create(123, false, CustomOptionType.Neutral, "QuarreledOnlyCrewMateSetting", false, QuarreledOption);
            
            LoversOption = CustomOption.Create(221, true, CustomOptionType.Neutral, cs(RoleClass.Lovers.color, "LoversName"), false, null, isHeader: true);
            LoversTeamCount = CustomOption.Create(222, true, CustomOptionType.Neutral, "LoversTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], LoversOption);
            LoversPar = CustomOption.Create(223, true, CustomOptionType.Neutral, "LoversParSetting", rates , LoversOption);
            LoversOnlyCrewMate = CustomOption.Create(224, true, CustomOptionType.Neutral, "LoversOnlyCrewMateSetting", false, LoversOption);
            LoversSingleTeam = CustomOption.Create(225, true, CustomOptionType.Neutral, "LoversSingleTeamSetting", true, LoversOption);
            LoversSameDie = CustomOption.Create(226, true, CustomOptionType.Neutral, "LoversSameDieSetting", true, LoversOption);
            LoversAliveTaskCount = CustomOption.Create(227, true, CustomOptionType.Neutral, "LoversAliveTaskCountSetting", false, LoversOption);
            LoversDuplicationQuarreled = CustomOption.Create(228, true, CustomOptionType.Neutral, "LoversDuplicationQuarreledSetting", true, LoversOption);
            var loversoption = SelectTask.TaskSetting(268, 269, 270,LoversOption, CustomOptionType.Neutral, true);
            LoversCommonTask = loversoption.Item1;
            LoversShortTask = loversoption.Item2;
            LoversLongTask = loversoption.Item3;

            SuperNewRolesPlugin.Logger.LogInfo("設定のidのMax:"+CustomOption.Max);
        }
    }
}
