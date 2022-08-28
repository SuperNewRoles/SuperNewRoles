using System.Collections.Generic;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.CustomOption
{
    public class CustomOptions
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

        public static string[] rates4 = new string[] { "0%", "25%", "50%", "75%", "100%" };

        public static string[] presets = new string[] { "preset1", "preset2", "preset3", "preset4", "preset5", "preset6", "preset7", "preset8", "preset9", "preset10" };
        public static CustomOption presetSelection;

        public static CustomOption specialOptions;
        public static CustomOption hideSettings;

        public static CustomOption crewmateRolesCountMax;
        public static CustomOption crewmateGhostRolesCountMax;
        public static CustomOption impostorRolesCountMax;
        public static CustomOption impostorGhostRolesCountMax;
        public static CustomOption neutralRolesCountMax;
        public static CustomOption neutralGhostRolesCountMax;

        public static CustomOption enableMirroMap;
        public static CustomOption enableAgartha;

        public static CustomOption IsDebugMode;
        public static CustomOption DebugModeFastStart;

        public static CustomOption DisconnectNotPCOption;

        public static CustomOption ZoomOption;
        public static CustomOption ClairvoyantZoom;
        public static CustomOption MouseZoom;
        public static CustomOption ZoomCoolTime;
        public static CustomOption ZoomDurationTime;

        public static CustomOption IsAlwaysReduceCooldown;
        public static CustomOption IsAlwaysReduceCooldownExceptInVent;
        public static CustomOption IsAlwaysReduceCooldownExceptOnTask;

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
        public static CustomOption SheriffKillMaxCount;
        public static CustomOption SheriffCanKillImpostor;
        //=============================================
        public static CustomOption SheriffMadRoleKill;
        //シェリフマッドキル
        //=============================================
        public static CustomOption SheriffFriendsRoleKill;
        //シェリフフレンズキル
        //=============================================
        public static CustomOption SheriffNeutralKill;
        //シェリフ第三キル
        //=============================================
        public static CustomOption SheriffLoversKill;
        public static CustomOption SheriffQuarreledKill;


        public static CustomRoleOption RemoteSheriffOption;
        public static CustomOption RemoteSheriffPlayerCount;
        public static CustomOption RemoteSheriffCoolTime;
        public static CustomOption RemoteSheriffMadRoleKill;
        public static CustomOption RemoteSheriffNeutralKill;
        public static CustomOption RemoteSheriffLoversKill;
        public static CustomOption RemoteSheriffKillMaxCount;
        public static CustomOption RemoteSheriffIsKillTeleportSetting;

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
        public static CustomOption JackalIsImpostorLight;
        public static CustomOption JackalCreateFriend;
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

        public static CustomRoleOption ShielderOption;
        public static CustomOption ShielderPlayerCount;
        public static CustomOption ShielderCoolTime;
        public static CustomOption ShielderDurationTime;

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
        public static CustomOption VultureIsUseVent;
        public static CustomOption VultureShowArrows;

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
        public static CustomOption EvilNekomataNotImpostorExiled;

        public static CustomRoleOption JackalFriendsOption;
        public static CustomOption JackalFriendsPlayerCount;
        public static CustomOption JackalFriendsIsCheckJackal;
        public static CustomOption JackalFriendsCommonTask;
        public static CustomOption JackalFriendsShortTask;
        public static CustomOption JackalFriendsLongTask;
        public static CustomOption JackalFriendsCheckJackalTask;
        public static CustomOption JackalFriendsIsUseVent;
        public static CustomOption JackalFriendsIsImpostorLight;

        public static CustomRoleOption DoctorOption;
        public static CustomOption DoctorPlayerCount;
        public static CustomOption DoctorChargeTime;
        public static CustomOption DoctorUseTime;

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
        public static CustomOption MadJesterCommonTask;
        public static CustomOption MadJesterShortTask;
        public static CustomOption MadJesterLongTask;

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

        public static CustomRoleOption FoxOption;
        public static CustomOption FoxPlayerCount;
        public static CustomOption FoxIsUseVent;
        public static CustomOption FoxIsImpostorLight;
        public static CustomOption FoxReport;

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

        public static CustomRoleOption MadSeerOption;
        public static CustomOption MadSeerPlayerCount;
        public static CustomOption MadSeerMode;
        public static CustomOption MadSeerModeBoth;
        public static CustomOption MadSeerModeFlash;
        public static CustomOption MadSeerModeSouls;
        public static CustomOption MadSeerLimitSoulDuration;
        public static CustomOption MadSeerSoulDuration;
        public static CustomOption MadSeerIsCheckImpostor;
        public static CustomOption MadSeerCommonTask;
        public static CustomOption MadSeerShortTask;
        public static CustomOption MadSeerLongTask;
        public static CustomOption MadSeerCheckImpostorTask;
        public static CustomOption MadSeerIsUseVent;
        public static CustomOption MadSeerIsImpostorLight;

        public static CustomRoleOption EvilSeerOption;
        public static CustomOption EvilSeerPlayerCount;
        public static CustomOption EvilSeerMode;
        public static CustomOption EvilSeerModeBoth;
        public static CustomOption EvilSeerModeFlash;
        public static CustomOption EvilSeerModeSouls;
        public static CustomOption EvilSeerLimitSoulDuration;
        public static CustomOption EvilSeerSoulDuration;

        public static CustomRoleOption TeleportingJackalOption;
        public static CustomOption TeleportingJackalPlayerCount;
        public static CustomOption TeleportingJackalKillCoolDown;
        public static CustomOption TeleportingJackalUseVent;
        public static CustomOption TeleportingJackalUseSabo;
        public static CustomOption TeleportingJackalIsImpostorLight;
        public static CustomOption TeleportingJackalCoolTime;
        public static CustomOption TeleportingJackalDurationTime;

        public static CustomRoleOption MadMakerOption;
        public static CustomOption MadMakerPlayerCount;
        public static CustomOption MadMakerIsUseVent;
        public static CustomOption MadMakerIsImpostorLight;

        public static CustomRoleOption DemonOption;
        public static CustomOption DemonPlayerCount;
        public static CustomOption DemonCoolTime;
        public static CustomOption DemonIsUseVent;
        public static CustomOption DemonIsCheckImpostor;
        public static CustomOption DemonIsAliveWin;

        public static CustomRoleOption TaskManagerOption;
        public static CustomOption TaskManagerPlayerCount;
        public static CustomOption TaskManagerCommonTask;
        public static CustomOption TaskManagerShortTask;
        public static CustomOption TaskManagerLongTask;

        public static CustomRoleOption ArsonistOption;
        public static CustomOption ArsonistPlayerCount;
        public static CustomOption ArsonistCoolTime;
        public static CustomOption ArsonistDurationTime;
        public static CustomOption ArsonistIsUseVent;

        public static CustomRoleOption SeerFriendsOption;
        public static CustomOption SeerFriendsPlayerCount;
        public static CustomOption SeerFriendsMode;
        public static CustomOption SeerFriendsModeBoth;
        public static CustomOption SeerFriendsModeFlash;
        public static CustomOption SeerFriendsModeSouls;
        public static CustomOption SeerFriendsLimitSoulDuration;
        public static CustomOption SeerFriendsSoulDuration;
        public static CustomOption SeerFriendsIsCheckJackal;
        public static CustomOption SeerFriendsCommonTask;
        public static CustomOption SeerFriendsShortTask;
        public static CustomOption SeerFriendsLongTask;
        public static CustomOption SeerFriendsCheckJackalTask;
        public static CustomOption SeerFriendsIsUseVent;
        public static CustomOption SeerFriendsIsImpostorLight;

        public static CustomRoleOption PositionSwapperOption;
        public static CustomOption PositionSwapperPlayerCount;
        public static CustomOption PositionSwapperSwapCount;
        public static CustomOption PositionSwapperCoolTime;
        public static CustomOption PositionSwapperDurationTime;

        public static CustomRoleOption TunaOption;
        public static CustomOption TunaPlayerCount;
        public static CustomOption TunaStoppingTime;
        public static CustomOption TunaIsUseVent;
        public static CustomOption TunaIsAddWin;

        public static CustomRoleOption MafiaOption;
        public static CustomOption MafiaPlayerCount;

        public static CustomRoleOption BlackCatOption;
        public static CustomOption BlackCatPlayerCount;
        public static CustomOption BlackCatNotImpostorExiled;
        public static CustomOption BlackCatIsCheckImpostor;
        public static CustomOption BlackCatCommonTask;
        public static CustomOption BlackCatShortTask;
        public static CustomOption BlackCatLongTask;
        public static CustomOption BlackCatCheckImpostorTask;
        public static CustomOption BlackCatIsUseVent;
        public static CustomOption BlackCatIsImpostorLight;

        public static CustomRoleOption JackalSeerOption;
        public static CustomOption JackalSeerPlayerCount;
        public static CustomOption JackalSeerMode;
        public static CustomOption JackalSeerModeBoth;
        public static CustomOption JackalSeerModeFlash;
        public static CustomOption JackalSeerModeSouls;
        public static CustomOption JackalSeerLimitSoulDuration;
        public static CustomOption JackalSeerSoulDuration;
        public static CustomOption JackalSeerKillCoolDown;
        public static CustomOption JackalSeerUseVent;
        public static CustomOption JackalSeerUseSabo;
        public static CustomOption JackalSeerIsImpostorLight;
        public static CustomOption JackalSeerCreateSidekick;
        public static CustomOption JackalSeerNewJackalCreateSidekick;

        public static CustomRoleOption AssassinAndMarineOption;
        public static CustomOption AssassinPlayerCount;
        public static CustomOption MarinePlayerCount;

        public static CustomRoleOption ChiefOption;
        public static CustomOption ChiefPlayerCount;
        public static CustomOption ChiefSheriffCoolTime;
        public static CustomOption ChiefIsNeutralKill;
        public static CustomOption ChiefIsLoversKill;
        public static CustomOption ChiefIsMadRoleKill;
        public static CustomOption ChiefKillLimit;

        public static CustomRoleOption CleanerOption;
        public static CustomOption CleanerPlayerCount;
        public static CustomOption CleanerCoolDown;
        public static CustomOption CleanerKillCoolTime;

        public static CustomRoleOption MadCleanerOption;
        public static CustomOption MadCleanerPlayerCount;
        public static CustomOption MadCleanerCoolDown;
        public static CustomOption MadCleanerIsUseVent;
        public static CustomOption MadCleanerIsImpostorLight;

        public static CustomRoleOption SamuraiOption;
        public static CustomOption SamuraiPlayerCount;
        public static CustomOption SamuraiKillCoolTime;
        public static CustomOption SamuraiSwordCoolTime;
        public static CustomOption SamuraiVent;
        public static CustomOption SamuraiSabo;
        public static CustomOption SamuraiScope;

        public static CustomRoleOption MayorFriendsOption;
        public static CustomOption MayorFriendsPlayerCount;
        public static CustomOption MayorFriendsIsCheckJackal;
        public static CustomOption MayorFriendsCommonTask;
        public static CustomOption MayorFriendsShortTask;
        public static CustomOption MayorFriendsLongTask;
        public static CustomOption MayorFriendsCheckJackalTask;
        public static CustomOption MayorFriendsIsUseVent;
        public static CustomOption MayorFriendsIsImpostorLight;
        public static CustomOption MayorFriendsVoteCount;

        public static CustomRoleOption VentMakerOption;
        public static CustomOption VentMakerPlayerCount;

        public static CustomRoleOption GhostMechanicOption;
        public static CustomOption GhostMechanicPlayerCount;
        public static CustomOption GhostMechanicRepairLimit;

        public static CustomRoleOption HauntedWolfOption;
        public static CustomOption HauntedWolfPlayerCount;

        public static CustomRoleOption EvilHackerOption;
        public static CustomOption EvilHackerPlayerCount;
        public static CustomOption EvilHackerMadmateSetting;

        public static CustomRoleOption SecretlyKillerOption;
        public static CustomOption SecretlyKillerPlayerCount;
        public static CustomOption SecretlyKillerKillCoolTime;
        public static CustomOption SecretlyKillerIsKillCoolTimeChange;
        public static CustomOption SecretlyKillerIsBlackOutKillCharge;
        public static CustomOption SecretlyKillerSecretKillLimit;
        public static CustomOption SecretlyKillerSecretKillCoolTime;

        public static CustomRoleOption SpyOption;
        public static CustomOption SpyPlayerCount;
        public static CustomOption SpyCanUseVent;

        public static CustomRoleOption KunoichiOption;
        public static CustomOption KunoichiPlayerCount;
        public static CustomOption KunoichiCoolTime;
        public static CustomOption KunoichiKillKunai;
        public static CustomOption KunoichiIsHide;
        public static CustomOption KunoichiIsWaitAndPressTheButtonToHide;
        public static CustomOption KunoichiHideTime;
        public static CustomOption KunoichiHideKunai;

        public static CustomRoleOption DoubleKillerOption;
        public static CustomOption DoubleKillerPlayerCount;
        public static CustomOption MainKillCoolTime;
        public static CustomOption SubKillCoolTime;
        public static CustomOption DoubleKillerSabo;
        public static CustomOption DoubleKillerVent;

        public static CustomRoleOption SmasherOption;
        public static CustomOption SmasherPlayerCount;
        public static CustomOption SmasherKillCoolTime;

        public static CustomRoleOption SuicideWisherOption;
        public static CustomOption SuicideWisherPlayerCount;

        public static CustomRoleOption NeetOption;
        public static CustomOption NeetPlayerCount;
        public static CustomOption NeetIsAddWin;

        public static CustomRoleOption FastMakerOption;
        public static CustomOption FastMakerPlayerCount;

        public static CustomRoleOption ToiletFanOption;
        public static CustomOption ToiletFanPlayerCount;
        public static CustomOption ToiletFanCoolTime;

        public static CustomRoleOption SatsumaAndImoOption;
        public static CustomOption SatsumaAndImoPlayerCount;

        public static CustomRoleOption EvilButtonerOption;
        public static CustomOption EvilButtonerPlayerCount;
        public static CustomOption EvilButtonerCoolTime;
        public static CustomOption EvilButtonerCount;

        public static CustomRoleOption NiceButtonerOption;
        public static CustomOption NiceButtonerPlayerCount;
        public static CustomOption NiceButtonerCoolTime;
        public static CustomOption NiceButtonerCount;

        public static CustomRoleOption FinderOption;
        public static CustomOption FinderPlayerCount;
        public static CustomOption FinderCheckMadmateSetting;

        public static CustomRoleOption RevolutionistAndDictatorOption;
        public static CustomOption RevolutionistPlayerCount;
        public static CustomOption DictatorPlayerCount;
        public static CustomOption DictatorVoteCount;
        public static CustomOption DictatorSubstituteExile;
        public static CustomOption DictatorSubstituteExileLimit;
        public static CustomOption RevolutionistCoolTime;
        public static CustomOption RevolutionistTouchTime;
        public static CustomOption RevolutionistAddWin;
        public static CustomOption RevolutionistAddWinIsAlive;

        public static CustomRoleOption SpelunkerOption;
        public static CustomOption SpelunkerPlayerCount;
        public static CustomOption SpelunkerVentDeathChance;
        public static CustomOption SpelunkerLadderDeadChance;
        public static CustomOption SpelunkerIsDeathCommsOrPowerdown;
        public static CustomOption SpelunkerDeathCommsOrPowerdownTime;
        public static CustomOption SpelunkerLiftDeathChance;
        public static CustomOption SpelunkerDoorOpenChance;

        public static CustomRoleOption SuicidalIdeationOption;
        public static CustomOption SuicidalIdeationPlayerCount;
        public static CustomOption SuicidalIdeationWinText;
        public static CustomOption SuicidalIdeationTimeLeft;
        public static CustomOption SuicidalIdeationAddTimeLeft;
        public static CustomOption SuicidalIdeationFallProbability;
        public static CustomOption SuicidalIdeationCommonTask;
        public static CustomOption SuicidalIdeationShortTask;
        public static CustomOption SuicidalIdeationLongTask;

        public static CustomRoleOption HitmanOption;
        public static CustomOption HitmanPlayerCount;
        public static CustomOption HitmanKillCoolTime;
        public static CustomOption HitmanChangeTargetTime;
        public static CustomOption HitmanIsOutMission;
        public static CustomOption HitmanOutMissionLimit;
        public static CustomOption HitmanWinKillCount;
        public static CustomOption HitmanIsArrowView;
        public static CustomOption HitmanArrowUpdateTime;

        public static CustomRoleOption MatryoshkaOption;
        public static CustomOption MatryoshkaPlayerCount;
        public static CustomOption MatryoshkaWearLimit;
        public static CustomOption MatryoshkaAddKillCoolTime;
        public static CustomOption MatryoshkaWearReport;
        public static CustomOption MatryoshkaWearTime;
        public static CustomOption MatryoshkaCoolTime;

        public static CustomRoleOption NunOption;
        public static CustomOption NunPlayerCount;
        public static CustomOption NunCoolTime;

        public static CustomRoleOption PsychometristOption;
        public static CustomOption PsychometristPlayerCount;
        public static CustomOption PsychometristCoolTime;
        public static CustomOption PsychometristReadTime;
        public static CustomOption PsychometristIsCheckDeathTime;
        public static CustomOption PsychometristDeathTimeDeviation;
        public static CustomOption PsychometristIsCheckDeathReason;
        public static CustomOption PsychometristIsCheckFootprints;
        public static CustomOption PsychometristCanCheckFootprintsTime;
        public static CustomOption PsychometristIsReportCheckedDeadBody;

        public static CustomRoleOption SeeThroughPersonOption;
        public static CustomOption SeeThroughPersonPlayerCount;

        public static CustomRoleOption PartTimerOption;
        public static CustomOption PartTimerPlayerCount;
        public static CustomOption PartTimerDeathTurn;
        public static CustomOption PartTimerCoolTime;
        public static CustomOption PartTimerIsCheckTargetRole;

        public static CustomRoleOption PainterOption;
        public static CustomOption PainterPlayerCount;
        public static CustomOption PainterCoolTime;
        public static CustomOption PainterIsTaskCompleteFootprint;
        public static CustomOption PainterIsSabotageRepairFootprint;
        public static CustomOption PainterIsInVentFootprint;
        public static CustomOption PainterIsExitVentFootprint;
        public static CustomOption PainterIsCheckVitalFootprint;
        public static CustomOption PainterIsCheckAdminFootprint;
        public static CustomOption PainterIsDeathFootprint;
        public static CustomOption PainterIsDeathFootprintBig;
        public static CustomOption PainterIsFootprintMeetingDestroy;

        public static CustomRoleOption PhotographerOption;
        public static CustomOption PhotographerPlayerCount;
        public static CustomOption PhotographerCoolTime;
        public static CustomOption PhotographerIsBonus;
        public static CustomOption PhotographerBonusCount;
        public static CustomOption PhotographerBonusCoolTime;
        public static CustomOption PhotographerIsImpostorVision;
        public static CustomOption PhotographerIsNotification;

        public static CustomRoleOption StefinderOption;
        public static CustomOption StefinderPlayerCount;
        public static CustomOption StefinderKillCoolDown;
        public static CustomOption StefinderVent;
        public static CustomOption StefinderSabo;
        public static CustomOption StefinderSoloWin;

        public static CustomRoleOption SluggerOption;
        public static CustomOption SluggerPlayerCount;
        public static CustomOption SluggerChargeTime;
        public static CustomOption SluggerCoolTime;
        public static CustomOption SluggerIsMultiKill;
        //CustomOption

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
        public static CustomOption LadderDead;
        public static CustomOption LadderDeadChance;

        public static string[] LevelingerTexts = new string[] { };
        public static List<float> CrewPlayers = new() { 1f, 1f, 15f, 1f };
        public static List<float> AlonePlayers = new() { 1f, 1f, 1f, 1f };
        public static List<float> ImpostorPlayers = new() { 1f, 1f, 5f, 1f };
        public static List<float> QuarreledPlayers = new() { 1f, 1f, 7f, 1f };
        // public static CustomOption ;

        internal static Dictionary<byte, byte[]> blockedRolePairings = new();

        public static string Cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), ModTranslation.GetString(s));
        }


        public static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load()
        {
            List<string> Levedatas = new() { "optionOff", "LevelingerSettingKeep", "PursuerName", "TeleporterName", "SidekickName", "SpeedBoosterName", "MovingName" };
            List<string> LeveTransed = new();
            foreach (string data in Levedatas)
            {
                LeveTransed.Add(ModTranslation.GetString(data));
            }
            LevelingerTexts = LeveTransed.ToArray();
            presetSelection = CustomOption.Create(0, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingpresetSelection"), presets, null, true);

            specialOptions = new CustomOptionBlank(null);
            hideSettings = CustomOption.Create(2, true, CustomOptionType.Generic, Cs(Color.white, "SettingsHideSetting"), false, specialOptions);

            crewmateRolesCountMax = CustomOption.Create(3, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewRole"), 0f, 0f, 15f, 1f);
            crewmateGhostRolesCountMax = CustomOption.Create(4, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewGhostRole"), 0f, 0f, 15f, 1f);
            neutralRolesCountMax = CustomOption.Create(5, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralRole"), 0f, 0f, 15f, 1f);
            neutralGhostRolesCountMax = CustomOption.Create(6, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralGhostRole"), 0f, 0f, 15f, 1f);
            impostorRolesCountMax = CustomOption.Create(7, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoRole"), 0f, 0f, 3f, 1f);
            impostorGhostRolesCountMax = CustomOption.Create(8, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoGhostRole"), 0f, 0f, 15f, 1f);

            enableMirroMap = CustomOption.Create(9, false, CustomOptionType.Generic, "enableMirroMap", false);
            enableAgartha = CustomOption.Create(970, false, CustomOptionType.Generic, "AgarthaName", true, null, isHeader:true);

            if (ConfigRoles.DebugMode.Value)
            {
                IsDebugMode = CustomOption.Create(10, true, CustomOptionType.Generic, "デバッグモード", false, null, isHeader: true);
                DebugModeFastStart = CustomOption.Create(681, true, CustomOptionType.Generic, "即開始", false, IsDebugMode);
            }

            DisconnectNotPCOption = CustomOption.Create(11, true, CustomOptionType.Generic, Cs(Color.white, "DisconnectNotPC"), true, null, isHeader: true);

            ZoomOption = CustomOption.Create(618, false, CustomOptionType.Generic, Cs(Color.white, "Zoomafterdeath"), true, null, isHeader: true);
            MouseZoom = CustomOption.Create(619, false, CustomOptionType.Generic, "mousemode", false, ZoomOption);
            ClairvoyantZoom = CustomOption.Create(620, false, CustomOptionType.Generic, "clairvoyantmode", false, ZoomOption);
            ZoomCoolTime = CustomOption.Create(621, false, CustomOptionType.Generic, "clairvoyantCoolTime", 15f, 1f, 120f, 2.5f, ClairvoyantZoom, format: "unitCouples");
            ZoomDurationTime = CustomOption.Create(622, false, CustomOptionType.Generic, "clairvoyantDurationTime", 5f, 1f, 60f, 2.5f, ClairvoyantZoom, format: "unitCouples");

            MapOptions.MapOption.LoadOption();

            //SoothSayerRate = CustomOption.Create(2, Cs(SoothSayer.color,"soothName"),rates, null, true);
            Mode.ModeHandler.OptionLoad();

            MapCustoms.MapCustom.CreateOption();

            Sabotage.Options.Load();

            IsAlwaysReduceCooldown = CustomOption.Create(682, false, CustomOptionType.Generic, "IsAlwaysReduceCooldown", false, null);
            IsAlwaysReduceCooldownExceptInVent = CustomOption.Create(954, false, CustomOptionType.Generic, "IsAlwaysReduceCooldownExceptInVent", false, IsAlwaysReduceCooldown);
            IsAlwaysReduceCooldownExceptOnTask = CustomOption.Create(684, false, CustomOptionType.Generic, "IsAlwaysReduceCooldownExceptOnTask", true, IsAlwaysReduceCooldown);


            SoothSayerOption = new CustomRoleOption(12, false, CustomOptionType.Crewmate, "SoothSayerName", RoleClass.SoothSayer.color, 1);
            SoothSayerPlayerCount = CustomOption.Create(13, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
            SoothSayerDisplayMode = CustomOption.Create(14, false, CustomOptionType.Crewmate, "SoothSayerDisplaySetting", false, SoothSayerOption);
            SoothSayerMaxCount = CustomOption.Create(15, false, CustomOptionType.Crewmate, "SoothSayerMaxCountSetting", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);

            JesterOption = new CustomRoleOption(16, true, CustomOptionType.Neutral, "JesterName", RoleClass.Jester.color, 1);
            JesterPlayerCount = CustomOption.Create(17, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JesterOption);
            JesterIsVent = CustomOption.Create(18, true, CustomOptionType.Neutral, "JesterIsVentSetting", false, JesterOption);
            JesterIsSabotage = CustomOption.Create(19, false, CustomOptionType.Neutral, "JesterIsSabotageSetting", false, JesterOption);
            JesterIsWinCleartask = CustomOption.Create(20, true, CustomOptionType.Neutral, "JesterIsWinClearTaskSetting", false, JesterOption);
            var jesteroption = SelectTask.TaskSetting(21, 22, 23, JesterIsWinCleartask, CustomOptionType.Neutral, true);
            JesterCommonTask = jesteroption.Item1;
            JesterShortTask = jesteroption.Item2;
            JesterLongTask = jesteroption.Item3;

            LighterOption = new CustomRoleOption(24, false, CustomOptionType.Crewmate, "LighterName", RoleClass.Lighter.color, 1);
            LighterPlayerCount = CustomOption.Create(25, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LighterOption);
            LighterCoolTime = CustomOption.Create(26, false, CustomOptionType.Crewmate, "LigtherCoolDownSetting", 30f, 2.5f, 60f, 2.5f, LighterOption, format: "unitSeconds");
            LighterDurationTime = CustomOption.Create(27, false, CustomOptionType.Crewmate, "LigtherDurationSetting", 10f, 1f, 20f, 0.5f, LighterOption, format: "unitSeconds");
            LighterUpVision = CustomOption.Create(28, false, CustomOptionType.Crewmate, "LighterUpVisionSetting", 0.25f, 0f, 5f, 0.25f, LighterOption);

            /**
            EvilLighterOption = new CustomRoleOption(29, "EvilLighterName", RoleClass.ImpostorRed, 1);
            EvilLighterPlayerCount = CustomOption.Create(30, Cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilLighterOption);
            EvilLighterCoolTime = CustomOption.Create(31, ModTranslation.GetString("EvilLigtherCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, EvilLighterOption, format: "unitSeconds");
            EvilLighterDurationTime = CustomOption.Create(32, ModTranslation.GetString("EvilLigtherDurationSetting"), 10f, 1f, 20f, 0.5f, EvilLighterOption, format: "unitSeconds");

            **/
            EvilScientistOption = new CustomRoleOption(33, false, CustomOptionType.Impostor, "EvilScientistName", RoleClass.ImpostorRed, 1);
            EvilScientistPlayerCount = CustomOption.Create(34, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilScientistOption);
            EvilScientistCoolTime = CustomOption.Create(35, false, CustomOptionType.Impostor, "EvilScientistCoolDownSetting", 30f, 2.5f, 60f, 2.5f, EvilScientistOption, format: "unitSeconds");
            EvilScientistDurationTime = CustomOption.Create(36, false, CustomOptionType.Impostor, "EvilScientistDurationSetting", 10f, 2.5f, 20f, 2.5f, EvilScientistOption, format: "unitSeconds");

            SheriffOption = new CustomRoleOption(37, true, CustomOptionType.Crewmate, "SheriffName", RoleClass.Sheriff.color, 1);
            SheriffPlayerCount = CustomOption.Create(38, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SheriffOption);
            SheriffCoolTime = CustomOption.Create(39, true, CustomOptionType.Crewmate, "SheriffCoolDownSetting", 30f, 2.5f, 60f, 2.5f, SheriffOption, format: "unitSeconds");
            SheriffKillMaxCount = CustomOption.Create(43, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, SheriffOption, format: "unitSeconds");

            SheriffCanKillImpostor = CustomOption.Create(731, true, CustomOptionType.Crewmate, "Impostor" + "CanKillSetting", true, SheriffOption);
            SheriffMadRoleKill = CustomOption.Create(42, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, SheriffOption);

            SheriffFriendsRoleKill = CustomOption.Create(708, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, SheriffOption);

            SheriffNeutralKill = CustomOption.Create(40, true, CustomOptionType.Crewmate, "SheriffIsKillNewtralSetting", false, SheriffOption);
           
            SheriffLoversKill = CustomOption.Create(41, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, SheriffOption);
            SheriffQuarreledKill = CustomOption.Create(730, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, SheriffOption);

            RemoteSheriffOption = new CustomRoleOption(44, true, CustomOptionType.Crewmate, "RemoteSheriffName", RoleClass.RemoteSheriff.color, 1);
            RemoteSheriffPlayerCount = CustomOption.Create(45, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RemoteSheriffOption);
            RemoteSheriffCoolTime = CustomOption.Create(46, true, CustomOptionType.Crewmate, ModTranslation.GetString("SheriffCoolDownSetting"), 30f, 2.5f, 60f, 2.5f, RemoteSheriffOption, format: "unitSeconds");
            RemoteSheriffNeutralKill = CustomOption.Create(47, true, CustomOptionType.Crewmate, "SheriffIsKillNewtralSetting", false, RemoteSheriffOption);
            RemoteSheriffLoversKill = CustomOption.Create(48, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, RemoteSheriffOption);
            RemoteSheriffMadRoleKill = CustomOption.Create(49, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, RemoteSheriffOption);
            RemoteSheriffKillMaxCount = CustomOption.Create(50, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, RemoteSheriffOption, format: "unitSeconds");
            RemoteSheriffIsKillTeleportSetting = CustomOption.Create(51, true, CustomOptionType.Crewmate, "RemoteSheriffIsKillTeleportSetting", false, RemoteSheriffOption);

            MeetingSheriffOption = new CustomRoleOption(52, false, CustomOptionType.Crewmate, "MeetingSheriffName", RoleClass.MeetingSheriff.color, 1);
            MeetingSheriffPlayerCount = CustomOption.Create(53, false, CustomOptionType.Crewmate, Cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MeetingSheriffOption);
            MeetingSheriffNeutralKill = CustomOption.Create(54, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillNeutralSetting", false, MeetingSheriffOption);
            MeetingSheriffMadRoleKill = CustomOption.Create(55, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillMadRoleSetting", false, MeetingSheriffOption);
            MeetingSheriffKillMaxCount = CustomOption.Create(56, false, CustomOptionType.Crewmate, "MeetingSheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, MeetingSheriffOption, format: "unitSeconds");
            MeetingSheriffOneMeetingMultiKill = CustomOption.Create(57, false, CustomOptionType.Crewmate, "MeetingSheriffMeetingmultipleKillSetting", false, MeetingSheriffOption);

            JackalOption = new CustomRoleOption(58, true, CustomOptionType.Neutral, "JackalName", RoleClass.Jackal.color, 1);
            JackalPlayerCount = CustomOption.Create(59, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalOption);
            JackalKillCoolDown = CustomOption.Create(60, true, CustomOptionType.Neutral, "JackalCoolDownSetting", 30f, 2.5f, 60f, 2.5f, JackalOption, format: "unitSeconds");
            JackalUseVent = CustomOption.Create(61, true, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalOption);
            JackalUseSabo = CustomOption.Create(62, true, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalOption);
            JackalIsImpostorLight = CustomOption.Create(63, true, CustomOptionType.Neutral, "MadMateImpostorLightSetting", false, JackalOption);
            JackalCreateFriend = CustomOption.Create(666, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, JackalOption);
            JackalCreateSidekick = CustomOption.Create(64, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalOption);
            JackalNewJackalCreateSidekick = CustomOption.Create(65, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalCreateSidekick);

            TeleporterOption = new CustomRoleOption(66, false, CustomOptionType.Impostor, "TeleporterName", RoleClass.ImpostorRed, 1);
            TeleporterPlayerCount = CustomOption.Create(67, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TeleporterOption);
            TeleporterCoolTime = CustomOption.Create(68, false, CustomOptionType.Impostor, "TeleporterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, TeleporterOption, format: "unitSeconds");
            TeleporterDurationTime = CustomOption.Create(69, false, CustomOptionType.Impostor, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleporterOption, format: "unitSeconds");

            SpiritMediumOption = new CustomRoleOption(70, false, CustomOptionType.Crewmate, "SpiritMediumName", RoleClass.SpiritMedium.color, 1);
            SpiritMediumPlayerCount = CustomOption.Create(71, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpiritMediumOption);
            SpiritMediumDisplayMode = CustomOption.Create(72, false, CustomOptionType.Crewmate, "SpiritMediumDisplaySetting", false, SpiritMediumOption);
            SpiritMediumMaxCount = CustomOption.Create(73, false, CustomOptionType.Crewmate, "SpiritMediumMaxCountSetting", 2f, 1f, 15f, 1f, SpiritMediumOption);

            SpeedBoosterOption = new CustomRoleOption(74, false, CustomOptionType.Crewmate, "SpeedBoosterName", RoleClass.SpeedBooster.color, 1);
            SpeedBoosterPlayerCount = CustomOption.Create(75, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeedBoosterOption);
            SpeedBoosterCoolTime = CustomOption.Create(76, false, CustomOptionType.Crewmate, "SpeedBoosterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
            SpeedBoosterDurationTime = CustomOption.Create(77, false, CustomOptionType.Crewmate, "SpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
            SpeedBoosterSpeed = CustomOption.Create(78, false, CustomOptionType.Crewmate, "SpeedBoosterPlusSpeedSetting", 0.5f, 0.0f, 5f, 0.25f, SpeedBoosterOption, format: "unitSeconds");

            EvilSpeedBoosterOption = new CustomRoleOption(79, false, CustomOptionType.Impostor, "EvilSpeedBoosterName", RoleClass.ImpostorRed, 1);
            EvilSpeedBoosterPlayerCount = CustomOption.Create(80, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EvilSpeedBoosterOption);
            EvilSpeedBoosterCoolTime = CustomOption.Create(81, false, CustomOptionType.Impostor, "EvilSpeedBoosterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterDurationTime = CustomOption.Create(82, false, CustomOptionType.Impostor, "EvilSpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterSpeed = CustomOption.Create(83, false, CustomOptionType.Impostor, "EvilSpeedBoosterPlusSpeedSetting", 0.5f, 0.0f, 5f, 0.25f, EvilSpeedBoosterOption, format: "unitSeconds");
            EvilSpeedBoosterIsNotSpeedBooster = CustomOption.Create(84, false, CustomOptionType.Impostor, "EvilSpeedBoosterIsNotSpeedBooster", false, EvilSpeedBoosterOption);
            /**
            TaskerOption = new CustomRoleOption(85, "TaskerName", RoleClass.ImpostorRed, 1);
            TaskerPlayerCount = CustomOption.Create(86, Cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TaskerOption);
            TaskerAmount = CustomOption.Create(87, ModTranslation.GetString("TaskerTaskSetting"), 30f, 2.5f, 60f, 2.5f, TaskerOption);
            TaskerIsKill = CustomOption.Create(88, ModTranslation.GetString("TaskerIsKillSetting"), false, TaskerOption);
            **/
            DoorrOption = new CustomRoleOption(89, false, CustomOptionType.Crewmate, "DoorrName", RoleClass.Doorr.color, 1);
            DoorrPlayerCount = CustomOption.Create(90, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoorrOption);
            DoorrCoolTime = CustomOption.Create(91, false, CustomOptionType.Crewmate, "DoorrCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoorrOption);

            EvilDoorrOption = new CustomRoleOption(92, false, CustomOptionType.Impostor, "EvilDoorrName", RoleClass.ImpostorRed, 1);
            EvilDoorrPlayerCount = CustomOption.Create(93, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilDoorrOption);
            EvilDoorrCoolTime = CustomOption.Create(94, false, CustomOptionType.Impostor, "EvilDoorrCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, EvilDoorrOption);

            ShielderOption = new CustomRoleOption(95, false, CustomOptionType.Crewmate, "ShielderName", RoleClass.Shielder.color, 1);
            ShielderPlayerCount = CustomOption.Create(96, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ShielderOption);
            ShielderCoolTime = CustomOption.Create(97, false, CustomOptionType.Crewmate, "ShielderCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, ShielderOption, format: "unitCouples");
            ShielderDurationTime = CustomOption.Create(98, false, CustomOptionType.Crewmate, "ShielderDurationSetting", 10f, 2.5f, 30f, 2.5f, ShielderOption, format: "unitCouples");

            FreezerOption = new CustomRoleOption(99, false, CustomOptionType.Impostor, "FreezerName", RoleClass.ImpostorRed, 1);
            FreezerPlayerCount = CustomOption.Create(100, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FreezerOption);
            FreezerCoolTime = CustomOption.Create(101, false, CustomOptionType.Impostor, "FreezerCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, FreezerOption, format: "unitSeconds");
            FreezerDurationTime = CustomOption.Create(102, false, CustomOptionType.Impostor, "FreezerDurationSetting", 1f, 1f, 7f, 1f, FreezerOption, format: "unitSeconds");

            SpeederOption = new CustomRoleOption(103, false, CustomOptionType.Impostor, "SpeederName", RoleClass.ImpostorRed, 1);
            SpeederPlayerCount = CustomOption.Create(104, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeederOption);
            SpeederCoolTime = CustomOption.Create(105, false, CustomOptionType.Impostor, "SpeederCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, SpeederOption, format: "unitSeconds");
            SpeederDurationTime = CustomOption.Create(106, false, CustomOptionType.Impostor, "SpeederDurationTimeSetting", 10f, 2.5f, 20f, 2.5f, SpeederOption, format: "unitSeconds");
            /*
            GuesserOption = new CustomRoleOption(107, "GuesserName", RoleClass.Guesser.color, 1);
            GuesserPlayerCount = CustomOption.Create(108, Cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GuesserOption);
            GuesserShortMaxCount = CustomOption.Create(109, ModTranslation.GetString("GuesserShortMaxCountSetting"), 30f, 2.5f, 60f, 2.5f, GuesserOption);
            GuesserShortOneMeetingCount = CustomOption.Create(110, Cs(Color.white, "GuesserOneMeetingShortSetting"), GuesserCount, GuesserOption);

            EvilGuesserOption = new CustomRoleOption(111, "EvilGuesserName", RoleClass.ImpostorRed, 1);
            EvilGuesserPlayerCount = CustomOption.Create(112, Cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGuesserOption);
            EvilGuesserShortMaxCount = CustomOption.Create(113, ModTranslation.GetString("EvilGuesserShortMaxCountSetting"), 30f, 2.5f, 60f, 2.5f, EvilGuesserOption);
            EvilGuesserShortOneMeetingCount = CustomOption.Create(114, Cs(Color.white, "EvilGuesserOneMeetingShortSetting"), GuesserCount, EvilGuesserOption);
            */
            VultureOption = new CustomRoleOption(115, false, CustomOptionType.Neutral, "VultureName", RoleClass.Vulture.color, 1);
            VulturePlayerCount = CustomOption.Create(116, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], VultureOption);
            VultureCoolDown = CustomOption.Create(117, false, CustomOptionType.Neutral, "VultureCoolDownSetting", 30f, 2.5f, 60f, 2.5f, VultureOption, format: "unitSeconds");
            VultureDeadBodyMaxCount = CustomOption.Create(118, false, CustomOptionType.Neutral, "VultureDeadBodyCountSetting", 3f, 1f, 6f, 1f, VultureOption);
            VultureIsUseVent = CustomOption.Create(119, false, CustomOptionType.Neutral, "MadMateUseVentSetting", false, VultureOption);
            VultureShowArrows = CustomOption.Create(120, false, CustomOptionType.Neutral, "VultureShowArrowsSetting", false, VultureOption);

            NiceScientistOption = new CustomRoleOption(121, false, CustomOptionType.Crewmate, "NiceScientistName", RoleClass.NiceScientist.color, 1);
            NiceScientistPlayerCount = CustomOption.Create(122, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceScientistOption);
            NiceScientistCoolTime = CustomOption.Create(123, false, CustomOptionType.Crewmate, "NiceScientistCoolDownSetting", 30f, 2.5f, 60f, 2.5f, NiceScientistOption, format: "unitSeconds");
            NiceScientistDurationTime = CustomOption.Create(124, false, CustomOptionType.Crewmate, "NiceScientistDurationSetting", 10f, 2.5f, 20f, 2.5f, NiceScientistOption, format: "unitSeconds");

            ClergymanOption = new CustomRoleOption(125, false, CustomOptionType.Crewmate, "ClergymanName", RoleClass.Clergyman.color, 1);
            ClergymanPlayerCount = CustomOption.Create(126, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ClergymanOption);
            ClergymanCoolTime = CustomOption.Create(127, false, CustomOptionType.Crewmate, "ClergymanCoolDownSetting", 30f, 2.5f, 60f, 2.5f, ClergymanOption, format: "unitSeconds");
            ClergymanDurationTime = CustomOption.Create(128, false, CustomOptionType.Crewmate, "ClergymanDurationTimeSetting", 10f, 1f, 20f, 0.5f, ClergymanOption, format: "unitSeconds");
            ClergymanDownVision = CustomOption.Create(129, false, CustomOptionType.Crewmate, "ClergymanDownVisionSetting", 0.25f, 0f, 5f, 0.25f, ClergymanOption);

            MadMateOption = new CustomRoleOption(130, true, CustomOptionType.Crewmate, "MadMateName", RoleClass.ImpostorRed, 1);
            MadMatePlayerCount = CustomOption.Create(131, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMateOption);
            MadMateIsCheckImpostor = CustomOption.Create(132, true, CustomOptionType.Crewmate, "MadMateIsCheckImpostorSetting", false, MadMateOption);
            var madmateoption = SelectTask.TaskSetting(133, 134, 135, MadMateIsCheckImpostor, CustomOptionType.Crewmate, true);
            MadMateCommonTask = madmateoption.Item1;
            MadMateShortTask = madmateoption.Item2;
            MadMateLongTask = madmateoption.Item3;
            //MadMateIsNotTask = madmateoption.Item4;
            MadMateCheckImpostorTask = CustomOption.Create(136, true, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, MadMateIsCheckImpostor);
            MadMateIsUseVent = CustomOption.Create(137, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadMateOption);
            MadMateIsImpostorLight = CustomOption.Create(138, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadMateOption);

            BaitOption = new CustomRoleOption(486, true, CustomOptionType.Crewmate, "BaitName", RoleClass.Bait.color, 1);
            BaitPlayerCount = CustomOption.Create(487, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BaitOption);
            BaitReportTime = CustomOption.Create(488, true, CustomOptionType.Crewmate, "BaitReportTimeSetting", 2f, 1f, 4f, 0.5f, BaitOption);

            HomeSecurityGuardOption = new CustomRoleOption(139, true, CustomOptionType.Crewmate, "HomeSecurityGuardName", RoleClass.HomeSecurityGuard.color, 1);
            HomeSecurityGuardPlayerCount = CustomOption.Create(140, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HomeSecurityGuardOption);

            StuntManOption = new CustomRoleOption(141, true, CustomOptionType.Crewmate, "StuntManName", RoleClass.StuntMan.color, 1);
            StuntManPlayerCount = CustomOption.Create(142, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StuntManOption);
            StuntManMaxGuardCount = CustomOption.Create(143, true, CustomOptionType.Crewmate, "StuntManGuardMaxCountSetting", 1f, 1f, 15f, 1f, StuntManOption);

            MovingOption = new CustomRoleOption(144, false, CustomOptionType.Crewmate, "MovingName", RoleClass.Moving.color, 1);
            MovingPlayerCount = CustomOption.Create(145, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MovingOption);
            MovingCoolTime = CustomOption.Create(146, false, CustomOptionType.Crewmate, "MovingCoolDownSetting", 30f, 0f, 60f, 2.5f, MovingOption);

            OpportunistOption = new CustomRoleOption(147, true, CustomOptionType.Neutral, "OpportunistName", RoleClass.Opportunist.color, 1);
            OpportunistPlayerCount = CustomOption.Create(148, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], OpportunistOption);

            /**
            NiceGamblerOption = new CustomRoleOption(149, "NiceGamblerName", RoleClass.NiceGambler.color, 1);
            NiceGamblerPlayerCount = CustomOption.Create(150, Cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceGamblerOption);
            NiceGamblerUseCount = CustomOption.Create(151, Cs(Color.white, "NiceGamblerUseCountSetting"), 1f, 1f,15f, 1f, NiceGamblerOption);
            **/
            EvilGamblerOption = new CustomRoleOption(152, true, CustomOptionType.Impostor, "EvilGamblerName", RoleClass.EvilGambler.color, 1);
            EvilGamblerPlayerCount = CustomOption.Create(153, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGamblerOption);
            EvilGamblerSucTime = CustomOption.Create(154, true, CustomOptionType.Impostor, "EvilGamblerSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
            EvilGamblerNotSucTime = CustomOption.Create(155, true, CustomOptionType.Impostor, "EvilGamblerNotSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
            EvilGamblerSucpar = CustomOption.Create(156, true, CustomOptionType.Impostor, "EvilGamblerSucParSetting", rates, EvilGamblerOption);

            BestfalsechargeOption = new CustomRoleOption(157, true, CustomOptionType.Crewmate, "BestfalsechargeName", RoleClass.Bestfalsecharge.color, 1);
            BestfalsechargePlayerCount = CustomOption.Create(158, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BestfalsechargeOption);
            /*
            ResearcherOption = new CustomRoleOption(159, "ResearcherName", RoleClass.Researcher.color, 1);
            ResearcherPlayerCount = CustomOption.Create(160, Cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ResearcherOption);
            **/
            SelfBomberOption = new CustomRoleOption(161, true, CustomOptionType.Impostor, "SelfBomberName", RoleClass.SelfBomber.color, 1);
            SelfBomberPlayerCount = CustomOption.Create(162, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SelfBomberOption);
            SelfBomberScope = CustomOption.Create(163, true, CustomOptionType.Impostor, "SelfBomberScopeSetting", 1f, 0.5f, 3f, 0.5f, SelfBomberOption);

            GodOption = new CustomRoleOption(164, true, CustomOptionType.Neutral, "GodName", RoleClass.God.color, 1);
            GodPlayerCount = CustomOption.Create(165, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GodOption);
            GodViewVote = CustomOption.Create(167, true, CustomOptionType.Neutral, "GodViewVoteSetting", false, GodOption);
            GodIsEndTaskWin = CustomOption.Create(168, true, CustomOptionType.Neutral, "GodIsEndTaskWinSetting", true, GodOption);
            var godoption = SelectTask.TaskSetting(169, 170, 171, GodIsEndTaskWin, CustomOptionType.Neutral, true);
            GodCommonTask = godoption.Item1;
            GodShortTask = godoption.Item2;
            GodLongTask = godoption.Item3;
            /*
            AllCleanerOption = new CustomRoleOption(172, "AllCleanerName", RoleClass.AllCleaner.color, 1);
            AllCleanerPlayerCount = CustomOption.Create(173, Cs(Color.white, "SettingPlayerCountName"), ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], AllCleanerOption);
            AllCleanerCount = CustomOption.Create(174, Cs(Color.white, "AllCleanerCountSetting"), 1f, 1f, 15f, 1f, AllCleanerOption);
            */
            NiceNekomataOption = new CustomRoleOption(175, true, CustomOptionType.Crewmate, "NiceNekomataName", RoleClass.NiceNekomata.color, 1);
            NiceNekomataPlayerCount = CustomOption.Create(176, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceNekomataOption);
            NiceNekomataIsChain = CustomOption.Create(177, true, CustomOptionType.Crewmate, "NiceNekomataIsChainSetting", true, NiceNekomataOption);

            EvilNekomataOption = new CustomRoleOption(178, true, CustomOptionType.Impostor, "EvilNekomataName", RoleClass.EvilNekomata.color, 1);
            EvilNekomataPlayerCount = CustomOption.Create(179, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilNekomataOption);
            EvilNekomataNotImpostorExiled = CustomOption.Create(674, true, CustomOptionType.Impostor, "NotImpostorExiled", false, EvilNekomataOption);

            JackalFriendsOption = new CustomRoleOption(180, true, CustomOptionType.Crewmate, "JackalFriendsName", RoleClass.JackalFriends.color, 1);
            JackalFriendsPlayerCount = CustomOption.Create(181, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalFriendsOption);
            JackalFriendsIsUseVent = CustomOption.Create(182, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, JackalFriendsOption);
            JackalFriendsIsImpostorLight = CustomOption.Create(183, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, JackalFriendsOption);
            JackalFriendsIsCheckJackal = CustomOption.Create(184, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, JackalFriendsOption);
            var JackalFriendsoption = SelectTask.TaskSetting(185, 186, 187, JackalFriendsIsCheckJackal, CustomOptionType.Crewmate, true);
            JackalFriendsCommonTask = JackalFriendsoption.Item1;
            JackalFriendsShortTask = JackalFriendsoption.Item2;
            JackalFriendsLongTask = JackalFriendsoption.Item3;
            JackalFriendsCheckJackalTask = CustomOption.Create(189, true, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, JackalFriendsIsCheckJackal);

            DoctorOption = new CustomRoleOption(190, false, CustomOptionType.Crewmate, "DoctorName", RoleClass.Doctor.color, 1);
            DoctorPlayerCount = CustomOption.Create(191, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoctorOption);

            DoctorChargeTime = CustomOption.Create(966, false, CustomOptionType.Crewmate, "DoctorChargeTime", 10f, 0f, 60f, 2.5f, DoctorOption);
            DoctorUseTime = CustomOption.Create(967, false, CustomOptionType.Crewmate, "DoctorUseTime", 5f, 0f, 60f, 2.5f, DoctorOption);

            CountChangerOption = new CustomRoleOption(192, false, CustomOptionType.Impostor, "CountChangerName", RoleClass.CountChanger.color, 1);
            CountChangerPlayerCount = CustomOption.Create(193, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CountChangerOption);
            CountChangerMaxCount = CustomOption.Create(194, false, CustomOptionType.Impostor, "CountChangerMaxCountSetting", 1f, 1f, 15f, 1f, CountChangerOption);
            CountChangerNextTurn = CustomOption.Create(195, false, CustomOptionType.Impostor, "CountChangerNextTurnSetting", false, CountChangerOption);

            PursuerOption = new CustomRoleOption(196, false, CustomOptionType.Impostor, "PursuerName", RoleClass.Pursuer.color, 1);
            PursuerPlayerCount = CustomOption.Create(197, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PursuerOption);

            MinimalistOption = new CustomRoleOption(198, true, CustomOptionType.Impostor, "MinimalistName", RoleClass.Minimalist.color, 1);
            MinimalistPlayerCount = CustomOption.Create(199, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MinimalistOption);
            MinimalistKillCoolTime = CustomOption.Create(200, true, CustomOptionType.Impostor, "MinimalistKillCoolSetting", 20f, 2.5f, 60f, 2.5f, MinimalistOption);
            MinimalistVent = CustomOption.Create(201, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, MinimalistOption);
            MinimalistSabo = CustomOption.Create(202, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, MinimalistOption);
            MinimalistReport = CustomOption.Create(203, true, CustomOptionType.Impostor, "MinimalistReportSetting", true, MinimalistOption);

            HawkOption = new CustomRoleOption(204, false, CustomOptionType.Impostor, "HawkName", RoleClass.Hawk.color, 1);
            HawkPlayerCount = CustomOption.Create(205, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], HawkOption);
            HawkCoolTime = CustomOption.Create(206, false, CustomOptionType.Impostor, "HawkCoolTimeSetting", 15f, 1f, 120f, 2.5f, HawkOption, format: "unitCouples");
            HawkDurationTime = CustomOption.Create(207, false, CustomOptionType.Impostor, "HawkDurationTimeSetting", 5f, 1f, 60f, 2.5f, HawkOption, format: "unitCouples");

            EgoistOption = new CustomRoleOption(208, true, CustomOptionType.Neutral, "EgoistName", RoleClass.Egoist.color, 1);
            EgoistPlayerCount = CustomOption.Create(209, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EgoistOption);
            EgoistUseVent = CustomOption.Create(210, true, CustomOptionType.Neutral, "EgoistUseVentSetting", false, EgoistOption);
            EgoistUseSabo = CustomOption.Create(211, true, CustomOptionType.Neutral, "EgoistUseSaboSetting", false, EgoistOption);
            EgoistImpostorLight = CustomOption.Create(212, true, CustomOptionType.Neutral, "EgoistImpostorLightSetting", false, EgoistOption);
            EgoistUseKill = CustomOption.Create(213, true, CustomOptionType.Neutral, "EgoistUseKillSetting", false, EgoistOption);

            NiceRedRidingHoodOption = new CustomRoleOption(214, false, CustomOptionType.Crewmate, "NiceRedRidingHoodName", RoleClass.NiceRedRidingHood.color, 1);
            NiceRedRidingHoodPlayerCount = CustomOption.Create(215, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceRedRidingHoodOption);
            NiceRedRidingHoodCount = CustomOption.Create(216, false, CustomOptionType.Crewmate, "NiceRedRidingHoodCount", 1f, 1f, 15f, 1f, NiceRedRidingHoodOption);

            EvilEraserOption = new CustomRoleOption(217, false, CustomOptionType.Impostor, "EvilEraserName", RoleClass.EvilEraser.color, 1);
            EvilEraserPlayerCount = CustomOption.Create(218, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilEraserOption);
            EvilEraserMaxCount = CustomOption.Create(219, false, CustomOptionType.Impostor, "EvilEraserMaxCountSetting", 1f, 1f, 15f, 1f, EvilEraserOption);

            WorkpersonOption = new CustomRoleOption(220, true, CustomOptionType.Neutral, "WorkpersonName", RoleClass.Workperson.color, 1);
            WorkpersonPlayerCount = CustomOption.Create(221, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WorkpersonOption);
            WorkpersonIsAliveWin = CustomOption.Create(222, true, CustomOptionType.Neutral, "WorkpersonIsAliveWinSetting", false, WorkpersonOption);
            WorkpersonCommonTask = CustomOption.Create(223, true, CustomOptionType.Neutral, "GameCommonTasks", 2, 0, 12, 1, WorkpersonOption);
            WorkpersonLongTask = CustomOption.Create(224, true, CustomOptionType.Neutral, "GameLongTasks", 10, 0, 69, 1, WorkpersonOption);
            WorkpersonShortTask = CustomOption.Create(225, true, CustomOptionType.Neutral, "GameShortTasks", 5, 0, 45, 1, WorkpersonOption);

            MagazinerOption = new CustomRoleOption(226, false, CustomOptionType.Impostor, "MagazinerName", RoleClass.Magaziner.color, 1);
            MagazinerPlayerCount = CustomOption.Create(227, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MagazinerOption);
            MagazinerSetKillTime = CustomOption.Create(228, false, CustomOptionType.Impostor, "MagazinerSetTimeSetting", 0f, 0f, 60f, 2.5f, MagazinerOption);

            MayorOption = new CustomRoleOption(229, true, CustomOptionType.Crewmate, "MayorName", RoleClass.Mayor.color, 1);
            MayorPlayerCount = CustomOption.Create(230, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorOption);
            MayorVoteCount = CustomOption.Create(231, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorOption);

            trueloverOption = new CustomRoleOption(232, true, CustomOptionType.Neutral, "trueloverName", RoleClass.Truelover.color, 1);
            trueloverPlayerCount = CustomOption.Create(233, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], trueloverOption);

            TechnicianOption = new CustomRoleOption(234, true, CustomOptionType.Crewmate, "TechnicianName", RoleClass.Technician.color, 1);
            TechnicianPlayerCount = CustomOption.Create(235, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TechnicianOption);

            SerialKillerOption = new CustomRoleOption(236, true, CustomOptionType.Impostor, "SerialKillerName", RoleClass.SerialKiller.color, 1);
            SerialKillerPlayerCount = CustomOption.Create(237, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SerialKillerOption);
            SerialKillerSuicideTime = CustomOption.Create(238, true, CustomOptionType.Impostor, "SerialKillerSuicideTimeSetting", 60f, 0f, 180f, 2.5f, SerialKillerOption);
            SerialKillerKillTime = CustomOption.Create(239, true, CustomOptionType.Impostor, "SerialKillerKillTimeSetting", 15f, 0f, 60f, 2.5f, SerialKillerOption);
            SerialKillerIsMeetingReset = CustomOption.Create(240, true, CustomOptionType.Impostor, "SerialKillerIsMeetingResetSetting", true, SerialKillerOption);

            OverKillerOption = new CustomRoleOption(241, true, CustomOptionType.Impostor, "OverKillerName", RoleClass.OverKiller.color, 1);
            OverKillerPlayerCount = CustomOption.Create(242, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], OverKillerOption);
            OverKillerKillCoolTime = CustomOption.Create(243, true, CustomOptionType.Impostor, "OverKillerKillCoolTimeSetting", 45f, 0f, 60f, 2.5f, OverKillerOption);
            OverKillerKillCount = CustomOption.Create(245, true, CustomOptionType.Impostor, "OverKillerKillCountSetting", 30f, 1f, 60f, 1f, OverKillerOption);

            LevelingerOption = new CustomRoleOption(246, false, CustomOptionType.Impostor, "LevelingerName", RoleClass.Levelinger.color, 1);
            LevelingerPlayerCount = CustomOption.Create(247, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], LevelingerOption);
            LevelingerOneKillXP = CustomOption.Create(248, false, CustomOptionType.Impostor, "LevelingerOneKillXPSetting", 1f, 0f, 10f, 1f, LevelingerOption);
            LevelingerUpLevelXP = CustomOption.Create(249, false, CustomOptionType.Impostor, "LevelingerUpLevelXPSetting", 2f, 1f, 50f, 1f, LevelingerOption);
            LevelingerLevelOneGetPower = CustomOption.Create(250, false, CustomOptionType.Impostor, "1" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelTwoGetPower = CustomOption.Create(251, false, CustomOptionType.Impostor, "2" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelThreeGetPower = CustomOption.Create(252, false, CustomOptionType.Impostor, "3" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelFourGetPower = CustomOption.Create(253, false, CustomOptionType.Impostor, "4" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerLevelFiveGetPower = CustomOption.Create(254, false, CustomOptionType.Impostor, "5" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
            LevelingerReviveXP = CustomOption.Create(255, false, CustomOptionType.Impostor, "LevelingerReviveXPSetting", false, LevelingerOption);
            LevelingerUseXPRevive = CustomOption.Create(256, false, CustomOptionType.Impostor, "LevelingerUseXPReviveSetting", 5f, 0f, 20f, 1f, LevelingerReviveXP);

            EvilMovingOption = new CustomRoleOption(257, false, CustomOptionType.Impostor, "EvilMovingName", RoleClass.EvilMoving.color, 1);
            EvilMovingPlayerCount = CustomOption.Create(258, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilMovingOption);
            EvilMovingCoolTime = CustomOption.Create(259, false, CustomOptionType.Impostor, "MovingCoolDownSetting", 30f, 0f, 60f, 2.5f, EvilMovingOption);

            AmnesiacOption = new CustomRoleOption(260, false, CustomOptionType.Neutral, "AmnesiacName", RoleClass.Amnesiac.color, 1);
            AmnesiacPlayerCount = CustomOption.Create(261, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AmnesiacOption);

            SideKillerOption = new CustomRoleOption(262, false, CustomOptionType.Impostor, "SideKillerName", RoleClass.SideKiller.color, 1);
            SideKillerPlayerCount = CustomOption.Create(263, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SideKillerOption);
            SideKillerKillCoolTime = CustomOption.Create(264, false, CustomOptionType.Impostor, "SideKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);
            SideKillerMadKillerKillCoolTime = CustomOption.Create(265, false, CustomOptionType.Impostor, "SideKillerMadKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);

            SurvivorOption = new CustomRoleOption(266, true, CustomOptionType.Impostor, "SurvivorName", RoleClass.Survivor.color, 1);
            SurvivorPlayerCount = CustomOption.Create(267, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SurvivorOption);
            SurvivorKillCoolTime = CustomOption.Create(268, true, CustomOptionType.Impostor, "SurvivorKillCoolTimeSetting", 15f, 0f, 75f, 2.5f, SurvivorOption);

            MadMayorOption = new CustomRoleOption(269, true, CustomOptionType.Crewmate, "MadMayorName", RoleClass.ImpostorRed, 1);
            MadMayorPlayerCount = CustomOption.Create(270, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMayorOption);
            MadMayorVoteCount = CustomOption.Create(271, true, CustomOptionType.Crewmate, "MadMayorVoteCountSetting", 2f, 1f, 100f, 1f, MadMayorOption);
            MadMayorIsCheckImpostor = CustomOption.Create(272, true, CustomOptionType.Crewmate, "MadMayorIsCheckImpostorSetting", false, MadMayorOption);
            var madmayoroption = SelectTask.TaskSetting(273, 274, 275, MadMayorIsCheckImpostor, CustomOptionType.Crewmate, true);
            MadMayorCommonTask = madmayoroption.Item1;
            MadMayorShortTask = madmayoroption.Item2;
            MadMayorLongTask = madmayoroption.Item3;
            MadMayorCheckImpostorTask = CustomOption.Create(276, true, CustomOptionType.Crewmate, "MadMayorCheckImpostorTaskSetting", rates4, MadMayorIsCheckImpostor);
            MadMayorIsUseVent = CustomOption.Create(277, true, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadMayorOption);
            MadMayorIsImpostorLight = CustomOption.Create(278, true, CustomOptionType.Crewmate, "MadMayorImpostorLightSetting", false, MadMayorOption);

            NiceHawkOption = new CustomRoleOption(279, false, CustomOptionType.Crewmate, "NiceHawkName", RoleClass.NiceHawk.color, 1);
            NiceHawkPlayerCount = CustomOption.Create(280, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceHawkOption);
            NiceHawkCoolTime = CustomOption.Create(281, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 1f, 120f, 2.5f, NiceHawkOption, format: "unitCouples");
            NiceHawkDurationTime = CustomOption.Create(282, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 2.5f, NiceHawkOption, format: "unitCouples");

            MadStuntManOption = new CustomRoleOption(283, false, CustomOptionType.Crewmate, "MadStuntManName", RoleClass.ImpostorRed, 1);
            MadStuntManPlayerCount = CustomOption.Create(284, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadStuntManOption);
            MadStuntManIsUseVent = CustomOption.Create(285, false, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadStuntManOption);
            MadStuntManIsImpostorLight = CustomOption.Create(286, false, CustomOptionType.Crewmate, "MadStuntManImpostorLightSetting", false, MadStuntManOption);

            MadHawkOption = new CustomRoleOption(287, false, CustomOptionType.Crewmate, "MadHawkName", RoleClass.ImpostorRed, 1);
            MadHawkPlayerCount = CustomOption.Create(289, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadHawkOption);
            MadHawkCoolTime = CustomOption.Create(290, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 1f, 120f, 2.5f, MadHawkOption, format: "unitCouples");
            MadHawkDurationTime = CustomOption.Create(291, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 2.5f, MadHawkOption, format: "unitCouples");
            MadHawkIsUseVent = CustomOption.Create(292, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadHawkOption);
            MadHawkIsImpostorLight = CustomOption.Create(293, false, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadHawkOption);

            BakeryOption = new CustomRoleOption(294, true, CustomOptionType.Crewmate, "BakeryName", RoleClass.Bakery.color, 1);
            BakeryPlayerCount = CustomOption.Create(295, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BakeryOption);

            MadJesterOption = new CustomRoleOption(296, true, CustomOptionType.Crewmate, "MadJesterName", RoleClass.MadJester.color, 1);
            MadJesterPlayerCount = CustomOption.Create(297, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadJesterOption);
            MadJesterIsUseVent = CustomOption.Create(298, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadJesterOption);
            MadJesterIsImpostorLight = CustomOption.Create(299, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadJesterOption);
            IsMadJesterTaskClearWin = CustomOption.Create(300, true, CustomOptionType.Crewmate, "JesterIsWinClearTaskSetting", false, MadJesterOption);
            var MadJesteroption = SelectTask.TaskSetting(667, 668, 669, IsMadJesterTaskClearWin, CustomOptionType.Crewmate, true);
            MadJesterCommonTask = MadJesteroption.Item1;
            MadJesterShortTask = MadJesteroption.Item2;
            MadJesterLongTask = MadJesteroption.Item3;

            FalseChargesOption = new CustomRoleOption(517, true, CustomOptionType.Neutral, "FalseChargesName", RoleClass.FalseCharges.color, 1);
            FalseChargesPlayerCount = CustomOption.Create(518, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FalseChargesOption);
            FalseChargesExileTurn = CustomOption.Create(519, true, CustomOptionType.Neutral, "FalseChargesExileTurn", 2f, 1f, 10f, 1f, FalseChargesOption);
            FalseChargesCoolTime = CustomOption.Create(520, true, CustomOptionType.Neutral, "FalseChargesCoolTime", 15f, 0f, 75f, 2.5f, FalseChargesOption);

            NiceTeleporterOption = new CustomRoleOption(521, false, CustomOptionType.Crewmate, "NiceTeleporterName", RoleClass.NiceTeleporter.color, 1);
            NiceTeleporterPlayerCount = CustomOption.Create(522, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceTeleporterOption);
            NiceTeleporterCoolTime = CustomOption.Create(523, false, CustomOptionType.Crewmate, "NiceTeleporterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, NiceTeleporterOption, format: "unitSeconds");
            NiceTeleporterDurationTime = CustomOption.Create(524, false, CustomOptionType.Crewmate, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, NiceTeleporterOption, format: "unitSeconds");

            CelebrityOption = new CustomRoleOption(525, true, CustomOptionType.Crewmate, "CelebrityName", RoleClass.Celebrity.color, 1);
            CelebrityPlayerCount = CustomOption.Create(301, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], CelebrityOption);
            CelebrityChangeRoleView = CustomOption.Create(302, true, CustomOptionType.Crewmate, "CelebrityChangeRoleViewSetting", false, CelebrityOption);

            NocturnalityOption = new CustomRoleOption(303, true, CustomOptionType.Crewmate, "NocturnalityName", RoleClass.Nocturnality.color, 1);
            NocturnalityPlayerCount = CustomOption.Create(304, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NocturnalityOption);

            ObserverOption = new CustomRoleOption(305, true, CustomOptionType.Crewmate, "ObserverName", RoleClass.Observer.color, 1);
            ObserverPlayerCount = CustomOption.Create(306, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ObserverOption);

            VampireOption = new CustomRoleOption(307, false, CustomOptionType.Impostor, "VampireName", RoleClass.Vampire.color, 1);
            VampirePlayerCount = CustomOption.Create(308, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VampireOption);
            VampireKillDelay = CustomOption.Create(309, false, CustomOptionType.Impostor, "VampireKillDelay", 0f, 1f, 60f, 0.5f, VampireOption, format: "unitSeconds");

            FoxOption = new CustomRoleOption(310, true, CustomOptionType.Neutral, "FoxName", RoleClass.Fox.color, 1);
            FoxPlayerCount = CustomOption.Create(311, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FoxOption);
            FoxIsUseVent = CustomOption.Create(312, true, CustomOptionType.Neutral, "MadMateUseVentSetting", false, FoxOption);
            FoxIsImpostorLight = CustomOption.Create(313, true, CustomOptionType.Neutral, "MadMateImpostorLightSetting", false, FoxOption);
            FoxReport = CustomOption.Create(314, true, CustomOptionType.Neutral, "MinimalistReportSetting", true, FoxOption);

            DarkKillerOption = new CustomRoleOption(315, false, CustomOptionType.Impostor, "DarkKillerName", RoleClass.DarkKiller.color, 1);
            DarkKillerPlayerCount = CustomOption.Create(316, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DarkKillerOption);
            DarkKillerKillCoolTime = CustomOption.Create(317, false, CustomOptionType.Impostor, "DarkKillerKillCoolSetting", 20f, 2.5f, 60f, 2.5f, DarkKillerOption);

            SeerOption = new CustomRoleOption(318, false, CustomOptionType.Crewmate, "SeerName", RoleClass.Seer.color, 1);
            SeerPlayerCount = CustomOption.Create(319, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerOption);
            SeerMode = CustomOption.Create(320, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerOption);
            SeerLimitSoulDuration = CustomOption.Create(321, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerOption);
            SeerSoulDuration = CustomOption.Create(322, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerLimitSoulDuration, format: "unitCouples");

            MadSeerOption = new CustomRoleOption(323, false, CustomOptionType.Crewmate, "MadSeerName", RoleClass.MadSeer.color, 1);
            MadSeerPlayerCount = CustomOption.Create(324, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadSeerOption);
            MadSeerMode = CustomOption.Create(325, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, MadSeerOption);
            MadSeerLimitSoulDuration = CustomOption.Create(326, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, MadSeerOption);
            MadSeerSoulDuration = CustomOption.Create(327, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, MadSeerLimitSoulDuration, format: "unitCouples");
            MadSeerIsUseVent = CustomOption.Create(328, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadSeerOption);
            MadSeerIsImpostorLight = CustomOption.Create(329, false, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadSeerOption);
            MadSeerIsCheckImpostor = CustomOption.Create(330, false, CustomOptionType.Crewmate, "MadMateIsCheckImpostorSetting", false, MadSeerOption);
            var madseeroption = SelectTask.TaskSetting(331, 332, 526, MadSeerIsCheckImpostor, CustomOptionType.Crewmate, true);
            MadSeerCommonTask = madseeroption.Item1;
            MadSeerShortTask = madseeroption.Item2;
            MadSeerLongTask = madseeroption.Item3;
            MadSeerCheckImpostorTask = CustomOption.Create(333, false, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, MadSeerIsCheckImpostor);

            EvilSeerOption = new CustomRoleOption(334, false, CustomOptionType.Impostor, "EvilSeerName", RoleClass.EvilSeer.color, 1);
            EvilSeerPlayerCount = CustomOption.Create(335, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilSeerOption);
            EvilSeerMode = CustomOption.Create(336, false, CustomOptionType.Impostor, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, EvilSeerOption);
            EvilSeerLimitSoulDuration = CustomOption.Create(337, false, CustomOptionType.Impostor, "SeerLimitSoulDuration", false, EvilSeerOption);
            EvilSeerSoulDuration = CustomOption.Create(338, false, CustomOptionType.Impostor, "SeerSoulDuration", 15f, 0f, 120f, 5f, EvilSeerLimitSoulDuration, format: "unitCouples");

            TeleportingJackalOption = new CustomRoleOption(339, false, CustomOptionType.Neutral, "TeleportingJackalName", RoleClass.TeleportingJackal.color, 1);
            TeleportingJackalPlayerCount = CustomOption.Create(340, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TeleportingJackalOption);
            TeleportingJackalKillCoolDown = CustomOption.Create(341, false, CustomOptionType.Neutral, "JackalCoolDownSetting", 30f, 2.5f, 60f, 2.5f, TeleportingJackalOption, format: "unitSeconds");
            TeleportingJackalUseVent = CustomOption.Create(342, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, TeleportingJackalOption);
            TeleportingJackalIsImpostorLight = CustomOption.Create(343, false, CustomOptionType.Neutral, "MadMateImpostorLightSetting", false, TeleportingJackalOption);
            TeleportingJackalUseSabo = CustomOption.Create(344, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, TeleportingJackalOption);
            TeleportingJackalCoolTime = CustomOption.Create(345, false, CustomOptionType.Neutral, "TeleporterCoolDownSetting", 30f, 2.5f, 60f, 2.5f, TeleportingJackalOption, format: "unitSeconds");
            TeleportingJackalDurationTime = CustomOption.Create(346, false, CustomOptionType.Neutral, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleportingJackalOption, format: "unitSeconds");

            MadMakerOption = new CustomRoleOption(347, true, CustomOptionType.Crewmate, "MadMakerName", RoleClass.MadMaker.color, 1);
            MadMakerPlayerCount = CustomOption.Create(348, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMakerOption);
            MadMakerIsUseVent = CustomOption.Create(349, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadMakerOption);
            MadMakerIsImpostorLight = CustomOption.Create(350, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadMakerOption);

            DemonOption = new CustomRoleOption(351, true, CustomOptionType.Neutral, "DemonName", RoleClass.Demon.color, 1);
            DemonPlayerCount = CustomOption.Create(352, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DemonOption);
            DemonCoolTime = CustomOption.Create(353, true, CustomOptionType.Neutral, "NiceScientistCoolDownSetting", 30f, 2.5f, 60f, 2.5f, DemonOption, format: "unitSeconds");
            DemonIsUseVent = CustomOption.Create(354, true, CustomOptionType.Neutral, "MadMateUseVentSetting", false, DemonOption);
            DemonIsCheckImpostor = CustomOption.Create(355, true, CustomOptionType.Neutral, "MadMateIsCheckImpostorSetting", false, DemonOption);
            DemonIsAliveWin = CustomOption.Create(356, true, CustomOptionType.Neutral, "DemonIsAliveWinSetting", false, DemonOption);

            TaskManagerOption = new CustomRoleOption(357, true, CustomOptionType.Crewmate, "TaskManagerName", RoleClass.TaskManager.color, 1);
            TaskManagerPlayerCount = CustomOption.Create(358, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TaskManagerOption);
            var taskmanageroption = SelectTask.TaskSetting(359, 360, 361, TaskManagerOption, CustomOptionType.Crewmate, true);
            TaskManagerCommonTask = taskmanageroption.Item1;
            TaskManagerShortTask = taskmanageroption.Item2;
            TaskManagerLongTask = taskmanageroption.Item3;

            SeerFriendsOption = new CustomRoleOption(362, false, CustomOptionType.Crewmate, "SeerFriendsName", RoleClass.SeerFriends.color, 1);
            SeerFriendsPlayerCount = CustomOption.Create(363, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerFriendsOption);
            SeerFriendsMode = CustomOption.Create(364, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerFriendsOption);
            SeerFriendsLimitSoulDuration = CustomOption.Create(365, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerFriendsOption);
            SeerFriendsSoulDuration = CustomOption.Create(366, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerFriendsLimitSoulDuration, format: "unitCouples");
            SeerFriendsIsUseVent = CustomOption.Create(367, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, SeerFriendsOption);
            SeerFriendsIsImpostorLight = CustomOption.Create(368, false, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, SeerFriendsOption);
            SeerFriendsIsCheckJackal = CustomOption.Create(369, false, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, SeerFriendsOption);
            var SeerFriendsoption = SelectTask.TaskSetting(371, 372, 373, SeerFriendsIsCheckJackal, CustomOptionType.Crewmate, true);
            SeerFriendsCommonTask = SeerFriendsoption.Item1;
            SeerFriendsShortTask = SeerFriendsoption.Item2;
            SeerFriendsLongTask = SeerFriendsoption.Item3;
            SeerFriendsCheckJackalTask = CustomOption.Create(374, false, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, SeerFriendsIsCheckJackal);

            JackalSeerOption = new CustomRoleOption(375, false, CustomOptionType.Neutral, "JackalSeerName", RoleClass.JackalSeer.color, 1);
            JackalSeerPlayerCount = CustomOption.Create(376, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalSeerOption);
            JackalSeerMode = CustomOption.Create(377, false, CustomOptionType.Neutral, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, JackalSeerOption);
            JackalSeerLimitSoulDuration = CustomOption.Create(378, false, CustomOptionType.Neutral, "SeerLimitSoulDuration", false, JackalSeerOption);
            JackalSeerSoulDuration = CustomOption.Create(379, false, CustomOptionType.Neutral, "SeerSoulDuration", 15f, 0f, 120f, 5f, JackalSeerLimitSoulDuration, format: "unitCouples");
            JackalSeerKillCoolDown = CustomOption.Create(380, false, CustomOptionType.Neutral, "JackalCoolDownSetting", 30f, 2.5f, 60f, 2.5f, JackalSeerOption, format: "unitSeconds");
            JackalSeerUseVent = CustomOption.Create(381, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalSeerOption);
            JackalSeerUseSabo = CustomOption.Create(382, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalSeerOption);
            JackalSeerIsImpostorLight = CustomOption.Create(383, false, CustomOptionType.Neutral, "MadMateImpostorLightSetting", false, JackalSeerOption);
            JackalSeerCreateSidekick = CustomOption.Create(384, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalSeerOption);
            JackalSeerNewJackalCreateSidekick = CustomOption.Create(385, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalSeerCreateSidekick);

            AssassinAndMarineOption = new CustomRoleOption(386, true, CustomOptionType.Impostor, "AssassinAndMarineName", Color.white, 1);
            AssassinPlayerCount = CustomOption.Create(387, true, CustomOptionType.Impostor, "AssassinSettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], AssassinAndMarineOption);
            MarinePlayerCount = CustomOption.Create(388, true, CustomOptionType.Impostor, "MarineSettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AssassinAndMarineOption);

            ArsonistOption = new CustomRoleOption(389, true, CustomOptionType.Neutral, "ArsonistName", RoleClass.Arsonist.color, 1);
            ArsonistPlayerCount = CustomOption.Create(390, true, CustomOptionType.Neutral, "SettingPlayerCountName", AlonePlayers[0], AlonePlayers[1], AlonePlayers[2], AlonePlayers[3], ArsonistOption);
            ArsonistCoolTime = CustomOption.Create(391, true, CustomOptionType.Neutral, "NiceScientistCoolDownSetting", 30f, 2.5f, 60f, 2.5f, ArsonistOption, format: "unitSeconds");
            ArsonistDurationTime = CustomOption.Create(392, true, CustomOptionType.Neutral, "ArsonistDurationTimeSetting", 3f, 0.5f, 10f, 0.5f, ArsonistOption, format: "unitSeconds");
            ArsonistIsUseVent = CustomOption.Create(393, true, CustomOptionType.Neutral, "MadMateUseVentSetting", false, ArsonistOption);

            ChiefOption = new CustomRoleOption(394, false, CustomOptionType.Crewmate, "ChiefName", RoleClass.Chief.color, 1);
            ChiefPlayerCount = CustomOption.Create(395, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ChiefOption);
            ChiefSheriffCoolTime = CustomOption.Create(626, false, CustomOptionType.Crewmate, "SheriffCoolDownSetting", 30f, 2.5f, 60f, 2.5f, ChiefOption, format: "unitSeconds");
            ChiefIsNeutralKill = CustomOption.Create(627, false, CustomOptionType.Crewmate, "SheriffIsKillNewtralSetting", false, ChiefOption);
            ChiefIsLoversKill = CustomOption.Create(628, false, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, ChiefOption);
            ChiefIsMadRoleKill = CustomOption.Create(629, false, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, ChiefOption);
            ChiefKillLimit = CustomOption.Create(630, false, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, ChiefOption, format: "unitSeconds");

            CleanerOption = new CustomRoleOption(396, false, CustomOptionType.Impostor, "CleanerName", RoleClass.Cleaner.color, 1);
            CleanerPlayerCount = CustomOption.Create(397, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CleanerOption);
            CleanerKillCoolTime = CustomOption.Create(398, false, CustomOptionType.Impostor, "CleanerKillCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, CleanerOption, format: "unitSeconds");
            CleanerCoolDown = CustomOption.Create(399, false, CustomOptionType.Impostor, "CleanerCoolDownSetting", 60f, 40f, 70f, 2.5f, CleanerOption, format: "unitSeconds");

            MadCleanerOption = new CustomRoleOption(400, false, CustomOptionType.Crewmate, "MadCleanerName", RoleClass.MadCleaner.color, 1);
            MadCleanerPlayerCount = CustomOption.Create(401, false, CustomOptionType.Crewmate, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MadCleanerOption);
            MadCleanerCoolDown = CustomOption.Create(402, false, CustomOptionType.Crewmate, "CleanerCoolDownSetting", 30f, 2.5f, 60f, 2.5f, MadCleanerOption, format: "unitSeconds");
            MadCleanerIsUseVent = CustomOption.Create(403, false, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MadCleanerOption);
            MadCleanerIsImpostorLight = CustomOption.Create(404, false, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MadCleanerOption);

            MayorFriendsOption = new CustomRoleOption(405, true, CustomOptionType.Crewmate, "MayorFriendsName", RoleClass.MayorFriends.color, 1);
            MayorFriendsPlayerCount = CustomOption.Create(406, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorFriendsOption);
            MayorFriendsIsUseVent = CustomOption.Create(407, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, MayorFriendsOption);
            MayorFriendsIsImpostorLight = CustomOption.Create(408, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, MayorFriendsOption);
            MayorFriendsIsCheckJackal = CustomOption.Create(409, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, MayorFriendsOption);
            var MayorFriendsoption = SelectTask.TaskSetting(410, 411, 412, MayorFriendsIsCheckJackal, CustomOptionType.Crewmate, true);
            MayorFriendsCommonTask = MayorFriendsoption.Item1;
            MayorFriendsShortTask = MayorFriendsoption.Item2;
            MayorFriendsLongTask = MayorFriendsoption.Item3;
            MayorFriendsCheckJackalTask = CustomOption.Create(413, true, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, MayorFriendsIsCheckJackal);
            MayorFriendsVoteCount = CustomOption.Create(414, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorFriendsOption);

            VentMakerOption = new CustomRoleOption(415, false, CustomOptionType.Impostor, "VentMakerName", RoleClass.VentMaker.color, 1);
            VentMakerPlayerCount = CustomOption.Create(416, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VentMakerOption);

            SamuraiOption = new CustomRoleOption(417, true, CustomOptionType.Impostor, "SamuraiName", RoleClass.Samurai.color, 1);
            SamuraiPlayerCount = CustomOption.Create(418, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SamuraiOption);
            SamuraiKillCoolTime = CustomOption.Create(419, true, CustomOptionType.Impostor, "SamuraiKillCoolSetting", 30f, 2.5f, 60f, 2.5f, SamuraiOption);
            SamuraiSwordCoolTime = CustomOption.Create(420, true, CustomOptionType.Impostor, "SamuraiSwordCoolSetting", 50f, 30f, 70f, 2.5f, SamuraiOption);
            SamuraiVent = CustomOption.Create(421, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, SamuraiOption);
            SamuraiSabo = CustomOption.Create(422, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, SamuraiOption);
            SamuraiScope = CustomOption.Create(423, true, CustomOptionType.Impostor, "SamuraiScopeSetting", 1f, 0.5f, 3f, 0.5f, SamuraiOption);

            EvilHackerOption = new CustomRoleOption(424, false, CustomOptionType.Impostor, "EvilHackerName", RoleClass.EvilHacker.color, 1);
            EvilHackerPlayerCount = CustomOption.Create(425, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilHackerOption);
            EvilHackerMadmateSetting = CustomOption.Create(426, false, CustomOptionType.Impostor, "EvilHackerMadmateSetting", false, EvilHackerOption);

            GhostMechanicOption = new CustomRoleOption(427, false, CustomOptionType.Crewmate, "GhostMechanicName", RoleClass.GhostMechanic.color, 1);
            GhostMechanicPlayerCount = CustomOption.Create(428, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GhostMechanicOption);
            GhostMechanicRepairLimit = CustomOption.Create(429, false, CustomOptionType.Crewmate, "GhostMechanicRepairLimitSetting", 1f, 1f, 30f, 1f, GhostMechanicOption);

            HauntedWolfOption = new CustomRoleOption(530, true, CustomOptionType.Crewmate, "HauntedWolfName", RoleClass.HauntedWolf.color, 1);
            HauntedWolfPlayerCount = CustomOption.Create(531, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HauntedWolfOption);

            PositionSwapperOption = new CustomRoleOption(609, false, CustomOptionType.Impostor, "PositionSwapperName", RoleClass.PositionSwapper.color, 1);
            PositionSwapperPlayerCount = CustomOption.Create(610, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PositionSwapperOption);
            PositionSwapperSwapCount = CustomOption.Create(611, false, CustomOptionType.Impostor, "SettingPositionSwapperSwapCountName", 1f, 0f, 99f, 1f, PositionSwapperOption);
            PositionSwapperCoolTime = CustomOption.Create(616, false, CustomOptionType.Impostor, "SettingPositionSwapperSwapCoolTimeName", 2.5f, 2.5f, 90f, 2.5f, PositionSwapperOption);

            TunaOption = new CustomRoleOption(552, true, CustomOptionType.Neutral, "TunaName", RoleClass.Tuna.color, 1);
            TunaPlayerCount = CustomOption.Create(553, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TunaOption);
            TunaStoppingTime = CustomOption.Create(554, true, CustomOptionType.Neutral, "TunaStoppingTimeSetting", 1f, 1f, 3f, 1f, TunaOption);
            TunaIsUseVent = CustomOption.Create(555, true, CustomOptionType.Neutral, "MadMateUseVentSetting", false, TunaOption);
            TunaIsAddWin = CustomOption.Create(671, true, CustomOptionType.Neutral, "TunaAddWinSetting", false, TunaOption);

            MafiaOption = new CustomRoleOption(602, true, CustomOptionType.Impostor, "MafiaName", RoleClass.Mafia.color, 1);
            MafiaPlayerCount = CustomOption.Create(603, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MafiaOption);

            BlackCatOption = new CustomRoleOption(556, true, CustomOptionType.Crewmate, "BlackCatName", RoleClass.ImpostorRed, 1);
            BlackCatPlayerCount = CustomOption.Create(557, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BlackCatOption);
            BlackCatNotImpostorExiled = CustomOption.Create(675, true, CustomOptionType.Crewmate, "NotImpostorExiled", false, BlackCatOption);
            BlackCatIsCheckImpostor = CustomOption.Create(558, true, CustomOptionType.Crewmate, "MadMateIsCheckImpostorSetting", false, BlackCatOption);
            var blackcatoption = SelectTask.TaskSetting(559, 560, 561, BlackCatIsCheckImpostor, CustomOptionType.Crewmate, true);
            BlackCatCommonTask = blackcatoption.Item1;
            BlackCatShortTask = blackcatoption.Item2;
            BlackCatLongTask = blackcatoption.Item3;
            //MadMateIsNotTask = madmateoption.Item4;
            BlackCatCheckImpostorTask = CustomOption.Create(562, true, CustomOptionType.Crewmate, "MadMateCheckImpostorTaskSetting", rates4, BlackCatIsCheckImpostor);
            BlackCatIsUseVent = CustomOption.Create(563, true, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, BlackCatOption);
            BlackCatIsImpostorLight = CustomOption.Create(564, true, CustomOptionType.Crewmate, "MadMateImpostorLightSetting", false, BlackCatOption);

            SecretlyKillerOption = new CustomRoleOption(607, false, CustomOptionType.Impostor, "SecretlyKillerName", RoleClass.SecretlyKiller.color, 1);
            SecretlyKillerPlayerCount = CustomOption.Create(608, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SecretlyKillerOption);
            SecretlyKillerKillCoolTime = CustomOption.Create(632, false, CustomOptionType.Impostor, "SheriffCoolDownSetting", 2.5f, 2.5f, 60f, 2.5f, SecretlyKillerOption);
            SecretlyKillerIsKillCoolTimeChange = CustomOption.Create(633, false, CustomOptionType.Impostor, "SettingCoolCharge", true, SecretlyKillerOption);
            SecretlyKillerIsBlackOutKillCharge = CustomOption.Create(634, false, CustomOptionType.Impostor, "SettingBlackoutCharge", false, SecretlyKillerOption);
            SecretlyKillerSecretKillLimit = CustomOption.Create(635, false, CustomOptionType.Impostor, "SettingLimitName", 1f, 0f, 99f, 1f, SecretlyKillerOption);
            SecretlyKillerSecretKillCoolTime = CustomOption.Create(636, false, CustomOptionType.Impostor, "NiceScientistCoolDownSetting", 45f, 2.5f, 60f, 2.5f, SecretlyKillerOption);

            SpyOption = new CustomRoleOption(614, true, CustomOptionType.Crewmate, "SpyName", RoleClass.Spy.color, 1);
            SpyPlayerCount = CustomOption.Create(615, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpyOption);
            SpyCanUseVent = CustomOption.Create(617, true, CustomOptionType.Crewmate, "JesterIsVentSetting", false, SpyOption);

            KunoichiOption = new CustomRoleOption(638, false, CustomOptionType.Impostor, "KunoichiName", RoleClass.Kunoichi.color, 1);
            KunoichiPlayerCount = CustomOption.Create(639, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], KunoichiOption);
            KunoichiCoolTime = CustomOption.Create(640, false, CustomOptionType.Impostor, "KunoichiCoolTime", 2.5f, 0f, 15f, 0.5f, KunoichiOption);
            KunoichiKillKunai = CustomOption.Create(641, false, CustomOptionType.Impostor, "KunoichiKillKunai", 10f, 1f, 20f, 1f, KunoichiOption);
            KunoichiIsHide = CustomOption.Create(642, false, CustomOptionType.Impostor, "KunoichiIsHide", true, KunoichiOption);
            KunoichiIsWaitAndPressTheButtonToHide = CustomOption.Create(907, false, CustomOptionType.Impostor, "KunoichiIsWaitAndPressTheButtonToHide", true, KunoichiIsHide);
            KunoichiHideTime = CustomOption.Create(643, false, CustomOptionType.Impostor, "KunoichiHideTime", 3f, 0.5f, 10f, 0.5f, KunoichiIsHide);
            KunoichiHideKunai = CustomOption.Create(644, false, CustomOptionType.Impostor, "KunoichiHideKunai", false, KunoichiIsHide);

            DoubleKillerOption = new CustomRoleOption(647, false, CustomOptionType.Impostor, "DoubleKillerName", RoleClass.DoubleKiller.color, 1);
            DoubleKillerPlayerCount = CustomOption.Create(648, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoubleKillerOption);
            MainKillCoolTime = CustomOption.Create(649, false, CustomOptionType.Impostor, "MainCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoubleKillerOption, format: "unitSeconds");
            SubKillCoolTime = CustomOption.Create(650, false, CustomOptionType.Impostor, "SubCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoubleKillerOption, format: "unitSeconds");
            DoubleKillerSabo = CustomOption.Create(651, false, CustomOptionType.Impostor, "DoubleKillerSaboSetting", false, DoubleKillerOption);
            DoubleKillerVent = CustomOption.Create(652, false, CustomOptionType.Impostor, "MinimalistVentSetting", false, DoubleKillerOption);

            SmasherOption = new CustomRoleOption(653, false, CustomOptionType.Impostor, "SmasherName", RoleClass.Smasher.color, 1);
            SmasherPlayerCount = CustomOption.Create(654, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SmasherOption);
            SmasherKillCoolTime = CustomOption.Create(655, false, CustomOptionType.Impostor, "KillCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, SmasherOption, format: "unitSeconds");

            SuicideWisherOption = new CustomRoleOption(678, true, CustomOptionType.Impostor, "SuicideWisherName", RoleClass.SuicideWisher.color, 1);
            SuicideWisherPlayerCount = CustomOption.Create(679, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SuicideWisherOption);

            NeetOption = new CustomRoleOption(680, false, CustomOptionType.Neutral, "NeetName", RoleClass.Neet.color, 1);
            NeetPlayerCount = CustomOption.Create(659, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NeetOption);
            NeetIsAddWin = CustomOption.Create(683, false, CustomOptionType.Neutral, "TunaAddWinSetting", false, NeetOption);

            FastMakerOption = new CustomRoleOption(676, true, CustomOptionType.Impostor, "FastMakerName", RoleClass.FastMaker.color, 1);
            FastMakerPlayerCount = CustomOption.Create(661, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FastMakerOption);

            ToiletFanOption = new CustomRoleOption(656, true, CustomOptionType.Crewmate, "ToiletFanName", RoleClass.ToiletFan.color, 1);
            ToiletFanPlayerCount = CustomOption.Create(657, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ToiletFanOption);
            ToiletFanCoolTime = CustomOption.Create(658, true, CustomOptionType.Crewmate, "ToiletCoolDownSetting", 30f, 0f, 60f, 2.5f, ToiletFanOption);

            SatsumaAndImoOption = new CustomRoleOption(953, true, CustomOptionType.Crewmate, "SatsumaAndImoName", RoleClass.SatsumaAndImo.color, 1);
            SatsumaAndImoPlayerCount = CustomOption.Create(800, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SatsumaAndImoOption);

            EvilButtonerOption = new CustomRoleOption(801, true, CustomOptionType.Impostor, "EvilButtonerName", RoleClass.EvilButtoner.color, 1);
            EvilButtonerPlayerCount = CustomOption.Create(802, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilButtonerOption);
            EvilButtonerCoolTime = CustomOption.Create(803, false, CustomOptionType.Impostor, "ButtonerCoolDownSetting", 20f, 2.5f, 60f, 2.5f, EvilButtonerOption, format: "unitSeconds");//クールタイムはSHR未対応の為false
            EvilButtonerCount = CustomOption.Create(804, true, CustomOptionType.Impostor, "ButtonerCountSetting", 1f, 1f, 10f, 1f, EvilButtonerOption);

            NiceButtonerOption = new CustomRoleOption(805, true, CustomOptionType.Crewmate, "NiceButtonerName", RoleClass.NiceButtoner.color, 1);
            NiceButtonerPlayerCount = CustomOption.Create(806, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceButtonerOption);
            NiceButtonerCoolTime = CustomOption.Create(807, false, CustomOptionType.Crewmate, "ButtonerCoolDownSetting", 20f, 2.5f, 60f, 2.5f, NiceButtonerOption, format: "unitSeconds");//クールタイムはSHR未対応の為false
            NiceButtonerCount = CustomOption.Create(808, true, CustomOptionType.Crewmate, "ButtonerCountSetting", 1f, 1f, 10f, 1f, NiceButtonerOption);

            SpelunkerOption = new CustomRoleOption(809, false, CustomOptionType.Neutral, "SpelunkerName", RoleClass.Spelunker.color, 1);
            SpelunkerPlayerCount = CustomOption.Create(810, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpelunkerOption);
            SpelunkerVentDeathChance = CustomOption.Create(811, false, CustomOptionType.Neutral, "SpelunkerVentDeathChance", rates, SpelunkerOption);
            SpelunkerLadderDeadChance = CustomOption.Create(812, false, CustomOptionType.Neutral, "LadderDeadChance", rates, SpelunkerOption);
            SpelunkerIsDeathCommsOrPowerdown = CustomOption.Create(813, false, CustomOptionType.Neutral, "SpelunkerIsDeathCommsOrPowerdown", true, SpelunkerOption);
            SpelunkerDeathCommsOrPowerdownTime = CustomOption.Create(814, false, CustomOptionType.Neutral, "SpelunkerDeathCommsOrPowerdownTime", 20f, 0f, 120f, 2.5f, SpelunkerIsDeathCommsOrPowerdown);
            SpelunkerLiftDeathChance = CustomOption.Create(815, false, CustomOptionType.Neutral, "SpelunkerLiftDeathChance", rates, SpelunkerOption);
            SpelunkerDoorOpenChance = CustomOption.Create(816, false, CustomOptionType.Neutral, "SpelunkerDoorOpenChance", rates, SpelunkerOption);

            FinderOption = new CustomRoleOption(817, false, CustomOptionType.Impostor, "FinderName", RoleClass.Finder.color, 1);
            FinderPlayerCount = CustomOption.Create(818, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FinderOption);
            FinderCheckMadmateSetting = CustomOption.Create(819, false, CustomOptionType.Impostor, "FinderCheckMadmateSetting", 3f, 1f, 15f, 1f, FinderOption);

            RevolutionistAndDictatorOption = new CustomRoleOption(820, false, CustomOptionType.Neutral, "RevolutionistAndDictatorName", Color.white, 1);
            RevolutionistPlayerCount = CustomOption.Create(821, false, CustomOptionType.Neutral, "SettingRevolutionistPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RevolutionistAndDictatorOption);
            DictatorPlayerCount = CustomOption.Create(822, false, CustomOptionType.Neutral, "SettingDictatorPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RevolutionistAndDictatorOption);
            DictatorVoteCount = CustomOption.Create(823, false, CustomOptionType.Neutral, "DictatorVoteCount", 2f, 1f, 100f, 1f, RevolutionistAndDictatorOption);
            DictatorSubstituteExile = CustomOption.Create(824, false, CustomOptionType.Neutral, "DictatorSubExile", false, RevolutionistAndDictatorOption);
            DictatorSubstituteExileLimit = CustomOption.Create(825, false, CustomOptionType.Neutral, "DictatorSubExileLimit", 1f, 1f, 15f, 1f, DictatorSubstituteExile);
            RevolutionistCoolTime = CustomOption.Create(826, false, CustomOptionType.Neutral, "RevolutionCoolTime", 10f, 2.5f, 60f, 2.5f, RevolutionistAndDictatorOption);
            RevolutionistTouchTime = CustomOption.Create(827, false, CustomOptionType.Neutral, "RevolutionTouchTime", 1f, 0f, 10f, 0.5f, RevolutionistAndDictatorOption);
            RevolutionistAddWin = CustomOption.Create(828, false, CustomOptionType.Neutral, "RevolutionistAddWin", false, RevolutionistAndDictatorOption);
            RevolutionistAddWinIsAlive = CustomOption.Create(829, false, CustomOptionType.Neutral, "RevolutionistAddWinIsAlive", true, RevolutionistAddWin);

            SuicidalIdeationOption = new CustomRoleOption(830, false, CustomOptionType.Neutral, "SuicidalIdeationName", RoleClass.SuicidalIdeation.color, 1);
            SuicidalIdeationPlayerCount = CustomOption.Create(831, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SuicidalIdeationOption);
            SuicidalIdeationWinText = CustomOption.Create(832, false, CustomOptionType.Neutral, "SuicidalIdeationWinTextSetting", false, SuicidalIdeationOption);
            SuicidalIdeationTimeLeft = CustomOption.Create(833, false, CustomOptionType.Neutral, "SuicidalIdeationTimeLeftSetting", 90f, 30f, 600f, 5f, SuicidalIdeationOption, format: "unitSeconds");
            SuicidalIdeationAddTimeLeft = CustomOption.Create(834, false, CustomOptionType.Neutral, "SuicidalIdeationAddTimeLeftSetting", 20f, 0f, 300f, 5f, SuicidalIdeationOption, format: "unitSeconds");
            SuicidalIdeationFallProbability = CustomOption.Create(835, false, CustomOptionType.Neutral, "SuicidalIdeationFallProbabilitySetting", rates, SuicidalIdeationOption);
            var SuicidalIdeationoption = SelectTask.TaskSetting(836, 837, 838, SuicidalIdeationOption, CustomOptionType.Neutral, false);
            SuicidalIdeationCommonTask = SuicidalIdeationoption.Item1;
            SuicidalIdeationShortTask = SuicidalIdeationoption.Item2;
            SuicidalIdeationLongTask = SuicidalIdeationoption.Item3;
            
            NunOption = new CustomRoleOption(958, false, CustomOptionType.Impostor, "NunName",RoleClass.Nun.color, 1);
            NunPlayerCount = CustomOption.Create(959, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], NunOption);
            NunCoolTime = CustomOption.Create(960, false, CustomOptionType.Impostor, "NiceScientistCoolDownSetting", 20f, 2.5f, 60f, 2.5f, NunOption);


            PartTimerOption = new CustomRoleOption(961, false, CustomOptionType.Neutral, "PartTimerName",RoleClass.PartTimer.color, 1);
            PartTimerPlayerCount = CustomOption.Create(962, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PartTimerOption);
            PartTimerDeathTurn = CustomOption.Create(963, false, CustomOptionType.Neutral, "PartTimerDeathTurn", 3f, 0f, 15f, 1f, PartTimerOption);
            PartTimerCoolTime = CustomOption.Create(964, false, CustomOptionType.Neutral, "NiceScientistCoolDownSetting", 20f, 2.5f, 60f, 2.5f, PartTimerOption);
            PartTimerIsCheckTargetRole = CustomOption.Create(965, false, CustomOptionType.Neutral, "PartTimerIsCheckTargetRole", true, PartTimerOption);

            SluggerOption = new CustomRoleOption(901, false, CustomOptionType.Impostor, "SluggerName",RoleClass.Slugger.color, 1);
            SluggerPlayerCount = CustomOption.Create(902, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SluggerOption);
            SluggerChargeTime = CustomOption.Create(903, false, CustomOptionType.Impostor, "SluggerChargeTime", 3f, 0f, 30f, 0.5f, SluggerOption);
            SluggerCoolTime = CustomOption.Create(904, false, CustomOptionType.Impostor, "NiceScientistCoolDownSetting", 20f, 2.5f, 60f, 2.5f, SluggerOption);
            SluggerIsMultiKill = CustomOption.Create(905, false, CustomOptionType.Impostor, "SluggerIsMultiKill", false, SluggerOption);

            PainterOption = new CustomRoleOption(941, false, CustomOptionType.Crewmate, "PainterName", RoleClass.Painter.color, 1);
            PainterPlayerCount = CustomOption.Create(942, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PainterOption);
            PainterCoolTime = CustomOption.Create(952, false, CustomOptionType.Crewmate, "NiceScientistCoolDownSetting", 20f, 2.5f, 60f, 2.5f, PainterOption);
            PainterIsTaskCompleteFootprint = CustomOption.Create(943, false, CustomOptionType.Crewmate, "PainterIsTaskCompleteFootprint", true, PainterOption);
            PainterIsSabotageRepairFootprint = CustomOption.Create(944, false, CustomOptionType.Crewmate, "PainterIsSabotageRepairFootprint", true, PainterOption);
            PainterIsInVentFootprint = CustomOption.Create(945, false, CustomOptionType.Crewmate, "PainterIsInVentFootprint", true, PainterOption);
            PainterIsExitVentFootprint = CustomOption.Create(946, false, CustomOptionType.Crewmate, "PainterIsExitVentFootprint", true, PainterOption);
            PainterIsCheckVitalFootprint = CustomOption.Create(947, false, CustomOptionType.Crewmate, "PainterIsCheckVitalFootprint", false, PainterOption);
            PainterIsCheckAdminFootprint = CustomOption.Create(948, false, CustomOptionType.Crewmate, "PainterIsCheckAdminFootprint", false, PainterOption);
            PainterIsDeathFootprint = CustomOption.Create(949, false, CustomOptionType.Crewmate, "PainterIsDeathFootprint", true, PainterOption);
            PainterIsDeathFootprintBig = CustomOption.Create(950, false, CustomOptionType.Crewmate, "PainterIsDeathFootprintBig", true, PainterIsDeathFootprint);
            PainterIsFootprintMeetingDestroy = CustomOption.Create(951, false, CustomOptionType.Crewmate, "PainterIsFootprintMeetingDestroy", true, PainterOption);

            HitmanOption = new CustomRoleOption(839, false, CustomOptionType.Neutral, "HitmanName",RoleClass.Hitman.color, 1);
            HitmanPlayerCount = CustomOption.Create(840, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HitmanOption);
            HitmanKillCoolTime = CustomOption.Create(841, false, CustomOptionType.Neutral, "SheriffCoolDownSetting", 20f, 0f, 120f, 2.5f, HitmanOption);
            HitmanChangeTargetTime = CustomOption.Create(842, false, CustomOptionType.Neutral, "HitmanChangeTargetTime", 20f, 0f, 240f, 2.5f, HitmanOption);
            HitmanIsOutMission = CustomOption.Create(843, false, CustomOptionType.Neutral, "HitmanIsOutMission", true, HitmanOption);
            HitmanOutMissionLimit = CustomOption.Create(845, false, CustomOptionType.Neutral, "HitmanOutMissionLimit", 5f, 1f, 30f, 1f, HitmanIsOutMission);
            HitmanWinKillCount = CustomOption.Create(846, false, CustomOptionType.Neutral, "HitmanWinKillCount", 5f, 1f, 15f, 1f, HitmanOption);
            HitmanIsArrowView = CustomOption.Create(847, false, CustomOptionType.Neutral, "HitmanIsTargetArrow", true, HitmanOption);
            HitmanArrowUpdateTime = CustomOption.Create(848, false, CustomOptionType.Neutral, "HitmanUpdateTargetArrowTime", 0f, 0f, 120f, 2.5f, HitmanIsArrowView);

            MatryoshkaOption = new CustomRoleOption(849, false, CustomOptionType.Impostor, "MatryoshkaName",RoleClass.Matryoshka.color, 1);
            MatryoshkaPlayerCount = CustomOption.Create(850, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MatryoshkaOption);
            MatryoshkaWearLimit = CustomOption.Create(851, false, CustomOptionType.Impostor, "MatryoshkaWearLimit", 3f, 1f, 15f, 1f, MatryoshkaOption);
            MatryoshkaWearReport = CustomOption.Create(852, false, CustomOptionType.Impostor, "MatryoshkaWearReport", true, MatryoshkaOption);
            MatryoshkaWearTime = CustomOption.Create(853, false, CustomOptionType.Impostor, "MatryoshkaWearTime", 7.5f, 0.5f, 60f, 0.5f, MatryoshkaOption);
            MatryoshkaAddKillCoolTime = CustomOption.Create(854, false, CustomOptionType.Impostor, "MatryoshkaAddKillCoolTime", 2.5f, 0f, 30f, 0.5f, MatryoshkaOption);
            MatryoshkaCoolTime = CustomOption.Create(855, false, CustomOptionType.Impostor, "NiceScientistCoolDownSetting", 30f, 0f, 180f, 2.5f, MatryoshkaOption);

            SeeThroughPersonOption = new CustomRoleOption(864, false, CustomOptionType.Crewmate, "SeeThroughPersonName",RoleClass.SeeThroughPerson.color, 1);
            SeeThroughPersonPlayerCount = CustomOption.Create(865, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeeThroughPersonOption);

            PhotographerOption = new CustomRoleOption(866, false, CustomOptionType.Neutral, "PhotographerName",RoleClass.Photographer.color, 1);
            PhotographerPlayerCount = CustomOption.Create(867, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PhotographerOption);
            PhotographerCoolTime = CustomOption.Create(868,false,CustomOptionType.Neutral, "NiceScientistCoolDownSetting", 20f, 2.5f, 60f, 2.5f, PhotographerOption);
            PhotographerIsBonus = CustomOption.Create(869, false, CustomOptionType.Neutral, "PhotographerIsBonus", true, PhotographerOption);
            PhotographerBonusCount = CustomOption.Create(870, false, CustomOptionType.Neutral, "PhotographerBonusCount", 5f, 1f, 15f, 1f, PhotographerIsBonus);
            PhotographerBonusCoolTime = CustomOption.Create(871, false, CustomOptionType.Neutral, "PhotographerBonusCoolTime", 20f, 2.5f, 60f, 2.5f, PhotographerIsBonus);
            PhotographerIsImpostorVision = CustomOption.Create(872, false, CustomOptionType.Neutral, "PhotographerIsImpostorVision", false, PhotographerOption);
            PhotographerIsNotification = CustomOption.Create(873, false, CustomOptionType.Neutral, "PhotographerIsNotification", true, PhotographerOption);

            StefinderOption = new CustomRoleOption(876, false, CustomOptionType.Neutral, "StefinderName",RoleClass.Stefinder.color, 1);
            StefinderPlayerCount = CustomOption.Create(877, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StefinderOption);
            StefinderKillCoolDown = CustomOption.Create(878, false, CustomOptionType.Neutral, "StefinderKillCoolDownSetting", 30f, 0f, 120f, 2.5f, StefinderOption, format: "unitSeconds");
            StefinderVent = CustomOption.Create(879, false, CustomOptionType.Neutral, "StefinderVentSetting", false, StefinderOption);
            StefinderSabo = CustomOption.Create(880, false, CustomOptionType.Neutral, "StefinderSaboSetting", false, StefinderOption);
            StefinderSoloWin = CustomOption.Create(881, false, CustomOptionType.Neutral, "StefinderSoloWinSetting", false, StefinderOption);

            PsychometristOption = new CustomRoleOption(883, false, CustomOptionType.Crewmate, "PsychometristName", RoleClass.Psychometrist.color, 1);
            PsychometristPlayerCount = CustomOption.Create(884, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PsychometristOption);
            PsychometristCoolTime = CustomOption.Create(885, false, CustomOptionType.Crewmate, "NiceScientistCoolDownSetting", 20f, 2.5f, 60f, 2.5f, PsychometristOption);
            PsychometristReadTime = CustomOption.Create(886, false, CustomOptionType.Crewmate, "PsychometristReadTime", 5f, 0f, 15f, 0.5f, PsychometristOption);
            PsychometristIsCheckDeathTime = CustomOption.Create(887, false, CustomOptionType.Crewmate, "PsychometristIsCheckDeathTime", true, PsychometristOption);
            PsychometristDeathTimeDeviation = CustomOption.Create(888, false, CustomOptionType.Crewmate, "PsychometristDeathTimeDeviation", 3f, 0f, 30f, 1f, PsychometristIsCheckDeathTime);
            PsychometristIsCheckDeathReason = CustomOption.Create(889, false, CustomOptionType.Crewmate, "PsychometristIsCheckDeathReason", true, PsychometristOption);
            PsychometristIsCheckFootprints = CustomOption.Create(890, false, CustomOptionType.Crewmate, "PsychometristIsCheckFootprints", true, PsychometristOption);
            PsychometristCanCheckFootprintsTime = CustomOption.Create(891, false, CustomOptionType.Crewmate, "PsychometristCanCheckFootprintsTime", 7.5f, 0.5f, 60f, 0.5f, PsychometristIsCheckFootprints);
            PsychometristIsReportCheckedDeadBody = CustomOption.Create(892, false, CustomOptionType.Crewmate, "PsychometristIsReportCheckedDeadBody", false, PsychometristOption);

            //表示設定

            QuarreledOption = CustomOption.Create(432, true, CustomOptionType.Neutral, Cs(RoleClass.Quarreled.color, "QuarreledName"), false, null, isHeader: true);
            QuarreledTeamCount = CustomOption.Create(433, true, CustomOptionType.Neutral, "QuarreledTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], QuarreledOption);
            QuarreledOnlyCrewMate = CustomOption.Create(434, true, CustomOptionType.Neutral, "QuarreledOnlyCrewMateSetting", false, QuarreledOption);

            LoversOption = CustomOption.Create(435, true, CustomOptionType.Neutral, Cs(RoleClass.Lovers.color, "LoversName"), false, null, isHeader: true);
            LoversTeamCount = CustomOption.Create(436, true, CustomOptionType.Neutral, "LoversTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], LoversOption);
            LoversPar = CustomOption.Create(437, true, CustomOptionType.Neutral, "LoversParSetting", rates, LoversOption);
            LoversOnlyCrewMate = CustomOption.Create(438, true, CustomOptionType.Neutral, "LoversOnlyCrewMateSetting", false, LoversOption);
            LoversSingleTeam = CustomOption.Create(439, true, CustomOptionType.Neutral, "LoversSingleTeamSetting", true, LoversOption);
            LoversSameDie = CustomOption.Create(440, true, CustomOptionType.Neutral, "LoversSameDieSetting", true, LoversOption);
            LoversAliveTaskCount = CustomOption.Create(441, true, CustomOptionType.Neutral, "LoversAliveTaskCountSetting", false, LoversOption);
            LoversDuplicationQuarreled = CustomOption.Create(442, true, CustomOptionType.Neutral, "LoversDuplicationQuarreledSetting", true, LoversOption);
            var loversoption = SelectTask.TaskSetting(443, 444, 445, LoversOption, CustomOptionType.Neutral, true);
            LoversCommonTask = loversoption.Item1;
            LoversShortTask = loversoption.Item2;
            LoversLongTask = loversoption.Item3;

            SuperNewRolesPlugin.Logger.LogInfo("設定のidのMax:" + CustomOption.Max);
            SuperNewRolesPlugin.Logger.LogInfo("設定数:" + CustomOption.options.Count);
        }
    }
}