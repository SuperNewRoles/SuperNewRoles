using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;

namespace SuperNewRoles.Modules;

public class CustomOptionHolder
{
    public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

    public static string[] rates4 = new string[] { "0%", "25%", "50%", "75%", "100%" };
    private static List<string> presetList()
    {
        var tmp = new List<string>();
        for (int i = 1; i < 11; i++)
        {
            tmp.Add($"{ModTranslation.GetString("preset")}{i}");
        }
        return tmp;
    }
    public static string[] presets = presetList().ToArray();
    public static CustomOption presetSelection;

    public static CustomOption specialOptions;
    public static CustomOption hideSettings;

    public static CustomOption crewmateRolesCountMax;
    public static CustomOption crewmateGhostRolesCountMax;
    public static CustomOption impostorRolesCountMax;
    public static CustomOption impostorGhostRolesCountMax;
    public static CustomOption neutralRolesCountMax;
    public static CustomOption neutralGhostRolesCountMax;

    public static CustomOption enableAgartha;

    public static CustomOption IsDebugMode;
    public static CustomOption DebugModeFastStart;
    public static CustomOption IsMurderPlayerAnnounce;

    public static CustomOption DisconnectNotPCOption;

    public static CustomOption IsOldMode;

    public static CustomOption DetectiveRate;
    public static CustomOption DetectivePlayerCount;

    public static CustomOption MadRolesCanFixComms;
    public static CustomOption MadRolesCanFixElectrical;
    public static CustomOption MadRolesCanVentMove;

    public static CustomRoleOption SoothSayerOption;
    public static CustomOption SoothSayerPlayerCount;
    public static CustomOption SoothSayerDisplayMode;
    public static CustomOption SoothSayerMaxCount;
    public static CustomOption SoothSayerFirstWhiteOption;

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
    public static CustomOption SheriffAlwaysKills;
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
    public static CustomOption RemoteSheriffAlwaysKills;
    public static CustomOption RemoteSheriffMadRoleKill;
    public static CustomOption RemoteSheriffNeutralKill;
    public static CustomOption RemoteSheriffFriendRolesKill;
    public static CustomOption RemoteSheriffLoversKill;
    public static CustomOption RemoteSheriffQuarreledKill;
    public static CustomOption RemoteSheriffKillMaxCount;
    public static CustomOption RemoteSheriffIsKillTeleportSetting;

    public static CustomRoleOption MeetingSheriffOption;
    public static CustomOption MeetingSheriffPlayerCount;
    public static CustomOption MeetingSheriffAlwaysKills;
    public static CustomOption MeetingSheriffMadRoleKill;
    public static CustomOption MeetingSheriffNeutralKill;
    public static CustomOption MeetingSheriffFriendsRoleKill;
    public static CustomOption MeetingSheriffLoversKill;
    public static CustomOption MeetingSheriffQuarreledKill;
    public static CustomOption MeetingSheriffKillMaxCount;
    public static CustomOption MeetingSheriffOneMeetingMultiKill;

    public static CustomRoleOption JackalOption;
    public static CustomOption JackalPlayerCount;
    public static CustomOption JackalKillCooldown;
    public static CustomOption JackalUseVent;
    public static CustomOption JackalUseSabo;
    public static CustomOption JackalIsImpostorLight;
    public static CustomOption JackalCreateFriend;
    public static CustomOption JackalCreateSidekick;
    public static CustomOption JackalSKCooldown;
    public static CustomOption JackalNewJackalCreateSidekick;

    public static CustomRoleOption TeleporterOption;
    public static CustomOption TeleporterPlayerCount;
    public static CustomOption TeleporterCoolTime;
    public static CustomOption TeleporterDurationTime;

    public static CustomRoleOption SpiritMediumOption;
    public static CustomOption SpiritMediumPlayerCount;
    public static CustomOption SpiritMediumIsAutoMode;
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
    public static CustomOption TaskerCommonTask;
    public static CustomOption TaskerShortTask;
    public static CustomOption TaskerLongTask;
    public static CustomOption TaskerIsKillCoolTaskNow;
    public static CustomOption TaskerCanKill;

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

    public static CustomRoleOption NiceGuesserOption;
    public static CustomOption NiceGuesserPlayerCount;
    public static CustomOption NiceGuesserShortOneMeetingCount;
    public static CustomOption NiceGuesserShortMaxCount;
    public static CustomOption NiceGuesserCanShotCrew;

    public static CustomRoleOption EvilGuesserOption;
    public static CustomOption EvilGuesserPlayerCount;
    public static CustomOption EvilGuesserShortOneMeetingCount;
    public static CustomOption EvilGuesserShortMaxCount;
    public static CustomOption EvilGuesserCanShotCrew;

    public static CustomRoleOption VultureOption;
    public static CustomOption VulturePlayerCount;
    public static CustomOption VultureCooldown;
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

    public static CustomRoleOption MadmateOption;
    public static CustomOption MadmatePlayerCount;
    public static CustomOption MadmateIsCheckImpostor;
    public static CustomOption MadmateCommonTask;
    public static CustomOption MadmateShortTask;
    public static CustomOption MadmateLongTask;
    public static CustomOption MadmateCheckImpostorTask;
    public static CustomOption MadmateIsUseVent;
    public static CustomOption MadmateIsImpostorLight;

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
    public static CustomOption SelfBomberBombCoolTime;

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
    public static CustomOption NiceRedRidinIsKillerDeathRevive;

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

    public static CustomRoleOption BakeryOption;
    public static CustomOption BakeryPlayerCount;

    public static CustomRoleOption MadJesterOption;
    public static CustomOption MadJesterPlayerCount;
    public static CustomOption MadJesterIsUseVent;
    public static CustomOption MadJesterIsImpostorLight;
    public static CustomOption IsMadJesterTaskClearWin;
    public static CustomOption MadJesterCommonTask;
    public static CustomOption MadJesterShortTask;
    public static CustomOption MadJesterLongTask;
    public static CustomOption MadJesterIsCheckImpostor;
    public static CustomOption MadJesterCheckImpostorTask;

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
    public static CustomOption CelebrityIsTaskPhaseFlash;
    public static CustomOption CelebrityIsFlashWhileAlivingOnly;

    public static CustomRoleOption NocturnalityOption;
    public static CustomOption NocturnalityPlayerCount;

    public static CustomRoleOption ObserverOption;
    public static CustomOption ObserverPlayerCount;

    public static CustomRoleOption VampireOption;
    public static CustomOption VampirePlayerCount;
    public static CustomOption VampireKillDelay;
    public static CustomOption VampireViewBloodStainsTurn;
    public static CustomOption VampireCanCreateDependents;
    public static CustomOption VampireCreateDependentsCoolTime;
    public static CustomOption VampireDependentsKillCoolTime;
    public static CustomOption VampireDependentsCanVent;

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
    public static CustomOption EvilSeerMadmateSetting;

    public static CustomRoleOption TeleportingJackalOption;
    public static CustomOption TeleportingJackalPlayerCount;
    public static CustomOption TeleportingJackalKillCooldown;
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
    public static CustomOption JackalSeerKillCooldown;
    public static CustomOption JackalSeerUseVent;
    public static CustomOption JackalSeerUseSabo;
    public static CustomOption JackalSeerIsImpostorLight;
    public static CustomOption JackalSeerCreateSidekick;
    public static CustomOption JackalSeerCreateFriend;
    public static CustomOption JackalSeerSKCooldown;
    public static CustomOption JackalSeerNewJackalCreateSidekick;

    public static CustomRoleOption AssassinAndMarlinOption;
    public static CustomOption AssassinPlayerCount;
    public static CustomOption AssassinViewVote;
    public static CustomOption MarlinPlayerCount;
    public static CustomOption MarlinViewVote;

    public static CustomRoleOption ChiefOption;
    public static CustomOption ChiefPlayerCount;
    public static CustomOption ChiefSheriffCoolTime;
    public static CustomOption ChiefSheriffAlwaysKills;
    public static CustomOption ChiefSheriffCanKillImpostor;
    public static CustomOption ChiefSheriffCanKillNeutral;
    public static CustomOption ChiefSheriffCanKillLovers;
    public static CustomOption ChiefSheriffCanKillMadRole;
    public static CustomOption ChiefSheriffFriendsRoleKill;
    public static CustomOption ChiefSheriffQuarreledKill;

    public static CustomOption ChiefSheriffKillLimit;

    public static CustomRoleOption CleanerOption;
    public static CustomOption CleanerPlayerCount;
    public static CustomOption CleanerCooldown;
    public static CustomOption CleanerKillCoolTime;

    public static CustomRoleOption MadCleanerOption;
    public static CustomOption MadCleanerPlayerCount;
    public static CustomOption MadCleanerCooldown;
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
    public static CustomOption EvilHackerCanMoveWhenUsesAdmin;
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
    public static CustomOption StefinderKillCooldown;
    public static CustomOption StefinderVent;
    public static CustomOption StefinderSabo;
    public static CustomOption StefinderSoloWin;

    public static CustomRoleOption SluggerOption;
    public static CustomOption SluggerPlayerCount;
    public static CustomOption SluggerChargeTime;
    public static CustomOption SluggerCoolTime;
    public static CustomOption SluggerIsMultiKill;
    public static CustomOption SluggerIsKillCoolSync;

    public static CustomRoleOption ConnectKillerOption;
    public static CustomOption ConnectKillerPlayerCount;

    public static CustomRoleOption WaveCannonOption;
    public static CustomOption WaveCannonPlayerCount;
    public static CustomOption WaveCannonCoolTime;
    public static CustomOption WaveCannonChargeTime;
    public static CustomOption WaveCannonIsSyncKillCoolTime;

    public static CustomRoleOption CrackerOption;
    public static CustomOption CrackerPlayerCount;
    public static CustomOption CrackerCoolTime;
    public static CustomOption CrackerIsAdminView;
    public static CustomOption CrackerIsVitalsView;
    public static CustomOption CrackerOneTurnSelectCount;
    public static CustomOption CrackerAllTurnSelectCount;
    public static CustomOption CrackerIsSelfNone;

    public static CustomRoleOption DoppelgangerOption;
    public static CustomOption DoppelgangerPlayerCount;
    public static CustomOption DoppelgangerDurationTime;
    public static CustomOption DoppelgangerCoolTime;
    public static CustomOption DoppelgangerSucTime;
    public static CustomOption DoppelgangerNotSucTime;

    public static CustomRoleOption WerewolfOption;
    public static CustomOption WerewolfPlayerCount;

    public static CustomRoleOption PavlovsownerOption;
    public static CustomOption PavlovsownerPlayerCount;
    public static CustomOption PavlovsownerCreateCoolTime;
    public static CustomOption PavlovsownerCreateDogLimit;
    public static CustomOption PavlovsownerIsTargetImpostorDeath;
    public static CustomOption PavlovsdogIsImpostorView;
    public static CustomOption PavlovsdogKillCoolTime;
    public static CustomOption PavlovsdogCanVent;
    public static CustomOption PavlovsdogRunAwayKillCoolTime;
    public static CustomOption PavlovsdogRunAwayDeathTime;
    public static CustomOption PavlovsdogRunAwayDeathTimeIsMeetingReset;

    public static CustomRoleOption CamouflagerOption;
    public static CustomOption CamouflagerPlayerCount;
    public static CustomOption CamouflagerCoolTime;
    public static CustomOption CamouflagerDurationTime;
    public static CustomOption CamouflagerCamouflageArsonist;
    public static CustomOption CamouflagerCamouflageDemon;
    public static CustomOption CamouflagerCamouflageLovers;
    public static CustomOption CamouflagerCamouflageQuarreled;
    public static CustomOption CamouflagerCamouflageChangeColor;
    public static CustomOption CamouflagerCamouflageColor;

    public static CustomRoleOption CupidOption;
    public static CustomOption CupidPlayerCount;
    public static CustomOption CupidCoolTime;

    public static CustomRoleOption HamburgerShopOption;
    public static CustomOption HamburgerShopPlayerCount;
    public static CustomOption HamburgerShopChangeTaskPrefab;
    public static CustomOption HamburgerShopCommonTask;
    public static CustomOption HamburgerShopShortTask;
    public static CustomOption HamburgerShopLongTask;

    public static CustomRoleOption PenguinOption;
    public static CustomOption PenguinPlayerCount;
    public static CustomOption PenguinCoolTime;
    public static CustomOption PenguinDurationTime;
    public static CustomOption PenguinCanDefaultKill;
    public static CustomOption PenguinMeetingKill;

    public static CustomRoleOption DependentsOption;
    public static CustomOption DependentsPlayerCount;

    public static CustomRoleOption LoversBreakerOption;
    public static CustomOption LoversBreakerPlayerCount;
    public static CustomOption LoversBreakerBreakCount;
    public static CustomOption LoversBreakerCoolTime;
    public static CustomOption LoversBreakerIsDeathWin;

    public static CustomRoleOption JumboOption;
    public static CustomOption JumboPlayerCount;
    public static CustomOption JumboCrewmateChance;
    public static CustomOption JumboMaxSize;
    public static CustomOption JumboSpeedUpSize;
    public static CustomOption JumboWalkSoundSize;
    //CustomOption

    public static CustomOption DebuggerOption;
    public static CustomOption GMOption;

    public static CustomOption QuarreledOption;
    public static CustomOption QuarreledTeamCount;
    public static CustomOption QuarreledOnlyCrewmate;

    public static CustomOption LoversOption;
    public static CustomOption LoversTeamCount;
    public static CustomOption LoversPar;
    public static CustomOption LoversOnlyCrewmate;
    public static CustomOption LoversSingleTeam;
    public static CustomOption LoversSameDie;
    public static CustomOption LoversAliveTaskCount;
    public static CustomOption LoversDuplicationQuarreled;
    public static CustomOption LoversCommonTask;
    public static CustomOption LoversLongTask;
    public static CustomOption LoversShortTask;

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
        List<string> leveData = new() { "optionOff", "LevelingerSettingKeep", "PursuerName", "TeleporterName", "SidekickName", "SpeedBoosterName", "MovingName" };
        List<string> LeveTransed = new();
        foreach (string data in leveData)
        {
            LeveTransed.Add(ModTranslation.GetString(data));
        }
        LevelingerTexts = LeveTransed.ToArray();
        presetSelection = Create(0, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingpresetSelection"), presets, null, true);

        specialOptions = new CustomOptionBlank(null);
        hideSettings = Create(2, true, CustomOptionType.Generic, Cs(Color.white, "SettingsHideSetting"), false, specialOptions);

        /* |: ========================= Mod Normal Settings ========================== :| */

        impostorRolesCountMax = Create(3, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoRole"), 0f, 0f, 15f, 1f);
        neutralRolesCountMax = Create(4, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralRole"), 0f, 0f, 15f, 1f);
        crewmateRolesCountMax = Create(5, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewRole"), 0f, 0f, 15f, 1f);
        impostorGhostRolesCountMax = Create(6, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoGhostRole"), 0f, 0f, 15f, 1f);
        neutralGhostRolesCountMax = Create(7, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralGhostRole"), 0f, 0f, 15f, 1f);
        crewmateGhostRolesCountMax = Create(8, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewGhostRole"), 0f, 0f, 15f, 1f);

        if (ConfigRoles.DebugMode.Value)
        {
            IsDebugMode = Create(10, true, CustomOptionType.Generic, "<color=#828282>デバッグモード</color>", false, null, isHeader: true);
            DebugModeFastStart = Create(681, true, CustomOptionType.Generic, "<color=#828282>即開始</color>", false, IsDebugMode);
            IsMurderPlayerAnnounce = Create(1073, true, CustomOptionType.Generic, "<color=#828282>MurderPlayer発生時に通知を行う</color>", false, IsDebugMode);
        }

        DisconnectNotPCOption = Create(11, true, CustomOptionType.Generic, Cs(new Color(238f / 187f, 204f / 255f, 203f / 255f, 1f), "DisconnectNotPC"), true, null, isHeader: true);

        enableAgartha = Create(970, false, CustomOptionType.Generic, "AgarthaName", true, null, isHeader: true);

        GMOption = Create(1028, false, CustomOptionType.Generic, Cs(RoleClass.GM.color, "GMName"), false, isHeader: true);
        if (ConfigRoles.DebugMode.Value) { DebuggerOption = Create(1172, false, CustomOptionType.Generic, Cs(RoleClass.Debugger.color, "DebuggerName"), false); }

        /* |: ========================= Mod Normal Settings ========================== :| */

        Mode.ModeHandler.OptionLoad(); // モード設定

        MapOption.MapOption.LoadOption(); // マップの設定

        MapCustoms.MapCustom.CreateOption(); // マップ改造

        Sabotage.Options.Load(); // 独自サボタージュの設定

        Mode.PlusMode.PlusGameOptions.Load(); // プラスゲームオプション

        IsOldMode = Create(1027, false, CustomOptionType.Generic, "IsOldMode", false, null, isHeader: true, isHidden: true);
        IsOldMode.selection = 0;

        /* |: ========================= Roles Settings ========================== :| */

        MadRolesCanFixComms = Create(984, true, CustomOptionType.Crewmate, "MadRolesCanFixComms", false, null);
        MadRolesCanFixElectrical = Create(985, true, CustomOptionType.Crewmate, "MadRolesCanFixElectrical", false, null);
        MadRolesCanVentMove = Create(1013, false, CustomOptionType.Crewmate, "MadRolesCanVentMove", false, null);

        JesterOption = SetupCustomRoleOption(16, true, RoleId.Jester);
        JesterPlayerCount = Create(17, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JesterOption);
        JesterIsVent = Create(18, true, CustomOptionType.Neutral, "JesterIsVentSetting", false, JesterOption);
        JesterIsSabotage = Create(19, false, CustomOptionType.Neutral, "JesterIsSabotageSetting", false, JesterOption);
        JesterIsWinCleartask = Create(20, true, CustomOptionType.Neutral, "JesterIsWinClearTaskSetting", false, JesterOption);
        var jesteroption = SelectTask.TaskSetting(21, 22, 23, JesterIsWinCleartask, CustomOptionType.Neutral, true);
        JesterCommonTask = jesteroption.Item1;
        JesterShortTask = jesteroption.Item2;
        JesterLongTask = jesteroption.Item3;

        EvilScientistOption = SetupCustomRoleOption(33, false, RoleId.EvilScientist);
        EvilScientistPlayerCount = Create(34, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilScientistOption);
        EvilScientistCoolTime = Create(35, false, CustomOptionType.Impostor, "EvilScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, EvilScientistOption, format: "unitSeconds");
        EvilScientistDurationTime = Create(36, false, CustomOptionType.Impostor, "EvilScientistDurationSetting", 10f, 2.5f, 20f, 2.5f, EvilScientistOption, format: "unitSeconds");

        SheriffOption = SetupCustomRoleOption(700, true, RoleId.Sheriff);
        SheriffPlayerCount = Create(701, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SheriffOption);
        SheriffCoolTime = Create(702, true, CustomOptionType.Crewmate, "SheriffCooldownSetting", 30f, 2.5f, 60f, 2.5f, SheriffOption, format: "unitSeconds");
        SheriffKillMaxCount = Create(703, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, SheriffOption, format: "unitSeconds");
        SheriffAlwaysKills = Create(704, true, CustomOptionType.Crewmate, "SheriffAlwaysKills", false, SheriffOption);
        SheriffCanKillImpostor = Create(705, true, CustomOptionType.Crewmate, "SheriffIsKillImpostorSetting", true, SheriffOption);
        SheriffMadRoleKill = Create(706, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, SheriffOption);
        SheriffNeutralKill = Create(707, true, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, SheriffOption);
        SheriffFriendsRoleKill = Create(708, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, SheriffOption);
        SheriffLoversKill = Create(709, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, SheriffOption);
        SheriffQuarreledKill = Create(710, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, SheriffOption);

        RemoteSheriffOption = SetupCustomRoleOption(711, true, RoleId.RemoteSheriff);
        RemoteSheriffPlayerCount = Create(712, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RemoteSheriffOption);
        RemoteSheriffCoolTime = Create(713, false, CustomOptionType.Crewmate, ModTranslation.GetString("SheriffCooldownSetting"), 30f, 2.5f, 60f, 2.5f, RemoteSheriffOption, format: "unitSeconds");
        RemoteSheriffKillMaxCount = Create(714, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, RemoteSheriffOption, format: "unitSeconds");
        RemoteSheriffIsKillTeleportSetting = Create(715, true, CustomOptionType.Crewmate, "RemoteSheriffIsKillTeleportSetting", false, RemoteSheriffOption);
        RemoteSheriffAlwaysKills = Create(716, true, CustomOptionType.Crewmate, "SheriffAlwaysKills", false, RemoteSheriffOption);
        RemoteSheriffMadRoleKill = Create(717, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, RemoteSheriffOption);
        RemoteSheriffNeutralKill = Create(718, true, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, RemoteSheriffOption);
        RemoteSheriffFriendRolesKill = Create(719, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, RemoteSheriffOption);
        RemoteSheriffLoversKill = Create(720, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, RemoteSheriffOption);
        RemoteSheriffQuarreledKill = Create(721, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, RemoteSheriffOption);

        MeetingSheriffOption = SetupCustomRoleOption(723, false, RoleId.MeetingSheriff);
        MeetingSheriffPlayerCount = Create(724, false, CustomOptionType.Crewmate, Cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MeetingSheriffOption);
        MeetingSheriffKillMaxCount = Create(725, false, CustomOptionType.Crewmate, "MeetingSheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, MeetingSheriffOption, format: "unitSeconds");
        MeetingSheriffOneMeetingMultiKill = Create(726, false, CustomOptionType.Crewmate, "MeetingSheriffMeetingmultipleKillSetting", false, MeetingSheriffOption);
        MeetingSheriffAlwaysKills = Create(727, false, CustomOptionType.Crewmate, "SheriffAlwaysKills", false, MeetingSheriffOption);
        MeetingSheriffMadRoleKill = Create(728, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillMadRoleSetting", false, MeetingSheriffOption);
        MeetingSheriffNeutralKill = Create(729, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillNeutralSetting", false, MeetingSheriffOption);
        MeetingSheriffFriendsRoleKill = Create(730, false, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, MeetingSheriffOption);
        MeetingSheriffLoversKill = Create(731, false, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, MeetingSheriffOption);
        MeetingSheriffQuarreledKill = Create(732, false, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, MeetingSheriffOption);

        ChiefOption = SetupCustomRoleOption(733, false, RoleId.Chief);
        ChiefPlayerCount = Create(734, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ChiefOption);
        ChiefSheriffCoolTime = Create(735, false, CustomOptionType.Crewmate, "SheriffCooldownSetting", 30f, 2.5f, 60f, 2.5f, ChiefOption, format: "unitSeconds");
        ChiefSheriffKillLimit = Create(736, false, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, ChiefOption, format: "unitSeconds");
        ChiefSheriffAlwaysKills = Create(737, false, CustomOptionType.Crewmate, "SheriffAlwaysKills", false, ChiefOption);
        ChiefSheriffCanKillImpostor = Create(738, false, CustomOptionType.Crewmate, "SheriffIsKillImpostorSetting", true, ChiefOption);
        ChiefSheriffCanKillMadRole = Create(739, false, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, ChiefOption);
        ChiefSheriffCanKillNeutral = Create(740, false, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, ChiefOption);
        ChiefSheriffFriendsRoleKill = Create(741, false, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, ChiefOption);
        ChiefSheriffCanKillLovers = Create(742, false, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, ChiefOption);
        ChiefSheriffQuarreledKill = Create(743, false, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, ChiefOption);

        MayorOption = SetupCustomRoleOption(229, true, RoleId.Mayor);
        MayorPlayerCount = Create(230, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorOption);
        MayorVoteCount = Create(231, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorOption);

        MadmateOption = SetupCustomRoleOption(130, true, RoleId.Madmate);
        MadmatePlayerCount = Create(131, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadmateOption);
        MadmateIsCheckImpostor = Create(132, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadmateOption);
        var madmateoption = SelectTask.TaskSetting(133, 134, 135, MadmateIsCheckImpostor, CustomOptionType.Crewmate, true);
        MadmateCommonTask = madmateoption.Item1;
        MadmateShortTask = madmateoption.Item2;
        MadmateLongTask = madmateoption.Item3;
        //MadmateIsNotTask = madmateoption.Item4;
        MadmateCheckImpostorTask = Create(136, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, MadmateIsCheckImpostor);
        MadmateIsUseVent = Create(137, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadmateOption);
        MadmateIsImpostorLight = Create(138, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadmateOption);

        BlackCatOption = SetupCustomRoleOption(556, true, RoleId.BlackCat);
        BlackCatPlayerCount = Create(557, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BlackCatOption);
        BlackCatNotImpostorExiled = Create(675, true, CustomOptionType.Crewmate, "NotImpostorExiled", false, BlackCatOption);
        BlackCatIsCheckImpostor = Create(558, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, BlackCatOption);
        var blackcatoption = SelectTask.TaskSetting(559, 560, 561, BlackCatIsCheckImpostor, CustomOptionType.Crewmate, true);
        BlackCatCommonTask = blackcatoption.Item1;
        BlackCatShortTask = blackcatoption.Item2;
        BlackCatLongTask = blackcatoption.Item3;
        //MadmateIsNotTask = madmateoption.Item4;
        BlackCatCheckImpostorTask = Create(562, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, BlackCatIsCheckImpostor);
        BlackCatIsUseVent = Create(563, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, BlackCatOption);
        BlackCatIsImpostorLight = Create(564, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, BlackCatOption);

        Roles.Impostor.MadRole.Worshiper.SetupCustomOptions();

        MadMayorOption = SetupCustomRoleOption(269, true, RoleId.MadMayor);
        MadMayorPlayerCount = Create(270, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMayorOption);
        MadMayorVoteCount = Create(271, true, CustomOptionType.Crewmate, "MadMayorVoteCountSetting", 2f, 1f, 100f, 1f, MadMayorOption);
        MadMayorIsCheckImpostor = Create(272, true, CustomOptionType.Crewmate, "MadMayorIsCheckImpostorSetting", false, MadMayorOption);
        var madmayoroption = SelectTask.TaskSetting(273, 274, 275, MadMayorIsCheckImpostor, CustomOptionType.Crewmate, true);
        MadMayorCommonTask = madmayoroption.Item1;
        MadMayorShortTask = madmayoroption.Item2;
        MadMayorLongTask = madmayoroption.Item3;
        MadMayorCheckImpostorTask = Create(276, true, CustomOptionType.Crewmate, "MadMayorCheckImpostorTaskSetting", rates4, MadMayorIsCheckImpostor);
        MadMayorIsUseVent = Create(277, true, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadMayorOption);
        MadMayorIsImpostorLight = Create(278, true, CustomOptionType.Crewmate, "MadMayorImpostorLightSetting", false, MadMayorOption);

        MadSeerOption = SetupCustomRoleOption(323, true, RoleId.MadSeer);
        MadSeerPlayerCount = Create(324, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadSeerOption);
        MadSeerMode = Create(325, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, MadSeerOption);
        MadSeerLimitSoulDuration = Create(326, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, MadSeerOption);
        MadSeerSoulDuration = Create(327, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, MadSeerLimitSoulDuration, format: "unitCouples");
        MadSeerIsUseVent = Create(328, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadSeerOption);
        MadSeerIsImpostorLight = Create(329, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadSeerOption);
        MadSeerIsCheckImpostor = Create(330, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadSeerOption);
        var madseeroption = SelectTask.TaskSetting(331, 332, 526, MadSeerIsCheckImpostor, CustomOptionType.Crewmate, true);
        MadSeerCommonTask = madseeroption.Item1;
        MadSeerShortTask = madseeroption.Item2;
        MadSeerLongTask = madseeroption.Item3;
        MadSeerCheckImpostorTask = Create(333, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, MadSeerIsCheckImpostor);

        MadMakerOption = SetupCustomRoleOption(347, true, RoleId.MadMaker);
        MadMakerPlayerCount = Create(348, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMakerOption);
        MadMakerIsUseVent = Create(349, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadMakerOption);
        MadMakerIsImpostorLight = Create(350, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadMakerOption);

        MadJesterOption = SetupCustomRoleOption(296, true, RoleId.MadJester);
        MadJesterPlayerCount = Create(297, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadJesterOption);
        MadJesterIsUseVent = Create(298, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadJesterOption);
        MadJesterIsImpostorLight = Create(299, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadJesterOption);
        IsMadJesterTaskClearWin = Create(300, true, CustomOptionType.Crewmate, "JesterIsWinClearTaskSetting", false, MadJesterOption);
        MadJesterIsCheckImpostor = Create(1173, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadJesterOption);
        MadJesterCheckImpostorTask = Create(1174, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, MadJesterIsCheckImpostor);
        var MadJesteroption = SelectTask.TaskSetting(667, 668, 669, MadJesterOption, CustomOptionType.Crewmate, true);
        MadJesterCommonTask = MadJesteroption.Item1;
        MadJesterShortTask = MadJesteroption.Item2;
        MadJesterLongTask = MadJesteroption.Item3;

        MadHawkOption = SetupCustomRoleOption(287, false, RoleId.MadHawk);
        MadHawkPlayerCount = Create(289, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadHawkOption);
        MadHawkCoolTime = Create(290, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 0f, 120f, 2.5f, MadHawkOption, format: "unitCouples");
        MadHawkDurationTime = Create(291, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 0.5f, MadHawkOption, format: "unitCouples");
        MadHawkIsUseVent = Create(292, false, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadHawkOption);
        MadHawkIsImpostorLight = Create(293, false, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadHawkOption);

        MadCleanerOption = SetupCustomRoleOption(400, false, RoleId.MadCleaner);
        MadCleanerPlayerCount = Create(401, false, CustomOptionType.Crewmate, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MadCleanerOption);
        MadCleanerCooldown = Create(402, false, CustomOptionType.Crewmate, "CleanerCooldownSetting", 30f, 2.5f, 60f, 2.5f, MadCleanerOption, format: "unitSeconds");
        MadCleanerIsUseVent = Create(403, false, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadCleanerOption);
        MadCleanerIsImpostorLight = Create(404, false, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadCleanerOption);

        MadStuntManOption = SetupCustomRoleOption(283, false, RoleId.MadStuntMan);
        MadStuntManPlayerCount = Create(284, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadStuntManOption);
        MadStuntManIsUseVent = Create(285, false, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadStuntManOption);
        MadStuntManIsImpostorLight = Create(286, false, CustomOptionType.Crewmate, "MadStuntManImpostorLightSetting", false, MadStuntManOption);

        SatsumaAndImoOption = SetupCustomRoleOption(953, true, RoleId.SatsumaAndImo);
        SatsumaAndImoPlayerCount = Create(800, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SatsumaAndImoOption);

        LighterOption = SetupCustomRoleOption(24, false, RoleId.Lighter);
        LighterPlayerCount = Create(25, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LighterOption);
        LighterCoolTime = Create(26, false, CustomOptionType.Crewmate, "LigtherCooldownSetting", 30f, 2.5f, 60f, 2.5f, LighterOption, format: "unitSeconds");
        LighterDurationTime = Create(27, false, CustomOptionType.Crewmate, "LigtherDurationSetting", 10f, 0f, 180f, 5f, LighterOption, format: "unitSeconds");
        LighterUpVision = Create(28, false, CustomOptionType.Crewmate, "LighterUpVisionSetting", 0.25f, 0f, 5f, 0.25f, LighterOption);

        JackalOption = SetupCustomRoleOption(58, true, RoleId.Jackal);
        JackalPlayerCount = Create(59, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalOption);
        JackalKillCooldown = Create(60, true, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, JackalOption, format: "unitSeconds");
        JackalUseVent = Create(61, true, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalOption);
        JackalUseSabo = Create(62, true, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalOption);
        JackalIsImpostorLight = Create(63, true, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, JackalOption);
        JackalCreateFriend = Create(666, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, JackalOption);
        JackalCreateSidekick = Create(64, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalOption);
        JackalSKCooldown = Create(1108, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, JackalCreateSidekick, format: "unitSeconds");
        JackalNewJackalCreateSidekick = Create(65, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalCreateSidekick);

        TeleporterOption = SetupCustomRoleOption(66, false, RoleId.Teleporter);
        TeleporterPlayerCount = Create(67, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TeleporterOption);
        TeleporterCoolTime = Create(68, false, CustomOptionType.Impostor, "TeleporterCooldownSetting", 30f, 2.5f, 60f, 2.5f, TeleporterOption, format: "unitSeconds");
        TeleporterDurationTime = Create(69, false, CustomOptionType.Impostor, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleporterOption, format: "unitSeconds");

        SoothSayerOption = SetupCustomRoleOption(12, false, RoleId.SoothSayer);
        SoothSayerPlayerCount = Create(13, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
        SoothSayerDisplayMode = Create(14, false, CustomOptionType.Crewmate, "SoothSayerDisplaySetting", false, SoothSayerOption);
        SoothSayerMaxCount = Create(15, false, CustomOptionType.Crewmate, "SoothSayerMaxCountSetting", CrewPlayers[0] - 1, CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
        SoothSayerFirstWhiteOption = Create(1050, false, CustomOptionType.Crewmate, "SoothSayerFirstWhiteOption", false, SoothSayerOption);

        SpiritMediumOption = SetupCustomRoleOption(70, false, RoleId.SpiritMedium);
        SpiritMediumPlayerCount = Create(71, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpiritMediumOption);
        SpiritMediumIsAutoMode = Create(1058, false, CustomOptionType.Crewmate, "SpiritMediumIsAutoMode", false, SpiritMediumOption);
        SpiritMediumDisplayMode = Create(72, false, CustomOptionType.Crewmate, "SpiritMediumDisplaySetting", false, SpiritMediumOption);
        SpiritMediumMaxCount = Create(73, false, CustomOptionType.Crewmate, "SpiritMediumMaxCountSetting", 2f, 1f, 15f, 1f, SpiritMediumOption);

        SpeedBoosterOption = SetupCustomRoleOption(74, false, RoleId.SpeedBooster);
        SpeedBoosterPlayerCount = Create(75, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeedBoosterOption);
        SpeedBoosterCoolTime = Create(76, false, CustomOptionType.Crewmate, "SpeedBoosterCooldownSetting", 30f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
        SpeedBoosterDurationTime = Create(77, false, CustomOptionType.Crewmate, "SpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
        SpeedBoosterSpeed = Create(78, false, CustomOptionType.Crewmate, "SpeedBoosterPlusSpeedSetting", 1.25f, 0.0f, 5f, 0.25f, SpeedBoosterOption, format: "unitSeconds");

        EvilSpeedBoosterOption = SetupCustomRoleOption(79, false, RoleId.EvilSpeedBooster);
        EvilSpeedBoosterPlayerCount = Create(80, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EvilSpeedBoosterOption);
        EvilSpeedBoosterCoolTime = Create(81, false, CustomOptionType.Impostor, "EvilSpeedBoosterCooldownSetting", 30f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
        EvilSpeedBoosterDurationTime = Create(82, false, CustomOptionType.Impostor, "EvilSpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
        EvilSpeedBoosterSpeed = Create(83, false, CustomOptionType.Impostor, "EvilSpeedBoosterPlusSpeedSetting", 1.25f, 0.0f, 5f, 0.25f, EvilSpeedBoosterOption, format: "unitSeconds");
        EvilSpeedBoosterIsNotSpeedBooster = Create(84, false, CustomOptionType.Impostor, "EvilSpeedBoosterIsNotSpeedBooster", false, EvilSpeedBoosterOption);

        DoorrOption = SetupCustomRoleOption(89, false, RoleId.Doorr);
        DoorrPlayerCount = Create(90, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoorrOption);
        DoorrCoolTime = Create(91, false, CustomOptionType.Crewmate, "DoorrCoolTimeSetting", 2.5f, 2.5f, 60f, 2.5f, DoorrOption);

        EvilDoorrOption = SetupCustomRoleOption(92, false, RoleId.EvilDoorr);
        EvilDoorrPlayerCount = Create(93, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilDoorrOption);
        EvilDoorrCoolTime = Create(94, false, CustomOptionType.Impostor, "EvilDoorrCoolTimeSetting", 2.5f, 2.5f, 60f, 2.5f, EvilDoorrOption);

        ShielderOption = SetupCustomRoleOption(95, false, RoleId.Shielder);
        ShielderPlayerCount = Create(96, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ShielderOption);
        ShielderCoolTime = Create(97, false, CustomOptionType.Crewmate, "ShielderCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, ShielderOption, format: "unitCouples");
        ShielderDurationTime = Create(98, false, CustomOptionType.Crewmate, "ShielderDurationSetting", 10f, 2.5f, 30f, 2.5f, ShielderOption, format: "unitCouples");

        FreezerOption = SetupCustomRoleOption(99, false, RoleId.Freezer);
        FreezerPlayerCount = Create(100, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FreezerOption);
        FreezerCoolTime = Create(101, false, CustomOptionType.Impostor, "FreezerCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, FreezerOption, format: "unitSeconds");
        FreezerDurationTime = Create(102, false, CustomOptionType.Impostor, "FreezerDurationSetting", 1f, 1f, 7f, 1f, FreezerOption, format: "unitSeconds");

        SpeederOption = SetupCustomRoleOption(103, false, RoleId.Speeder);
        SpeederPlayerCount = Create(104, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeederOption);
        SpeederCoolTime = Create(105, false, CustomOptionType.Impostor, "SpeederCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, SpeederOption, format: "unitSeconds");
        SpeederDurationTime = Create(106, false, CustomOptionType.Impostor, "SpeederDurationTimeSetting", 10f, 2.5f, 20f, 2.5f, SpeederOption, format: "unitSeconds");

        VultureOption = SetupCustomRoleOption(115, false, RoleId.Vulture);
        VulturePlayerCount = Create(116, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], VultureOption);
        VultureCooldown = Create(117, false, CustomOptionType.Neutral, "VultureCooldownSetting", 30f, 2.5f, 60f, 2.5f, VultureOption, format: "unitSeconds");
        VultureDeadBodyMaxCount = Create(118, false, CustomOptionType.Neutral, "VultureDeadBodyCountSetting", 3f, 1f, 6f, 1f, VultureOption);
        VultureIsUseVent = Create(119, false, CustomOptionType.Neutral, "MadmateUseVentSetting", false, VultureOption);
        VultureShowArrows = Create(120, false, CustomOptionType.Neutral, "VultureShowArrowsSetting", false, VultureOption);

        NiceScientistOption = SetupCustomRoleOption(121, false, RoleId.NiceScientist);
        NiceScientistPlayerCount = Create(122, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceScientistOption);
        NiceScientistCoolTime = Create(123, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, NiceScientistOption, format: "unitSeconds");
        NiceScientistDurationTime = Create(124, false, CustomOptionType.Crewmate, "NiceScientistDurationSetting", 10f, 2.5f, 20f, 2.5f, NiceScientistOption, format: "unitSeconds");

        ClergymanOption = SetupCustomRoleOption(125, false, RoleId.Clergyman);
        ClergymanPlayerCount = Create(126, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ClergymanOption);
        ClergymanCoolTime = Create(127, false, CustomOptionType.Crewmate, "ClergymanCooldownSetting", 30f, 2.5f, 60f, 2.5f, ClergymanOption, format: "unitSeconds");
        ClergymanDurationTime = Create(128, false, CustomOptionType.Crewmate, "ClergymanDurationTimeSetting", 10f, 1f, 20f, 0.5f, ClergymanOption, format: "unitSeconds");
        ClergymanDownVision = Create(129, false, CustomOptionType.Crewmate, "ClergymanDownVisionSetting", 0.25f, 0f, 5f, 0.25f, ClergymanOption);

        BaitOption = SetupCustomRoleOption(486, true, RoleId.Bait);
        BaitPlayerCount = Create(487, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BaitOption);
        BaitReportTime = Create(488, true, CustomOptionType.Crewmate, "BaitReportTimeSetting", 2f, 1f, 4f, 0.5f, BaitOption);

        HomeSecurityGuardOption = SetupCustomRoleOption(139, true, RoleId.HomeSecurityGuard);
        HomeSecurityGuardPlayerCount = Create(140, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HomeSecurityGuardOption);

        StuntManOption = SetupCustomRoleOption(141, true, RoleId.StuntMan);
        StuntManPlayerCount = Create(142, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StuntManOption);
        StuntManMaxGuardCount = Create(143, true, CustomOptionType.Crewmate, "StuntManGuardMaxCountSetting", 1f, 1f, 15f, 1f, StuntManOption);

        MovingOption = SetupCustomRoleOption(144, false, RoleId.Moving);
        MovingPlayerCount = Create(145, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MovingOption);
        MovingCoolTime = Create(146, false, CustomOptionType.Crewmate, "MovingCooldownSetting", 30f, 0f, 60f, 2.5f, MovingOption);

        OpportunistOption = SetupCustomRoleOption(147, true, RoleId.Opportunist);
        OpportunistPlayerCount = Create(148, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], OpportunistOption);

        EvilGamblerOption = SetupCustomRoleOption(152, true, RoleId.EvilGambler);
        EvilGamblerPlayerCount = Create(153, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGamblerOption);
        EvilGamblerSucTime = Create(154, true, CustomOptionType.Impostor, "EvilGamblerSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
        EvilGamblerNotSucTime = Create(155, true, CustomOptionType.Impostor, "EvilGamblerNotSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
        EvilGamblerSucpar = Create(156, true, CustomOptionType.Impostor, "EvilGamblerSucParSetting", rates, EvilGamblerOption);

        BestfalsechargeOption = SetupCustomRoleOption(157, true, RoleId.Bestfalsecharge);
        BestfalsechargePlayerCount = Create(158, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BestfalsechargeOption);

        SelfBomberOption = SetupCustomRoleOption(161, true, RoleId.SelfBomber);
        SelfBomberPlayerCount = Create(162, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SelfBomberOption);
        SelfBomberScope = Create(163, true, CustomOptionType.Impostor, "SelfBomberScopeSetting", 1f, 0.5f, 3f, 0.5f, SelfBomberOption);
        SelfBomberBombCoolTime = Create(1111, true, CustomOptionType.Impostor, "SelfBomberBombCoolTimeSettting", 10f, 2.5f, 90f, 2.5f, SelfBomberOption);

        GodOption = SetupCustomRoleOption(164, true, RoleId.God);
        GodPlayerCount = Create(165, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GodOption);
        GodViewVote = Create(167, true, CustomOptionType.Neutral, "GodViewVoteSetting", false, GodOption);
        GodIsEndTaskWin = Create(168, true, CustomOptionType.Neutral, "GodIsEndTaskWinSetting", true, GodOption);
        var godoption = SelectTask.TaskSetting(169, 170, 171, GodIsEndTaskWin, CustomOptionType.Neutral, true);
        GodCommonTask = godoption.Item1;
        GodShortTask = godoption.Item2;
        GodLongTask = godoption.Item3;

        NiceNekomataOption = SetupCustomRoleOption(175, true, RoleId.NiceNekomata);
        NiceNekomataPlayerCount = Create(176, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceNekomataOption);
        NiceNekomataIsChain = Create(177, true, CustomOptionType.Crewmate, "NiceNekomataIsChainSetting", true, NiceNekomataOption);

        EvilNekomataOption = SetupCustomRoleOption(178, true, RoleId.EvilNekomata);
        EvilNekomataPlayerCount = Create(179, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilNekomataOption);
        EvilNekomataNotImpostorExiled = Create(674, true, CustomOptionType.Impostor, "NotImpostorExiled", false, EvilNekomataOption);

        JackalFriendsOption = SetupCustomRoleOption(180, true, RoleId.JackalFriends);
        JackalFriendsPlayerCount = Create(181, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalFriendsOption);
        JackalFriendsIsUseVent = Create(182, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, JackalFriendsOption);
        JackalFriendsIsImpostorLight = Create(183, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, JackalFriendsOption);
        JackalFriendsIsCheckJackal = Create(184, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, JackalFriendsOption);
        var JackalFriendsoption = SelectTask.TaskSetting(185, 186, 187, JackalFriendsIsCheckJackal, CustomOptionType.Crewmate, true);
        JackalFriendsCommonTask = JackalFriendsoption.Item1;
        JackalFriendsShortTask = JackalFriendsoption.Item2;
        JackalFriendsLongTask = JackalFriendsoption.Item3;
        JackalFriendsCheckJackalTask = Create(189, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, JackalFriendsIsCheckJackal);

        DoctorOption = SetupCustomRoleOption(190, false, RoleId.Doctor);
        DoctorPlayerCount = Create(191, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoctorOption);
        DoctorChargeTime = Create(966, false, CustomOptionType.Crewmate, "DoctorChargeTime", 10f, 0f, 60f, 2.5f, DoctorOption);
        DoctorUseTime = Create(967, false, CustomOptionType.Crewmate, "DoctorUseTime", 5f, 0f, 60f, 2.5f, DoctorOption);

        CountChangerOption = SetupCustomRoleOption(192, false, RoleId.CountChanger);
        CountChangerPlayerCount = Create(193, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CountChangerOption);
        CountChangerMaxCount = Create(194, false, CustomOptionType.Impostor, "CountChangerMaxCountSetting", 1f, 1f, 15f, 1f, CountChangerOption);
        CountChangerNextTurn = Create(195, false, CustomOptionType.Impostor, "CountChangerNextTurnSetting", false, CountChangerOption);

        PursuerOption = SetupCustomRoleOption(196, false, RoleId.Pursuer);
        PursuerPlayerCount = Create(197, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PursuerOption);

        MinimalistOption = SetupCustomRoleOption(198, true, RoleId.Minimalist);
        MinimalistPlayerCount = Create(199, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MinimalistOption);
        MinimalistKillCoolTime = Create(200, true, CustomOptionType.Impostor, "MinimalistKillCoolSetting", 20f, 2.5f, 60f, 2.5f, MinimalistOption);
        MinimalistVent = Create(201, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, MinimalistOption);
        MinimalistSabo = Create(202, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, MinimalistOption);
        MinimalistReport = Create(203, true, CustomOptionType.Impostor, "MinimalistReportSetting", true, MinimalistOption);

        HawkOption = SetupCustomRoleOption(204, false, RoleId.Hawk);
        HawkPlayerCount = Create(205, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], HawkOption);
        HawkCoolTime = Create(206, false, CustomOptionType.Impostor, "HawkCoolTimeSetting", 15f, 0f, 120f, 2.5f, HawkOption, format: "unitCouples");
        HawkDurationTime = Create(207, false, CustomOptionType.Impostor, "HawkDurationTimeSetting", 5f, 0f, 60f, 0.5f, HawkOption, format: "unitCouples");

        EgoistOption = SetupCustomRoleOption(208, true, RoleId.Egoist);
        EgoistPlayerCount = Create(209, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EgoistOption);
        EgoistUseVent = Create(210, true, CustomOptionType.Neutral, "EgoistUseVentSetting", false, EgoistOption);
        EgoistUseSabo = Create(211, true, CustomOptionType.Neutral, "EgoistUseSaboSetting", false, EgoistOption);
        EgoistImpostorLight = Create(212, true, CustomOptionType.Neutral, "EgoistImpostorLightSetting", false, EgoistOption);
        EgoistUseKill = Create(213, true, CustomOptionType.Neutral, "EgoistUseKillSetting", false, EgoistOption);

        NiceRedRidingHoodOption = SetupCustomRoleOption(214, false, RoleId.NiceRedRidingHood);
        NiceRedRidingHoodPlayerCount = Create(215, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceRedRidingHoodOption);
        NiceRedRidingHoodCount = Create(216, false, CustomOptionType.Crewmate, "NiceRedRidingHoodCount", 1f, 1f, 15f, 1f, NiceRedRidingHoodOption);
        NiceRedRidinIsKillerDeathRevive = Create(1072, false, CustomOptionType.Crewmate, "NiceRedRidinIsKillerDeathRevive", true, NiceRedRidingHoodOption);

        EvilEraserOption = SetupCustomRoleOption(217, false, RoleId.EvilEraser);
        EvilEraserPlayerCount = Create(218, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilEraserOption);
        EvilEraserMaxCount = Create(219, false, CustomOptionType.Impostor, "EvilEraserMaxCountSetting", 1f, 1f, 15f, 1f, EvilEraserOption);

        WorkpersonOption = SetupCustomRoleOption(220, true, RoleId.Workperson);
        WorkpersonPlayerCount = Create(221, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WorkpersonOption);
        WorkpersonIsAliveWin = Create(222, true, CustomOptionType.Neutral, "WorkpersonIsAliveWinSetting", false, WorkpersonOption);
        WorkpersonCommonTask = Create(223, true, CustomOptionType.Neutral, "GameCommonTasks", 2, 0, 12, 1, WorkpersonOption);
        WorkpersonLongTask = Create(224, true, CustomOptionType.Neutral, "GameLongTasks", 10, 0, 69, 1, WorkpersonOption);
        WorkpersonShortTask = Create(225, true, CustomOptionType.Neutral, "GameShortTasks", 5, 0, 45, 1, WorkpersonOption);

        MagazinerOption = SetupCustomRoleOption(226, false, RoleId.Magaziner);
        MagazinerPlayerCount = Create(227, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MagazinerOption);
        MagazinerSetKillTime = Create(228, false, CustomOptionType.Impostor, "MagazinerSetTimeSetting", 0f, 0f, 60f, 2.5f, MagazinerOption);

        trueloverOption = SetupCustomRoleOption(232, true, RoleId.truelover);
        trueloverPlayerCount = Create(233, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], trueloverOption);

        TechnicianOption = SetupCustomRoleOption(234, true, RoleId.Technician);
        TechnicianPlayerCount = Create(235, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TechnicianOption);

        SerialKillerOption = SetupCustomRoleOption(236, true, RoleId.SerialKiller);
        SerialKillerPlayerCount = Create(237, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SerialKillerOption);
        SerialKillerSuicideTime = Create(238, true, CustomOptionType.Impostor, "SerialKillerSuicideTimeSetting", 60f, 0f, 180f, 2.5f, SerialKillerOption);
        SerialKillerKillTime = Create(239, true, CustomOptionType.Impostor, "SerialKillerKillTimeSetting", 15f, 0f, 60f, 2.5f, SerialKillerOption);
        SerialKillerIsMeetingReset = Create(240, true, CustomOptionType.Impostor, "SerialKillerIsMeetingResetSetting", true, SerialKillerOption);

        OverKillerOption = SetupCustomRoleOption(241, true, RoleId.OverKiller);
        OverKillerPlayerCount = Create(242, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], OverKillerOption);
        OverKillerKillCoolTime = Create(243, true, CustomOptionType.Impostor, "OverKillerKillCoolTimeSetting", 45f, 0f, 60f, 2.5f, OverKillerOption);
        OverKillerKillCount = Create(245, true, CustomOptionType.Impostor, "OverKillerKillCountSetting", 30f, 1f, 60f, 1f, OverKillerOption);

        LevelingerOption = SetupCustomRoleOption(246, false, RoleId.Levelinger);
        LevelingerPlayerCount = Create(247, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], LevelingerOption);
        LevelingerOneKillXP = Create(248, false, CustomOptionType.Impostor, "LevelingerOneKillXPSetting", 1f, 0f, 10f, 1f, LevelingerOption);
        LevelingerUpLevelXP = Create(249, false, CustomOptionType.Impostor, "LevelingerUpLevelXPSetting", 2f, 1f, 50f, 1f, LevelingerOption);
        LevelingerLevelOneGetPower = Create(250, false, CustomOptionType.Impostor, "1" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelTwoGetPower = Create(251, false, CustomOptionType.Impostor, "2" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelThreeGetPower = Create(252, false, CustomOptionType.Impostor, "3" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelFourGetPower = Create(253, false, CustomOptionType.Impostor, "4" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelFiveGetPower = Create(254, false, CustomOptionType.Impostor, "5" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerReviveXP = Create(255, false, CustomOptionType.Impostor, "LevelingerReviveXPSetting", false, LevelingerOption);
        LevelingerUseXPRevive = Create(256, false, CustomOptionType.Impostor, "LevelingerUseXPReviveSetting", 5f, 0f, 20f, 1f, LevelingerReviveXP);

        EvilMovingOption = SetupCustomRoleOption(257, false, RoleId.EvilMoving);
        EvilMovingPlayerCount = Create(258, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilMovingOption);
        EvilMovingCoolTime = Create(259, false, CustomOptionType.Impostor, "MovingCooldownSetting", 30f, 0f, 60f, 2.5f, EvilMovingOption);

        AmnesiacOption = SetupCustomRoleOption(260, false, RoleId.Amnesiac);
        AmnesiacPlayerCount = Create(261, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AmnesiacOption);

        SideKillerOption = SetupCustomRoleOption(262, false, RoleId.SideKiller);
        SideKillerPlayerCount = Create(263, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SideKillerOption);
        SideKillerKillCoolTime = Create(264, false, CustomOptionType.Impostor, "SideKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);
        SideKillerMadKillerKillCoolTime = Create(265, false, CustomOptionType.Impostor, "SideKillerMadKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);

        SurvivorOption = SetupCustomRoleOption(266, true, RoleId.Survivor);
        SurvivorPlayerCount = Create(267, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SurvivorOption);
        SurvivorKillCoolTime = Create(268, true, CustomOptionType.Impostor, "SurvivorKillCoolTimeSetting", 15f, 0f, 75f, 2.5f, SurvivorOption);

        NiceHawkOption = SetupCustomRoleOption(279, false, RoleId.NiceHawk);
        NiceHawkPlayerCount = Create(280, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceHawkOption);
        NiceHawkCoolTime = Create(281, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 0f, 120f, 2.5f, NiceHawkOption, format: "unitCouples");
        NiceHawkDurationTime = Create(282, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 0.5f, NiceHawkOption, format: "unitCouples");

        BakeryOption = SetupCustomRoleOption(294, true, RoleId.Bakery);
        BakeryPlayerCount = Create(295, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BakeryOption);

        FalseChargesOption = SetupCustomRoleOption(517, true, RoleId.FalseCharges);
        FalseChargesPlayerCount = Create(518, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FalseChargesOption);
        FalseChargesExileTurn = Create(519, true, CustomOptionType.Neutral, "FalseChargesExileTurn", 2f, 1f, 10f, 1f, FalseChargesOption);
        FalseChargesCoolTime = Create(520, true, CustomOptionType.Neutral, "FalseChargesCoolTime", 15f, 0f, 75f, 2.5f, FalseChargesOption);

        NiceTeleporterOption = SetupCustomRoleOption(521, false, RoleId.NiceTeleporter);
        NiceTeleporterPlayerCount = Create(522, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceTeleporterOption);
        NiceTeleporterCoolTime = Create(523, false, CustomOptionType.Crewmate, "NiceTeleporterCooldownSetting", 30f, 2.5f, 60f, 2.5f, NiceTeleporterOption, format: "unitSeconds");
        NiceTeleporterDurationTime = Create(524, false, CustomOptionType.Crewmate, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, NiceTeleporterOption, format: "unitSeconds");

        CelebrityOption = SetupCustomRoleOption(525, true, RoleId.Celebrity);
        CelebrityPlayerCount = Create(301, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], CelebrityOption);
        CelebrityChangeRoleView = Create(302, true, CustomOptionType.Crewmate, "CelebrityChangeRoleViewSetting", false, CelebrityOption);
        CelebrityIsTaskPhaseFlash = Create(1180, false, CustomOptionType.Crewmate, "CelebrityIsTaskPhaseFlashSetting", false, CelebrityOption);
        CelebrityIsFlashWhileAlivingOnly = Create(1181, false, CustomOptionType.Crewmate, "CelebrityIsFlashWhileAlivingOnly", false, CelebrityIsTaskPhaseFlash);

        NocturnalityOption = SetupCustomRoleOption(303, true, RoleId.Nocturnality);
        NocturnalityPlayerCount = Create(304, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NocturnalityOption);

        ObserverOption = SetupCustomRoleOption(305, true, RoleId.Observer);
        ObserverPlayerCount = Create(306, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ObserverOption);

        VampireOption = SetupCustomRoleOption(307, false, RoleId.Vampire);
        VampirePlayerCount = Create(308, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VampireOption);
        VampireKillDelay = Create(309, false, CustomOptionType.Impostor, "VampireKillDelay", 10f, 1f, 60f, 0.5f, VampireOption, format: "unitSeconds");
        VampireViewBloodStainsTurn = Create(1074, false, CustomOptionType.Impostor, "VampireViewBloodStainsTurn", 1f, 1f, 15f, 1f, VampireOption, format: "unitSeconds");
        VampireCanCreateDependents = Create(1075, false, CustomOptionType.Impostor, "VampireCanCreateDependents", true, VampireOption);
        VampireCreateDependentsCoolTime = Create(1076, false, CustomOptionType.Impostor, "VampireCreateDependentsCoolTime", 30f, 2.5f, 120f, 2.5f, VampireCanCreateDependents);
        VampireDependentsKillCoolTime = Create(1077, false, CustomOptionType.Impostor, "VampireDependentsKillCoolTime", 30f, 2.5f, 120f, 2.5f, VampireCanCreateDependents);
        VampireDependentsCanVent = Create(1078, false, CustomOptionType.Impostor, "VampireDependentsCanVent", true, VampireCanCreateDependents);

        FoxOption = SetupCustomRoleOption(310, true, RoleId.Fox);
        FoxPlayerCount = Create(311, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FoxOption);
        FoxIsUseVent = Create(312, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, FoxOption);
        FoxIsImpostorLight = Create(313, true, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, FoxOption);
        FoxReport = Create(314, true, CustomOptionType.Neutral, "MinimalistReportSetting", true, FoxOption);

        DarkKillerOption = SetupCustomRoleOption(315, true, RoleId.DarkKiller);
        DarkKillerPlayerCount = Create(316, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DarkKillerOption);
        DarkKillerKillCoolTime = Create(317, true, CustomOptionType.Impostor, "DarkKillerKillCoolSetting", 20f, 2.5f, 60f, 2.5f, DarkKillerOption);

        SeerOption = SetupCustomRoleOption(318, true, RoleId.Seer);
        SeerPlayerCount = Create(319, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerOption);
        SeerMode = Create(320, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerOption);
        SeerLimitSoulDuration = Create(321, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerOption);
        SeerSoulDuration = Create(322, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerLimitSoulDuration, format: "unitCouples");

        EvilSeerOption = SetupCustomRoleOption(334, true, RoleId.EvilSeer);
        EvilSeerPlayerCount = Create(335, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilSeerOption);
        EvilSeerMode = Create(336, false, CustomOptionType.Impostor, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, EvilSeerOption);
        EvilSeerLimitSoulDuration = Create(337, false, CustomOptionType.Impostor, "SeerLimitSoulDuration", false, EvilSeerOption);
        EvilSeerSoulDuration = Create(338, false, CustomOptionType.Impostor, "SeerSoulDuration", 15f, 0f, 120f, 5f, EvilSeerLimitSoulDuration, format: "unitCouples");
        EvilSeerMadmateSetting = Create(1092, false, CustomOptionType.Impostor, "CreateMadmateSetting", false, EvilSeerOption);

        TeleportingJackalOption = SetupCustomRoleOption(339, false, RoleId.TeleportingJackal);
        TeleportingJackalPlayerCount = Create(340, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TeleportingJackalOption);
        TeleportingJackalKillCooldown = Create(341, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, TeleportingJackalOption, format: "unitSeconds");
        TeleportingJackalUseVent = Create(342, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, TeleportingJackalOption);
        TeleportingJackalIsImpostorLight = Create(343, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, TeleportingJackalOption);
        TeleportingJackalUseSabo = Create(344, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, TeleportingJackalOption);
        TeleportingJackalCoolTime = Create(345, false, CustomOptionType.Neutral, "TeleporterCooldownSetting", 30f, 2.5f, 60f, 2.5f, TeleportingJackalOption, format: "unitSeconds");
        TeleportingJackalDurationTime = Create(346, false, CustomOptionType.Neutral, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleportingJackalOption, format: "unitSeconds");

        DemonOption = SetupCustomRoleOption(351, true, RoleId.Demon);
        DemonPlayerCount = Create(352, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DemonOption);
        DemonCoolTime = Create(353, true, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, DemonOption, format: "unitSeconds");
        DemonIsUseVent = Create(354, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, DemonOption);
        DemonIsCheckImpostor = Create(355, true, CustomOptionType.Neutral, "MadmateIsCheckImpostorSetting", false, DemonOption);
        DemonIsAliveWin = Create(356, true, CustomOptionType.Neutral, "DemonIsAliveWinSetting", false, DemonOption);

        TaskManagerOption = SetupCustomRoleOption(357, true, RoleId.TaskManager);
        TaskManagerPlayerCount = Create(358, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TaskManagerOption);
        var taskmanageroption = SelectTask.TaskSetting(359, 360, 361, TaskManagerOption, CustomOptionType.Crewmate, true);
        TaskManagerCommonTask = taskmanageroption.Item1;
        TaskManagerShortTask = taskmanageroption.Item2;
        TaskManagerLongTask = taskmanageroption.Item3;

        SeerFriendsOption = SetupCustomRoleOption(362, true, RoleId.SeerFriends);
        SeerFriendsPlayerCount = Create(363, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerFriendsOption);
        SeerFriendsMode = Create(364, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerFriendsOption);
        SeerFriendsLimitSoulDuration = Create(365, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerFriendsOption);
        SeerFriendsSoulDuration = Create(366, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerFriendsLimitSoulDuration, format: "unitCouples");
        SeerFriendsIsUseVent = Create(367, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, SeerFriendsOption);
        SeerFriendsIsImpostorLight = Create(368, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, SeerFriendsOption);
        SeerFriendsIsCheckJackal = Create(369, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, SeerFriendsOption);
        var SeerFriendsoption = SelectTask.TaskSetting(371, 372, 373, SeerFriendsIsCheckJackal, CustomOptionType.Crewmate, true);
        SeerFriendsCommonTask = SeerFriendsoption.Item1;
        SeerFriendsShortTask = SeerFriendsoption.Item2;
        SeerFriendsLongTask = SeerFriendsoption.Item3;
        SeerFriendsCheckJackalTask = Create(374, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, SeerFriendsIsCheckJackal);

        JackalSeerOption = SetupCustomRoleOption(375, true, RoleId.JackalSeer);
        JackalSeerPlayerCount = Create(376, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalSeerOption);
        JackalSeerMode = Create(377, false, CustomOptionType.Neutral, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, JackalSeerOption);
        JackalSeerLimitSoulDuration = Create(378, false, CustomOptionType.Neutral, "SeerLimitSoulDuration", false, JackalSeerOption);
        JackalSeerSoulDuration = Create(379, false, CustomOptionType.Neutral, "SeerSoulDuration", 15f, 0f, 120f, 5f, JackalSeerLimitSoulDuration, format: "unitCouples");
        JackalSeerKillCooldown = Create(380, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, JackalSeerOption, format: "unitSeconds");
        JackalSeerUseVent = Create(381, true, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalSeerOption);
        JackalSeerUseSabo = Create(382, true, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalSeerOption);
        JackalSeerIsImpostorLight = Create(383, true, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, JackalSeerOption);
        JackalSeerCreateSidekick = Create(384, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalSeerOption);
        JackalSeerCreateFriend = Create(1143, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, JackalSeerOption);
        JackalSeerSKCooldown = Create(1109, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, JackalSeerCreateSidekick, format: "unitSeconds");
        JackalSeerNewJackalCreateSidekick = Create(385, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalSeerCreateSidekick);

        AssassinAndMarlinOption = new(386, true, CustomOptionType.Impostor, "AssassinAndMarlinName", Color.white, 1)
        {
            RoleId = RoleId.Assassin
        };
        AssassinPlayerCount = Create(387, true, CustomOptionType.Impostor, "AssassinSettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], AssassinAndMarlinOption);
        AssassinViewVote = Create(1145, true, CustomOptionType.Impostor, "GodViewVoteSetting", false, AssassinAndMarlinOption);
        MarlinPlayerCount = Create(388, true, CustomOptionType.Impostor, "MarlinSettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AssassinAndMarlinOption);
        MarlinViewVote = Create(1146, true, CustomOptionType.Impostor, "GodViewVoteSetting", false, AssassinAndMarlinOption);

        ArsonistOption = SetupCustomRoleOption(389, true, RoleId.Arsonist);
        ArsonistPlayerCount = Create(390, true, CustomOptionType.Neutral, "SettingPlayerCountName", AlonePlayers[0], AlonePlayers[1], AlonePlayers[2], AlonePlayers[3], ArsonistOption);
        ArsonistCoolTime = Create(391, true, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, ArsonistOption, format: "unitSeconds");
        ArsonistDurationTime = Create(392, true, CustomOptionType.Neutral, "ArsonistDurationTimeSetting", 3f, 0.5f, 10f, 0.5f, ArsonistOption, format: "unitSeconds");
        ArsonistIsUseVent = Create(393, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, ArsonistOption);

        CleanerOption = SetupCustomRoleOption(396, false, RoleId.Cleaner);
        CleanerPlayerCount = Create(397, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CleanerOption);
        CleanerKillCoolTime = Create(398, false, CustomOptionType.Impostor, "CleanerKillCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, CleanerOption, format: "unitSeconds");
        CleanerCooldown = Create(399, false, CustomOptionType.Impostor, "CleanerCooldownSetting", 60f, 40f, 70f, 2.5f, CleanerOption, format: "unitSeconds");

        MayorFriendsOption = SetupCustomRoleOption(405, true, RoleId.MayorFriends);
        MayorFriendsPlayerCount = Create(406, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorFriendsOption);
        MayorFriendsIsUseVent = Create(407, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MayorFriendsOption);
        MayorFriendsIsImpostorLight = Create(408, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MayorFriendsOption);
        MayorFriendsIsCheckJackal = Create(409, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, MayorFriendsOption);
        var MayorFriendsoption = SelectTask.TaskSetting(410, 411, 412, MayorFriendsIsCheckJackal, CustomOptionType.Crewmate, true);
        MayorFriendsCommonTask = MayorFriendsoption.Item1;
        MayorFriendsShortTask = MayorFriendsoption.Item2;
        MayorFriendsLongTask = MayorFriendsoption.Item3;
        MayorFriendsCheckJackalTask = Create(413, true, CustomOptionType.Crewmate, "MadmateCheckImpostorTaskSetting", rates4, MayorFriendsIsCheckJackal);
        MayorFriendsVoteCount = Create(414, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorFriendsOption);

        VentMakerOption = SetupCustomRoleOption(415, false, RoleId.VentMaker);
        VentMakerPlayerCount = Create(416, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VentMakerOption);

        SamuraiOption = SetupCustomRoleOption(417, true, RoleId.Samurai);
        SamuraiPlayerCount = Create(418, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SamuraiOption);
        SamuraiKillCoolTime = Create(419, true, CustomOptionType.Impostor, "SamuraiKillCoolSetting", 30f, 2.5f, 60f, 2.5f, SamuraiOption);
        SamuraiSwordCoolTime = Create(420, true, CustomOptionType.Impostor, "SamuraiSwordCoolSetting", 50f, 30f, 70f, 2.5f, SamuraiOption);
        SamuraiVent = Create(421, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, SamuraiOption);
        SamuraiSabo = Create(422, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, SamuraiOption);
        SamuraiScope = Create(423, true, CustomOptionType.Impostor, "SamuraiScopeSetting", 1f, 0.5f, 3f, 0.5f, SamuraiOption);

        EvilHackerOption = SetupCustomRoleOption(424, false, RoleId.EvilHacker);
        EvilHackerPlayerCount = Create(425, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilHackerOption);
        EvilHackerCanMoveWhenUsesAdmin = Create(1017, false, CustomOptionType.Impostor, "CanMoveWhenUsesAdmin", false, EvilHackerOption);
        EvilHackerMadmateSetting = Create(426, false, CustomOptionType.Impostor, "CreateMadmateSetting", false, EvilHackerOption);

        GhostMechanicOption = SetupCustomRoleOption(427, false, RoleId.GhostMechanic);
        GhostMechanicPlayerCount = Create(428, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GhostMechanicOption);
        GhostMechanicRepairLimit = Create(429, false, CustomOptionType.Crewmate, "GhostMechanicRepairLimitSetting", 1f, 1f, 30f, 1f, GhostMechanicOption);

        HauntedWolfOption = SetupCustomRoleOption(530, true, RoleId.HauntedWolf);
        HauntedWolfPlayerCount = Create(531, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HauntedWolfOption);

        PositionSwapperOption = SetupCustomRoleOption(609, false, RoleId.PositionSwapper);
        PositionSwapperPlayerCount = Create(610, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PositionSwapperOption);
        PositionSwapperSwapCount = Create(611, false, CustomOptionType.Impostor, "SettingPositionSwapperSwapCountName", 1f, 0f, 99f, 1f, PositionSwapperOption);
        PositionSwapperCoolTime = Create(616, false, CustomOptionType.Impostor, "SettingPositionSwapperSwapCoolTimeName", 2.5f, 2.5f, 90f, 2.5f, PositionSwapperOption);

        TunaOption = SetupCustomRoleOption(552, true, RoleId.Tuna);
        TunaPlayerCount = Create(553, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TunaOption);
        TunaStoppingTime = Create(554, true, CustomOptionType.Neutral, "TunaStoppingTimeSetting", 1f, 1f, 3f, 1f, TunaOption);
        TunaIsUseVent = Create(555, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, TunaOption);
        TunaIsAddWin = Create(671, true, CustomOptionType.Neutral, "TunaAddWinSetting", false, TunaOption);

        MafiaOption = SetupCustomRoleOption(602, true, RoleId.Mafia);
        MafiaPlayerCount = Create(603, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MafiaOption);

        SecretlyKillerOption = SetupCustomRoleOption(607, false, RoleId.SecretlyKiller);
        SecretlyKillerPlayerCount = Create(608, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SecretlyKillerOption);
        SecretlyKillerKillCoolTime = Create(632, false, CustomOptionType.Impostor, "SheriffCooldownSetting", 2.5f, 2.5f, 60f, 2.5f, SecretlyKillerOption);
        SecretlyKillerIsKillCoolTimeChange = Create(633, false, CustomOptionType.Impostor, "SettingCoolCharge", true, SecretlyKillerOption);
        SecretlyKillerIsBlackOutKillCharge = Create(634, false, CustomOptionType.Impostor, "SettingBlackoutCharge", false, SecretlyKillerOption);
        SecretlyKillerSecretKillLimit = Create(635, false, CustomOptionType.Impostor, "SettingLimitName", 1f, 0f, 99f, 1f, SecretlyKillerOption);
        SecretlyKillerSecretKillCoolTime = Create(636, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 45f, 2.5f, 60f, 2.5f, SecretlyKillerOption);

        SpyOption = SetupCustomRoleOption(614, true, RoleId.Spy);
        SpyPlayerCount = Create(615, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpyOption);
        SpyCanUseVent = Create(617, true, CustomOptionType.Crewmate, "JesterIsVentSetting", false, SpyOption);

        KunoichiOption = SetupCustomRoleOption(638, false, RoleId.Kunoichi);
        KunoichiPlayerCount = Create(639, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], KunoichiOption);
        KunoichiCoolTime = Create(640, false, CustomOptionType.Impostor, "KunoichiCoolTime", 2.5f, 0f, 15f, 0.5f, KunoichiOption);
        KunoichiKillKunai = Create(641, false, CustomOptionType.Impostor, "KunoichiKillKunai", 10f, 1f, 20f, 1f, KunoichiOption);
        KunoichiIsHide = Create(642, false, CustomOptionType.Impostor, "KunoichiIsHide", true, KunoichiOption);
        KunoichiIsWaitAndPressTheButtonToHide = Create(907, false, CustomOptionType.Impostor, "KunoichiIsWaitAndPressTheButtonToHide", true, KunoichiIsHide);
        KunoichiHideTime = Create(643, false, CustomOptionType.Impostor, "KunoichiHideTime", 3f, 0.5f, 10f, 0.5f, KunoichiIsHide);
        KunoichiHideKunai = Create(644, false, CustomOptionType.Impostor, "KunoichiHideKunai", false, KunoichiIsHide);

        DoubleKillerOption = SetupCustomRoleOption(647, false, RoleId.DoubleKiller);
        DoubleKillerPlayerCount = Create(648, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoubleKillerOption);
        MainKillCoolTime = Create(649, false, CustomOptionType.Impostor, "MainCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoubleKillerOption, format: "unitSeconds");
        SubKillCoolTime = Create(650, false, CustomOptionType.Impostor, "SubCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoubleKillerOption, format: "unitSeconds");
        DoubleKillerSabo = Create(651, false, CustomOptionType.Impostor, "DoubleKillerSaboSetting", false, DoubleKillerOption);
        DoubleKillerVent = Create(652, false, CustomOptionType.Impostor, "MinimalistVentSetting", false, DoubleKillerOption);

        SmasherOption = SetupCustomRoleOption(653, false, RoleId.Smasher);
        SmasherPlayerCount = Create(654, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SmasherOption);
        SmasherKillCoolTime = Create(655, false, CustomOptionType.Impostor, "KillCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, SmasherOption, format: "unitSeconds");

        SuicideWisherOption = SetupCustomRoleOption(678, true, RoleId.SuicideWisher);
        SuicideWisherPlayerCount = Create(679, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SuicideWisherOption);

        NeetOption = SetupCustomRoleOption(680, false, RoleId.Neet);
        NeetPlayerCount = Create(659, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NeetOption);
        NeetIsAddWin = Create(683, false, CustomOptionType.Neutral, "TunaAddWinSetting", false, NeetOption);

        FastMakerOption = SetupCustomRoleOption(676, true, RoleId.FastMaker);
        FastMakerPlayerCount = Create(661, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FastMakerOption);

        ToiletFanOption = SetupCustomRoleOption(656, true, RoleId.ToiletFan);
        ToiletFanPlayerCount = Create(657, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ToiletFanOption);
        ToiletFanCoolTime = Create(658, true, CustomOptionType.Crewmate, "ToiletCooldownSetting", 30f, 0f, 60f, 2.5f, ToiletFanOption);

        EvilButtonerOption = SetupCustomRoleOption(801, true, RoleId.EvilButtoner);
        EvilButtonerPlayerCount = Create(802, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilButtonerOption);
        EvilButtonerCoolTime = Create(803, false, CustomOptionType.Impostor, "ButtonerCooldownSetting", 20f, 2.5f, 60f, 2.5f, EvilButtonerOption, format: "unitSeconds");//クールタイムはSHR未対応の為false
        EvilButtonerCount = Create(804, true, CustomOptionType.Impostor, "ButtonerCountSetting", 1f, 1f, 10f, 1f, EvilButtonerOption);

        NiceButtonerOption = SetupCustomRoleOption(805, true, RoleId.NiceButtoner);
        NiceButtonerPlayerCount = Create(806, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceButtonerOption);
        NiceButtonerCoolTime = Create(807, false, CustomOptionType.Crewmate, "ButtonerCooldownSetting", 20f, 2.5f, 60f, 2.5f, NiceButtonerOption, format: "unitSeconds");//クールタイムはSHR未対応の為false
        NiceButtonerCount = Create(808, true, CustomOptionType.Crewmate, "ButtonerCountSetting", 1f, 1f, 10f, 1f, NiceButtonerOption);

        SpelunkerOption = SetupCustomRoleOption(809, false, RoleId.Spelunker);
        SpelunkerPlayerCount = Create(810, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpelunkerOption);
        SpelunkerVentDeathChance = Create(811, false, CustomOptionType.Neutral, "SpelunkerVentDeathChance", rates, SpelunkerOption);
        SpelunkerLadderDeadChance = Create(812, false, CustomOptionType.Neutral, "LadderDeadChance", rates, SpelunkerOption);
        SpelunkerIsDeathCommsOrPowerdown = Create(813, false, CustomOptionType.Neutral, "SpelunkerIsDeathCommsOrPowerdown", true, SpelunkerOption);
        SpelunkerDeathCommsOrPowerdownTime = Create(814, false, CustomOptionType.Neutral, "SpelunkerDeathCommsOrPowerdownTime", 20f, 0f, 120f, 2.5f, SpelunkerIsDeathCommsOrPowerdown);
        SpelunkerLiftDeathChance = Create(815, false, CustomOptionType.Neutral, "SpelunkerLiftDeathChance", rates, SpelunkerOption);
        SpelunkerDoorOpenChance = Create(816, false, CustomOptionType.Neutral, "SpelunkerDoorOpenChance", rates, SpelunkerOption);

        FinderOption = SetupCustomRoleOption(817, true, RoleId.Finder);
        FinderPlayerCount = Create(818, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FinderOption);
        FinderCheckMadmateSetting = Create(819, true, CustomOptionType.Impostor, "FinderCheckMadmateSetting", 3f, 1f, 15f, 1f, FinderOption);

        RevolutionistAndDictatorOption = new(820, false, CustomOptionType.Neutral, "RevolutionistAndDictatorName", Color.white, 1)
        {
            RoleId = RoleId.Revolutionist
        };
        RevolutionistPlayerCount = Create(821, false, CustomOptionType.Neutral, "SettingRevolutionistPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RevolutionistAndDictatorOption);
        DictatorPlayerCount = Create(822, false, CustomOptionType.Neutral, "SettingDictatorPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RevolutionistAndDictatorOption);
        DictatorVoteCount = Create(823, false, CustomOptionType.Neutral, "DictatorVoteCount", 2f, 1f, 100f, 1f, RevolutionistAndDictatorOption);
        DictatorSubstituteExile = Create(824, false, CustomOptionType.Neutral, "DictatorSubExile", false, RevolutionistAndDictatorOption);
        DictatorSubstituteExileLimit = Create(825, false, CustomOptionType.Neutral, "DictatorSubExileLimit", 1f, 1f, 15f, 1f, DictatorSubstituteExile);
        RevolutionistCoolTime = Create(826, false, CustomOptionType.Neutral, "RevolutionCoolTime", 10f, 2.5f, 60f, 2.5f, RevolutionistAndDictatorOption);
        RevolutionistTouchTime = Create(827, false, CustomOptionType.Neutral, "RevolutionTouchTime", 1f, 0f, 10f, 0.5f, RevolutionistAndDictatorOption);
        RevolutionistAddWin = Create(828, false, CustomOptionType.Neutral, "RevolutionistAddWin", false, RevolutionistAndDictatorOption);
        RevolutionistAddWinIsAlive = Create(829, false, CustomOptionType.Neutral, "RevolutionistAddWinIsAlive", true, RevolutionistAddWin);

        SuicidalIdeationOption = SetupCustomRoleOption(830, false, RoleId.SuicidalIdeation);
        SuicidalIdeationPlayerCount = Create(831, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SuicidalIdeationOption);
        SuicidalIdeationWinText = Create(832, false, CustomOptionType.Neutral, "SuicidalIdeationWinTextSetting", false, SuicidalIdeationOption);
        SuicidalIdeationTimeLeft = Create(833, false, CustomOptionType.Neutral, "SuicidalIdeationTimeLeftSetting", 90f, 30f, 600f, 5f, SuicidalIdeationOption, format: "unitSeconds");
        SuicidalIdeationAddTimeLeft = Create(834, false, CustomOptionType.Neutral, "SuicidalIdeationAddTimeLeftSetting", 20f, 0f, 300f, 5f, SuicidalIdeationOption, format: "unitSeconds");
        SuicidalIdeationFallProbability = Create(835, false, CustomOptionType.Neutral, "SuicidalIdeationFallProbabilitySetting", rates, SuicidalIdeationOption);
        var SuicidalIdeationoption = SelectTask.TaskSetting(836, 837, 838, SuicidalIdeationOption, CustomOptionType.Neutral, false);
        SuicidalIdeationCommonTask = SuicidalIdeationoption.Item1;
        SuicidalIdeationShortTask = SuicidalIdeationoption.Item2;
        SuicidalIdeationLongTask = SuicidalIdeationoption.Item3;

        NunOption = SetupCustomRoleOption(958, false, RoleId.Nun);
        NunPlayerCount = Create(959, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], NunOption);
        NunCoolTime = Create(960, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, NunOption);


        PartTimerOption = SetupCustomRoleOption(961, false, RoleId.PartTimer);
        PartTimerPlayerCount = Create(962, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PartTimerOption);
        PartTimerDeathTurn = Create(963, false, CustomOptionType.Neutral, "PartTimerDeathTurn", 3f, 0f, 15f, 1f, PartTimerOption);
        PartTimerCoolTime = Create(964, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PartTimerOption);
        PartTimerIsCheckTargetRole = Create(965, false, CustomOptionType.Neutral, "PartTimerIsCheckTargetRole", true, PartTimerOption);

        SluggerOption = SetupCustomRoleOption(901, false, RoleId.Slugger);
        SluggerPlayerCount = Create(902, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SluggerOption);
        SluggerChargeTime = Create(903, false, CustomOptionType.Impostor, "SluggerChargeTime", 3f, 0f, 30f, 0.5f, SluggerOption);
        SluggerCoolTime = Create(904, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, SluggerOption);
        SluggerIsMultiKill = Create(905, false, CustomOptionType.Impostor, "SluggerIsMultiKill", false, SluggerOption);
        SluggerIsKillCoolSync = Create(1030, false, CustomOptionType.Impostor, "IsSyncKillCoolTime", false, SluggerOption);

        PainterOption = SetupCustomRoleOption(941, false, RoleId.Painter);
        PainterPlayerCount = Create(942, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PainterOption);
        PainterCoolTime = Create(952, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PainterOption);
        PainterIsTaskCompleteFootprint = Create(943, false, CustomOptionType.Crewmate, "PainterIsTaskCompleteFootprint", true, PainterOption);
        PainterIsSabotageRepairFootprint = Create(944, false, CustomOptionType.Crewmate, "PainterIsSabotageRepairFootprint", true, PainterOption);
        PainterIsInVentFootprint = Create(945, false, CustomOptionType.Crewmate, "PainterIsInVentFootprint", true, PainterOption);
        PainterIsExitVentFootprint = Create(946, false, CustomOptionType.Crewmate, "PainterIsExitVentFootprint", true, PainterOption);
        PainterIsCheckVitalFootprint = Create(947, false, CustomOptionType.Crewmate, "PainterIsCheckVitalFootprint", false, PainterOption);
        PainterIsCheckAdminFootprint = Create(948, false, CustomOptionType.Crewmate, "PainterIsCheckAdminFootprint", false, PainterOption);
        PainterIsDeathFootprint = Create(949, false, CustomOptionType.Crewmate, "PainterIsDeathFootprint", true, PainterOption);
        PainterIsDeathFootprintBig = Create(950, false, CustomOptionType.Crewmate, "PainterIsDeathFootprintBig", true, PainterIsDeathFootprint);
        PainterIsFootprintMeetingDestroy = Create(951, false, CustomOptionType.Crewmate, "PainterIsFootprintMeetingDestroy", true, PainterOption);

        HitmanOption = SetupCustomRoleOption(839, false, RoleId.Hitman);
        HitmanPlayerCount = Create(840, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HitmanOption);
        HitmanKillCoolTime = Create(841, false, CustomOptionType.Neutral, "SheriffCooldownSetting", 20f, 0f, 120f, 2.5f, HitmanOption);
        HitmanChangeTargetTime = Create(842, false, CustomOptionType.Neutral, "HitmanChangeTargetTime", 60f, 0f, 240f, 2.5f, HitmanOption);
        HitmanIsOutMission = Create(843, false, CustomOptionType.Neutral, "HitmanIsOutMission", true, HitmanOption);
        HitmanOutMissionLimit = Create(845, false, CustomOptionType.Neutral, "HitmanOutMissionLimit", 5f, 1f, 30f, 1f, HitmanIsOutMission);
        HitmanWinKillCount = Create(846, false, CustomOptionType.Neutral, "HitmanWinKillCount", 5f, 1f, 15f, 1f, HitmanOption);
        HitmanIsArrowView = Create(847, false, CustomOptionType.Neutral, "HitmanIsTargetArrow", true, HitmanOption);
        HitmanArrowUpdateTime = Create(848, false, CustomOptionType.Neutral, "HitmanUpdateTargetArrowTime", 0f, 0f, 120f, 2.5f, HitmanIsArrowView);

        MatryoshkaOption = SetupCustomRoleOption(849, false, RoleId.Matryoshka);
        MatryoshkaPlayerCount = Create(850, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MatryoshkaOption);
        MatryoshkaWearLimit = Create(851, false, CustomOptionType.Impostor, "MatryoshkaWearLimit", 3f, 1f, 15f, 1f, MatryoshkaOption);
        MatryoshkaWearReport = Create(852, false, CustomOptionType.Impostor, "MatryoshkaWearReport", true, MatryoshkaOption);
        MatryoshkaWearTime = Create(853, false, CustomOptionType.Impostor, "MatryoshkaWearTime", 7.5f, 0.5f, 60f, 0.5f, MatryoshkaOption);
        MatryoshkaAddKillCoolTime = Create(854, false, CustomOptionType.Impostor, "MatryoshkaAddKillCoolTime", 2.5f, 0f, 30f, 0.5f, MatryoshkaOption);
        MatryoshkaCoolTime = Create(855, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 0f, 180f, 2.5f, MatryoshkaOption);

        SeeThroughPersonOption = SetupCustomRoleOption(864, false, RoleId.SeeThroughPerson);
        SeeThroughPersonPlayerCount = Create(865, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeeThroughPersonOption);

        PhotographerOption = SetupCustomRoleOption(866, false, RoleId.Photographer);
        PhotographerPlayerCount = Create(867, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PhotographerOption);
        PhotographerCoolTime = Create(868, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PhotographerOption);
        PhotographerIsBonus = Create(869, false, CustomOptionType.Neutral, "PhotographerIsBonus", true, PhotographerOption);
        PhotographerBonusCount = Create(870, false, CustomOptionType.Neutral, "PhotographerBonusCount", 5f, 1f, 15f, 1f, PhotographerIsBonus);
        PhotographerBonusCoolTime = Create(871, false, CustomOptionType.Neutral, "PhotographerBonusCoolTime", 20f, 2.5f, 60f, 2.5f, PhotographerIsBonus);
        PhotographerIsImpostorVision = Create(872, false, CustomOptionType.Neutral, "PhotographerIsImpostorVision", false, PhotographerOption);
        PhotographerIsNotification = Create(873, false, CustomOptionType.Neutral, "PhotographerIsNotification", true, PhotographerOption);

        StefinderOption = SetupCustomRoleOption(876, false, RoleId.Stefinder);
        StefinderPlayerCount = Create(877, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StefinderOption);
        StefinderKillCooldown = Create(878, false, CustomOptionType.Neutral, "StefinderKillCooldownSetting", 30f, 0f, 120f, 2.5f, StefinderOption, format: "unitSeconds");
        StefinderVent = Create(879, false, CustomOptionType.Neutral, "StefinderVentSetting", false, StefinderOption);
        StefinderSabo = Create(880, false, CustomOptionType.Neutral, "StefinderSaboSetting", false, StefinderOption);
        StefinderSoloWin = Create(881, false, CustomOptionType.Neutral, "StefinderSoloWinSetting", false, StefinderOption);

        PsychometristOption = SetupCustomRoleOption(883, false, RoleId.Psychometrist);
        PsychometristPlayerCount = Create(884, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PsychometristOption);
        PsychometristCoolTime = Create(885, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PsychometristOption);
        PsychometristReadTime = Create(886, false, CustomOptionType.Crewmate, "PsychometristReadTime", 5f, 0f, 15f, 0.5f, PsychometristOption);
        PsychometristIsCheckDeathTime = Create(887, false, CustomOptionType.Crewmate, "PsychometristIsCheckDeathTime", true, PsychometristOption);
        PsychometristDeathTimeDeviation = Create(888, false, CustomOptionType.Crewmate, "PsychometristDeathTimeDeviation", 3f, 0f, 30f, 1f, PsychometristIsCheckDeathTime);
        PsychometristIsCheckDeathReason = Create(889, false, CustomOptionType.Crewmate, "PsychometristIsCheckDeathReason", true, PsychometristOption);
        PsychometristIsCheckFootprints = Create(890, false, CustomOptionType.Crewmate, "PsychometristIsCheckFootprints", true, PsychometristOption);
        PsychometristCanCheckFootprintsTime = Create(891, false, CustomOptionType.Crewmate, "PsychometristCanCheckFootprintsTime", 7.5f, 0.5f, 60f, 0.5f, PsychometristIsCheckFootprints);
        PsychometristIsReportCheckedDeadBody = Create(892, false, CustomOptionType.Crewmate, "PsychometristIsReportCheckedDeadBody", false, PsychometristOption);

        ShiftActor.SetupCustomOptions();

        NekoKabocha.SetupCustomOptions();

        CrackerOption = SetupCustomRoleOption(1038, false, RoleId.Cracker);
        CrackerPlayerCount = Create(1031, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CrackerOption);
        CrackerCoolTime = Create(1032, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, CrackerOption);
        CrackerIsAdminView = Create(1033, false, CustomOptionType.Impostor, "CrackerIsAdminView", false, CrackerOption);
        CrackerIsVitalsView = Create(1034, false, CustomOptionType.Impostor, "CrackerIsVitalsView", false, CrackerOption);
        CrackerOneTurnSelectCount = Create(1035, false, CustomOptionType.Impostor, "CrackerOneTurnSelectCount", 1f, 1f, 15f, 1f, CrackerOption);
        CrackerAllTurnSelectCount = Create(1036, false, CustomOptionType.Impostor, "CrackerAllTurnSelectCount", 3f, 1f, 100f, 1f, CrackerOption);
        CrackerIsSelfNone = Create(1037, false, CustomOptionType.Impostor, "CrackerIsSelfNone", true, CrackerOption);

        ConnectKillerOption = SetupCustomRoleOption(982, false, RoleId.ConnectKiller);
        ConnectKillerPlayerCount = Create(983, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], ConnectKillerOption);

        DoppelgangerOption = SetupCustomRoleOption(986, true, RoleId.Doppelganger);
        DoppelgangerPlayerCount = Create(987, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoppelgangerOption);
        DoppelgangerDurationTime = Create(988, true, CustomOptionType.Impostor, "DoppelgangerDurationTimeSetting", 90f, 0f, 250f, 5f, DoppelgangerOption);
        DoppelgangerCoolTime = Create(989, true, CustomOptionType.Impostor, "DoppelgangerCooldownSetting", 5f, 5f, 60f, 2.5f, DoppelgangerOption);
        DoppelgangerSucTime = Create(990, true, CustomOptionType.Impostor, "DoppelgangerSucTimeSetting", 2.5f, 0f, 120f, 2.5f, DoppelgangerOption);
        DoppelgangerNotSucTime = Create(991, true, CustomOptionType.Impostor, "DoppelgangerNotSucTimeSetting", 40f, 0f, 120f, 2.5f, DoppelgangerOption);

        WerewolfOption = new(1057, false, CustomOptionType.Impostor, "WerewolfName", RoleClass.Werewolf.color, 1);
        WerewolfPlayerCount = Create(1051, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], WerewolfOption);

        Knight.SetupCustomOptions();

        (PavlovsownerOption = new(1039, false, CustomOptionType.Neutral, "PavlovsdogsName", RoleClass.Pavlovsdogs.color, 1))
        .RoleId = RoleId.Pavlovsowner;
        PavlovsownerPlayerCount = Create(1040, false, CustomOptionType.Neutral, "SettingPlayerCountName", AlonePlayers[0], AlonePlayers[1], AlonePlayers[2], AlonePlayers[3], PavlovsownerOption);
        PavlovsownerCreateCoolTime = Create(1041, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, PavlovsownerOption);
        PavlovsownerCreateDogLimit = Create(1042, false, CustomOptionType.Neutral, "PavlovsownerCreateDogLimit", 1f, 1f, 15f, 1f, PavlovsownerOption);
        PavlovsownerIsTargetImpostorDeath = Create(1043, false, CustomOptionType.Neutral, "PavlovsownerIsTargetImpostorDeath", true, PavlovsownerOption);
        PavlovsdogIsImpostorView = Create(1044, false, CustomOptionType.Neutral, "PavlovsdogIsImpostorView", true, PavlovsownerOption);
        PavlovsdogKillCoolTime = Create(1045, false, CustomOptionType.Neutral, "SheriffCooldownSetting", 30f, 2.5f, 120f, 2.5f, PavlovsownerOption);
        PavlovsdogCanVent = Create(1046, false, CustomOptionType.Neutral, "MadmateUseVentSetting", true, PavlovsownerOption);
        PavlovsdogRunAwayKillCoolTime = Create(1047, false, CustomOptionType.Neutral, "PavlovsdogRunAwayKillCoolTime", 20f, 2.5f, 60f, 2.5f, PavlovsownerOption);
        PavlovsdogRunAwayDeathTime = Create(1048, false, CustomOptionType.Neutral, "PavlovsdogRunAwayDeathTime", 60f, 2.5f, 180f, 2.5f, PavlovsownerOption);
        PavlovsdogRunAwayDeathTimeIsMeetingReset = Create(1049, false, CustomOptionType.Neutral, "PavlovsdogRunAwayDeathTimeIsMeetingReset", true, PavlovsownerOption);

        WaveCannonOption = SetupCustomRoleOption(1019, false, RoleId.WaveCannon, CustomOptionType.Impostor);
        WaveCannonPlayerCount = Create(1018, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], WaveCannonOption);
        WaveCannonCoolTime = Create(1020, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 20f, 2.5f, 180f, 2.5f, WaveCannonOption);
        WaveCannonChargeTime = Create(1021, false, CustomOptionType.Impostor, "WaveCannonChargeTime", 3f, 0.5f, 15f, 0.5f, WaveCannonOption);
        WaveCannonIsSyncKillCoolTime = Create(1016, false, CustomOptionType.Impostor, "IsSyncKillCoolTime", false, WaveCannonOption);

        WaveCannonJackal.SetupCustomOptions();

        Conjurer.SetupCustomOptions();

        TaskerOption = SetupCustomRoleOption(1006, false, RoleId.Tasker);
        TaskerPlayerCount = Create(1007, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoppelgangerOption);
        var taskeroption = SelectTask.TaskSetting(1008, 1009, 1010, TaskerOption, CustomOptionType.Impostor, false);
        TaskerCommonTask = taskeroption.Item1;
        TaskerShortTask = taskeroption.Item2;
        TaskerLongTask = taskeroption.Item3;
        TaskerIsKillCoolTaskNow = Create(1011, false, CustomOptionType.Impostor, "TaskerIsKillCoolTaskNow", true, TaskerOption);
        TaskerCanKill = Create(1012, false, CustomOptionType.Impostor, "TaskerCanKill", true, TaskerOption);

        CamouflagerOption = SetupCustomRoleOption(1059, false, RoleId.Camouflager);
        CamouflagerPlayerCount = Create(1060, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CamouflagerOption);
        CamouflagerCoolTime = Create(1061, false, CustomOptionType.Impostor, "CamouflagerCoolTimeSetting", 30f, 0f, 60f, 2.5f, CamouflagerOption);
        CamouflagerDurationTime = Create(1062, false, CustomOptionType.Impostor, "CamouflagerDurationTimeSetting", 10f, 0f, 60f, 2.5f, CamouflagerOption);
        CamouflagerCamouflageArsonist = Create(1063, false, CustomOptionType.Impostor, "CamouflagerCamouflageArsonistSetting", true, CamouflagerOption);
        CamouflagerCamouflageDemon = Create(1064, false, CustomOptionType.Impostor, "CamouflagerCamouflageDemonSetting", true, CamouflagerOption);
        CamouflagerCamouflageLovers = Create(1065, false, CustomOptionType.Impostor, "CamouflagerCamouflageLoversSetting", false, CamouflagerOption);
        CamouflagerCamouflageQuarreled = Create(1066, false, CustomOptionType.Impostor, "CamouflagerCamouflageQuarreledSetting", false, CamouflagerOption);
        CamouflagerCamouflageChangeColor = Create(1067, false, CustomOptionType.Impostor, "CamouflagerCamouflageChangeColorSetting", false, CamouflagerOption);
        CamouflagerCamouflageColor = Create(1068, false, CustomOptionType.Impostor, "CamouflagerCamouflageColorSetting", Camouflager.ColorOption, CamouflagerCamouflageChangeColor);

        NiceGuesserOption = SetupCustomRoleOption(1069, false, RoleId.NiceGuesser);
        NiceGuesserPlayerCount = Create(1070, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceGuesserOption);
        NiceGuesserShortMaxCount = Create(977, false, CustomOptionType.Crewmate, "EvilGuesserShortMaxCountSetting", 2f, 1f, 15f, 1f, NiceGuesserOption);
        NiceGuesserShortOneMeetingCount = Create(978, false, CustomOptionType.Crewmate, "EvilGuesserOneMeetingShortSetting", true, NiceGuesserOption);
        NiceGuesserCanShotCrew = Create(1266, false, CustomOptionType.Crewmate, "EvilGuesserCanCrewShotSetting", true, NiceGuesserOption);

        EvilGuesserOption = SetupCustomRoleOption(1071, false, RoleId.EvilGuesser);
        EvilGuesserPlayerCount = Create(974, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGuesserOption);
        EvilGuesserShortMaxCount = Create(975, false, CustomOptionType.Impostor, "EvilGuesserShortMaxCountSetting", 2f, 1f, 15f, 1f, EvilGuesserOption);
        EvilGuesserShortOneMeetingCount = Create(976, false, CustomOptionType.Impostor, "EvilGuesserOneMeetingShortSetting", true, EvilGuesserOption);
        EvilGuesserCanShotCrew = Create(1267, false, CustomOptionType.Impostor, "EvilGuesserCanCrewShotSetting", true, EvilGuesserOption);

        CupidOption = SetupCustomRoleOption(1079, false, RoleId.Cupid);
        CupidPlayerCount = Create(1080, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], CupidOption);
        CupidCoolTime = Create(1081, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 180f, 2.5f, CupidOption);

        HamburgerShopOption = SetupCustomRoleOption(1091, true, RoleId.HamburgerShop);
        HamburgerShopPlayerCount = Create(1093, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HamburgerShopOption);
        HamburgerShopChangeTaskPrefab = Create(1094, false, CustomOptionType.Crewmate, "HamburgerShopChangeTaskPrefab", true, HamburgerShopOption);
        var HamburgerShopoption = SelectTask.TaskSetting(1095, 1096, 1097, HamburgerShopOption, CustomOptionType.Crewmate, true);
        HamburgerShopCommonTask = HamburgerShopoption.Item1;
        HamburgerShopShortTask = HamburgerShopoption.Item2;
        HamburgerShopLongTask = HamburgerShopoption.Item3;

        PenguinOption = SetupCustomRoleOption(1082, false, RoleId.Penguin);
        PenguinPlayerCount = Create(1087, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PenguinOption);
        PenguinCoolTime = Create(1088, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, PenguinOption, format: "unitSeconds");
        PenguinDurationTime = Create(1089, false, CustomOptionType.Impostor, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, PenguinOption, format: "unitSeconds");
        PenguinCanDefaultKill = Create(1090, false, CustomOptionType.Impostor, "PenguinCanDefaultKill", false, PenguinOption);
        PenguinMeetingKill = Create(1251, false, CustomOptionType.Impostor, "PenguinMeetingKill", true, PenguinOption);

        LoversBreakerOption = SetupCustomRoleOption(1132, false, RoleId.LoversBreaker);
        LoversBreakerPlayerCount = Create(1133, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LoversBreakerOption);
        LoversBreakerBreakCount = Create(1134, false, CustomOptionType.Neutral, "LoversBreakerBreakCount", 1f, 1f, 7f, 1f, LoversBreakerOption);
        LoversBreakerCoolTime = Create(1135, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, LoversBreakerOption, format: "unitSeconds");
        LoversBreakerIsDeathWin = Create(1136, false, CustomOptionType.Neutral, "LoversBreakerIsDeathWin", true, LoversBreakerOption);

        JumboOption = SetupCustomRoleOption(1137, false, RoleId.Jumbo, type: CustomOptionType.Neutral);
        JumboPlayerCount = Create(1138, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JumboOption);
        JumboCrewmateChance = Create(1139, false, CustomOptionType.Neutral, "JumboCrewmateChance", rates, JumboOption);
        JumboMaxSize = Create(1140, false, CustomOptionType.Neutral, "JumboMaxSize", 24f, 1f, 48f, 1f, JumboOption);
        JumboSpeedUpSize = Create(1141, false, CustomOptionType.Neutral, "JumboSpeedUpSize", 300f, 10f, 600f, 10f, JumboOption);
        JumboWalkSoundSize = Create(1142, false, CustomOptionType.Neutral, "JumboWalkSoundSize", rates, JumboOption);

        Safecracker.SetupCustomOptions();

        FireFox.SetupCustomOptions();

        Squid.SetupCustomOptions();

        DyingMessenger.SetupCustomOptions();
        NiceMechanic.SetupCustomOptions();

        EvilMechanic.SetupCustomOptions();
        TheThreeLittlePigs.SetupCustomOptions();

        OrientalShaman.SetupCustomOptions();

        WiseMan.SetupCustomOptions();

        RoleBaseHelper.SetUpOptions();

        Balancer.SetupCustomOptions();

        // 表示設定

        QuarreledOption = Create(432, true, CustomOptionType.Neutral, Cs(RoleClass.Quarreled.color, "QuarreledName"), false, null, isHeader: true);
        QuarreledTeamCount = Create(433, true, CustomOptionType.Neutral, "QuarreledTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], QuarreledOption);
        QuarreledOnlyCrewmate = Create(434, true, CustomOptionType.Neutral, "QuarreledOnlyCrewmateSetting", false, QuarreledOption);

        LoversOption = Create(435, true, CustomOptionType.Neutral, Cs(RoleClass.Lovers.color, "LoversName"), false, null, isHeader: true);
        LoversTeamCount = Create(436, true, CustomOptionType.Neutral, "LoversTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], LoversOption);
        LoversPar = Create(437, true, CustomOptionType.Neutral, "LoversParSetting", rates, LoversOption);
        LoversOnlyCrewmate = Create(438, true, CustomOptionType.Neutral, "LoversOnlyCrewmateSetting", false, LoversOption);
        LoversSingleTeam = Create(439, true, CustomOptionType.Neutral, "LoversSingleTeamSetting", true, LoversOption);
        LoversSameDie = Create(440, true, CustomOptionType.Neutral, "LoversSameDieSetting", true, LoversOption);
        LoversAliveTaskCount = Create(441, true, CustomOptionType.Neutral, "LoversAliveTaskCountSetting", false, LoversOption);
        LoversDuplicationQuarreled = Create(442, true, CustomOptionType.Neutral, "LoversDuplicationQuarreledSetting", true, LoversOption);
        var loversoption = SelectTask.TaskSetting(443, 444, 445, LoversOption, CustomOptionType.Neutral, true);
        LoversCommonTask = loversoption.Item1;
        LoversShortTask = loversoption.Item2;
        LoversLongTask = loversoption.Item3;

        /* |: ========================= Roles Settings ========================== :| */

        SuperNewRolesPlugin.Logger.LogInfo("設定のidのMax:" + Max);
        SuperNewRolesPlugin.Logger.LogInfo("設定数:" + options.Count);
    }
}