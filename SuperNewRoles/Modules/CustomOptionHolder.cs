using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;

namespace SuperNewRoles.Modules;

public class CustomOptionHolder
{
    public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

    public static string[] rates4 = new string[] { "0%", "25%", "50%", "75%", "100%" };
    public static string[] ratesper5 = new string[] { "0%", "5%", "10%", "15%", "20%", "25%", "30%", "35%", "40%", "45%", "50%", "55%", "60%", "65%", "70%", "75%", "80%", "85%", "90%", "95%", "100%" };
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
    public static CustomOption CanUseChatWhenTaskPhase;
    public static CustomOption DebugModeFastStart;
    public static CustomOption IsMurderPlayerAnnounce;

    public static CustomOption DisconnectNotPCOption;
    public static CustomOption DisconnectDontHaveFriendCodeOption;

    public static CustomOption SNRWebSendConditionHostDependency;

    public static CustomOption ProhibitModColor;

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
    public static CustomOption JesterIsSettingNumberOfUniqueTasks;
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

    public static CustomRoleOption SheriffOption;
    public static CustomOption SheriffPlayerCount;
    public static CustomOption SheriffCoolTime;
    public static CustomOption SheriffKillMaxCount;
    public static CustomOption SheriffCanKillImpostor;
    public static CustomOption SheriffExecutionMode;
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
    public static CustomOption RemoteSheriffExecutionMode;
    public static CustomOption RemoteSheriffMadRoleKill;
    public static CustomOption RemoteSheriffNeutralKill;
    public static CustomOption RemoteSheriffFriendRolesKill;
    public static CustomOption RemoteSheriffLoversKill;
    public static CustomOption RemoteSheriffQuarreledKill;
    public static CustomOption RemoteSheriffKillMaxCount;
    public static CustomOption RemoteSheriffIsKillTeleportSetting;

    public static CustomRoleOption MeetingSheriffOption;
    public static CustomOption MeetingSheriffPlayerCount;
    public static CustomOption MeetingSheriffExecutionMode;
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
    public static CustomOption TaskerIsSettingNumberOfUniqueTasks;
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

    public static CustomRoleOption VultureOption;
    public static CustomOption VulturePlayerCount;
    public static CustomOption VultureCooldown;
    public static CustomOption VultureDeadBodyMaxCount;
    public static CustomOption VultureIsUseVent;
    public static CustomOption VultureShowArrows;

    public static CustomRoleOption ClergymanOption;
    public static CustomOption ClergymanPlayerCount;
    public static CustomOption ClergymanCoolTime;
    public static CustomOption ClergymanDurationTime;
    public static CustomOption ClergymanDownVision;

    public static CustomRoleOption MadmateOption;
    public static CustomOption MadmatePlayerCount;
    public static CustomOption MadmateIsCheckImpostor;
    public static CustomOption MadmateIsSettingNumberOfUniqueTasks;
    public static CustomOption MadmateCommonTask;
    public static CustomOption MadmateShortTask;
    public static CustomOption MadmateLongTask;
    public static CustomOption MadmateIsParcentageForTaskTrigger;
    public static CustomOption MadmateParcentageForTaskTriggerSetting;
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
    public static CustomOption GodIsSettingNumberOfUniqueTasks;
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
    public static CustomOption JackalFriendsIsSettingNumberOfUniqueTasks;
    public static CustomOption JackalFriendsCommonTask;
    public static CustomOption JackalFriendsShortTask;
    public static CustomOption JackalFriendsLongTask;
    public static CustomOption JackalFriendsIsParcentageForTaskTrigger;
    public static CustomOption JackalFriendsParcentageForTaskTriggerSetting;
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

    public static CustomRoleOption EvilEraserOption;
    public static CustomOption EvilEraserPlayerCount;
    public static CustomOption EvilEraserMaxCount;

    public static CustomRoleOption WorkpersonOption;
    public static CustomOption WorkpersonPlayerCount;
    public static CustomOption WorkpersonIsAliveWin;
    public static CustomOption WorkpersonIsSettingNumberOfUniqueTasks;
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
    public static CustomOption AmnesiacShowArrows;

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
    public static CustomOption MadMayorIsSettingNumberOfUniqueTasks;
    public static CustomOption MadMayorShortTask;
    public static CustomOption MadMayorLongTask;
    public static CustomOption MadMayorIsParcentageForTaskTrigger;
    public static CustomOption MadMayorParcentageForTaskTriggerSetting;
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
    public static CustomOption MadStuntManIsSettingNumberOfUniqueTasks;
    public static CustomOption MadStuntManCommonTask;
    public static CustomOption MadStuntManShortTask;
    public static CustomOption MadStuntManLongTask;
    public static CustomOption MadStuntManIsParcentageForTaskTrigger;
    public static CustomOption MadStuntManParcentageForTaskTriggerSetting;
    public static CustomOption MadStuntManMaxGuardCount;

    public static CustomRoleOption MadHawkOption;
    public static CustomOption MadHawkPlayerCount;
    public static CustomOption MadHawkCoolTime;
    public static CustomOption MadHawkDurationTime;
    public static CustomOption MadHawkIsCheckImpostor;
    public static CustomOption MadHawkIsSettingNumberOfUniqueTasks;
    public static CustomOption MadHawkCommonTask;
    public static CustomOption MadHawkShortTask;
    public static CustomOption MadHawkLongTask;
    public static CustomOption MadHawkIsParcentageForTaskTrigger;
    public static CustomOption MadHawkParcentageForTaskTriggerSetting;
    public static CustomOption MadHawkIsUseVent;
    public static CustomOption MadHawkIsImpostorLight;

    public static CustomRoleOption BakeryOption;
    public static CustomOption BakeryPlayerCount;

    public static CustomRoleOption MadJesterOption;
    public static CustomOption MadJesterPlayerCount;
    public static CustomOption MadJesterIsUseVent;
    public static CustomOption MadJesterIsImpostorLight;
    public static CustomOption IsMadJesterTaskClearWin;
    public static CustomOption MadJesterIsSettingNumberOfUniqueTasks;
    public static CustomOption MadJesterCommonTask;
    public static CustomOption MadJesterShortTask;
    public static CustomOption MadJesterLongTask;
    public static CustomOption MadJesterIsParcentageForTaskTrigger;
    public static CustomOption MadJesterParcentageForTaskTriggerSetting;
    public static CustomOption MadJesterIsCheckImpostor;

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
    public static CustomOption FoxCanHouwaWin;

    public static CustomRoleOption DarkKillerOption;
    public static CustomOption DarkKillerPlayerCount;
    public static CustomOption DarkKillerKillCoolTime;

    public static CustomRoleOption SeerOption;
    public static CustomOption SeerPlayerCount;
    public static CustomOption SeerMode;
    public static CustomOption SeerLimitSoulDuration;
    public static CustomOption SeerSoulDuration;

    public static CustomRoleOption MadSeerOption;
    public static CustomOption MadSeerPlayerCount;
    public static CustomOption MadSeerMode;
    public static CustomOption MadSeerLimitSoulDuration;
    public static CustomOption MadSeerSoulDuration;
    public static CustomOption MadSeerIsCheckImpostor;
    public static CustomOption MadSeerIsSettingNumberOfUniqueTasks;
    public static CustomOption MadSeerCommonTask;
    public static CustomOption MadSeerShortTask;
    public static CustomOption MadSeerLongTask;
    public static CustomOption MadSeerIsParcentageForTaskTrigger;
    public static CustomOption MadSeerParcentageForTaskTriggerSetting;
    public static CustomOption MadSeerIsUseVent;
    public static CustomOption MadSeerIsImpostorLight;

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
    public static CustomOption SeerFriendsLimitSoulDuration;
    public static CustomOption SeerFriendsSoulDuration;
    public static CustomOption SeerFriendsIsCheckJackal;
    public static CustomOption SeerFriendsIsSettingNumberOfUniqueTasks;
    public static CustomOption SeerFriendsCommonTask;
    public static CustomOption SeerFriendsShortTask;
    public static CustomOption SeerFriendsLongTask;
    public static CustomOption SeerFriendsIsParcentageForTaskTrigger;
    public static CustomOption SeerFriendsParcentageForTaskTriggerSetting;
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
    public static CustomOption BlackCatIsSettingNumberOfUniqueTasks;
    public static CustomOption BlackCatCommonTask;
    public static CustomOption BlackCatShortTask;
    public static CustomOption BlackCatLongTask;
    public static CustomOption BlackCatIsParcentageForTaskTrigger;
    public static CustomOption BlackCatParcentageForTaskTriggerSetting;
    public static CustomOption BlackCatIsUseVent;
    public static CustomOption BlackCatIsImpostorLight;

    public static CustomRoleOption JackalSeerOption;
    public static CustomOption JackalSeerPlayerCount;
    public static CustomOption JackalSeerMode;
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
    public static CustomOption ChiefSheriffExecutionMode;
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
    public static CustomOption MadCleanerIsCheckImpostor;
    public static CustomOption MadCleanerIsSettingNumberOfUniqueTasks;
    public static CustomOption MadCleanerCommonTask;
    public static CustomOption MadCleanerShortTask;
    public static CustomOption MadCleanerLongTask;
    public static CustomOption MadCleanerIsParcentageForTaskTrigger;
    public static CustomOption MadCleanerParcentageForTaskTriggerSetting;
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
    public static CustomOption MayorFriendsIsSettingNumberOfUniqueTasks;
    public static CustomOption MayorFriendsCommonTask;
    public static CustomOption MayorFriendsShortTask;
    public static CustomOption MayorFriendsLongTask;
    public static CustomOption MayorFriendsIsParcentageForTaskTrigger;
    public static CustomOption MayorFriendsParcentageForTaskTriggerSetting;
    public static CustomOption MayorFriendsIsUseVent;
    public static CustomOption MayorFriendsIsImpostorLight;
    public static CustomOption MayorFriendsVoteCount;

    public static CustomRoleOption VentMakerOption;
    public static CustomOption VentMakerPlayerCount;

    public static CustomRoleOption GhostMechanicOption;
    public static CustomOption GhostMechanicPlayerCount;
    public static CustomOption GhostMechanicRepairLimit;
    public static CustomOption GhostMechanicCooldown;

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
    public static CustomOption SuicidalIdeationIsSettingNumberOfUniqueTasks;
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

    public static CustomRoleOption ConnectKillerOption;
    public static CustomOption ConnectKillerPlayerCount;

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

    public static CustomRoleOption HamburgerShopOption;
    public static CustomOption HamburgerShopPlayerCount;
    public static CustomOption HamburgerShopChangeTaskPrefab;
    public static CustomOption HamburgerShopIsSettingNumberOfUniqueTasks;
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
    public static List<float> ImpostorPlayers = new() { 1f, 1f, 15f, 1f };
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
        hideSettings = Create(100100, true, CustomOptionType.Generic, Cs(Color.white, "SettingsHideSetting"), false, specialOptions);

        /* |: ========================= Mod Normal Settings ========================== :| */

        impostorRolesCountMax = Create(100200, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoRole"), 0f, 0f, 15f, 1f);
        neutralRolesCountMax = Create(100300, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralRole"), 0f, 0f, 15f, 1f);
        crewmateRolesCountMax = Create(100400, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewRole"), 0f, 0f, 15f, 1f);
        impostorGhostRolesCountMax = Create(100500, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxImpoGhostRole"), 0f, 0f, 15f, 1f);
        neutralGhostRolesCountMax = Create(100600, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxNeutralGhostRole"), 0f, 0f, 15f, 1f);
        crewmateGhostRolesCountMax = Create(100700, true, CustomOptionType.Generic, Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "SettingMaxCrewGhostRole"), 0f, 0f, 15f, 1f);

        if (ConfigRoles.DebugMode.Value)
        {
            Color debugColor = (Color)RoleClass.Debugger.color;
            IsDebugMode = Create(100800, true, CustomOptionType.Generic, Cs(debugColor, "デバッグモード"), false, null, isHeader: true);
            DebugModeFastStart = Create(100801, true, CustomOptionType.Generic, Cs(debugColor, "即開始"), false, IsDebugMode);
            CanUseChatWhenTaskPhase = Create(100802, true, CustomOptionType.Generic, Cs(debugColor, "タスクフェイズ中にチャットを使える"), false, IsDebugMode);
            IsMurderPlayerAnnounce = Create(100803, true, CustomOptionType.Generic, Cs(debugColor, "MurderPlayer発生時に通知を行う"), false, IsDebugMode);
        }

        Color roomSetting = new(238f / 187f, 204f / 255f, 203f / 255f, 1f);
        DisconnectNotPCOption = Create(100900, true, CustomOptionType.Generic, Cs(roomSetting, "DisconnectNotPC"), true, null, isHeader: true);
        DisconnectDontHaveFriendCodeOption = Create(100901, true, CustomOptionType.Generic, Cs(roomSetting, "DisconnectDontHaveFriendCode"), true, null);

        SNRWebSendConditionHostDependency = Create(104901, true, CustomOptionType.Generic, Cs(roomSetting, "SNRWebTransmissionConditionHostDependency"), true, null, isHeader: true);

        ProhibitModColor = Create(104600, false, CustomOptionType.Generic, Cs(roomSetting, "ProhibitModColor"), false, null, isHeader: true);

        enableAgartha = Create(101000, false, CustomOptionType.Generic, "AgarthaName", true, null, isHeader: true);

        GMOption = Create(101100, false, CustomOptionType.Generic, Cs(RoleClass.GM.color, "GMName"), false, isHeader: true);
        if (ConfigRoles.DebugMode.Value) { DebuggerOption = Create(101101, false, CustomOptionType.Generic, Cs(RoleClass.Debugger.color, "DebuggerName"), false); }

        /* |: ========================= Mod Normal Settings ========================== :| */

        Mode.ModeHandler.OptionLoad(); // モード設定

        MapOption.MapOption.LoadOption(); // マップの設定

        MapCustoms.MapCustom.CreateOption(); // マップ改造

        Sabotage.Options.Load(); // 独自サボタージュの設定

        Mode.PlusMode.PlusGameOptions.Load(); // プラスゲームオプション

        IsOldMode = Create(104400, false, CustomOptionType.Generic, "IsOldMode", false, null, isHeader: true, isHidden: true);
        IsOldMode.selection = 0;

        Patches.CursedTasks.Main.SetupCustomOptions();

        /* |: ========================= Roles Settings ========================== :| */

        /* |: ========================= Impostor Settings ========================== :| */

        var SortedOptionInfos = OptionInfo.OptionInfos.OrderBy(x => (int)x.Key);
        foreach (var optionInfo in SortedOptionInfos)
            optionInfo.Value.CreateOption();

        ShiftActor.SetupCustomOptions();

        AssassinAndMarlinOption = new(200500, true, CustomOptionType.Impostor, "AssassinAndMarlinName", Color.white, 1, role: RoleId.Assassin);
        AssassinPlayerCount = Create(200501, true, CustomOptionType.Impostor, "AssassinSettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], AssassinAndMarlinOption);
        AssassinViewVote = Create(200502, true, CustomOptionType.Impostor, "GodViewVoteSetting", false, AssassinAndMarlinOption);
        MarlinPlayerCount = Create(200503, true, CustomOptionType.Impostor, "MarlinSettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AssassinAndMarlinOption);
        MarlinViewVote = Create(200504, true, CustomOptionType.Impostor, "GodViewVoteSetting", false, AssassinAndMarlinOption);

        PenguinOption = SetupCustomRoleOption(200600, true, RoleId.Penguin);
        PenguinPlayerCount = Create(200601, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PenguinOption);
        PenguinCoolTime = Create(200602, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, PenguinOption, format: "unitSeconds");
        PenguinDurationTime = Create(200603, true, CustomOptionType.Impostor, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, PenguinOption, format: "unitSeconds");
        PenguinCanDefaultKill = Create(200604, false, CustomOptionType.Impostor, "PenguinCanDefaultKill", false, PenguinOption);
        PenguinMeetingKill = Create(200605, true, CustomOptionType.Impostor, "PenguinMeetingKill", true, PenguinOption);

        Rocket.CustomOptionData.SetupCustomOptions();

        DoppelgangerOption = SetupCustomRoleOption(200700, true, RoleId.Doppelganger);
        DoppelgangerPlayerCount = Create(200701, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoppelgangerOption);
        DoppelgangerDurationTime = Create(200702, true, CustomOptionType.Impostor, "DoppelgangerDurationTimeSetting", 90f, 0f, 250f, 5f, DoppelgangerOption);
        DoppelgangerCoolTime = Create(200703, true, CustomOptionType.Impostor, "DoppelgangerCooldownSetting", 5f, 5f, 60f, 2.5f, DoppelgangerOption);
        DoppelgangerSucTime = Create(200704, true, CustomOptionType.Impostor, "DoppelgangerSucTimeSetting", 2.5f, 0f, 120f, 2.5f, DoppelgangerOption);
        DoppelgangerNotSucTime = Create(200705, true, CustomOptionType.Impostor, "DoppelgangerNotSucTimeSetting", 40f, 0f, 120f, 2.5f, DoppelgangerOption);

        MatryoshkaOption = SetupCustomRoleOption(200800, false, RoleId.Matryoshka);
        MatryoshkaPlayerCount = Create(200801, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MatryoshkaOption);
        MatryoshkaWearLimit = Create(200802, false, CustomOptionType.Impostor, "MatryoshkaWearLimit", 3f, 1f, 15f, 1f, MatryoshkaOption);
        MatryoshkaWearReport = Create(200803, false, CustomOptionType.Impostor, "MatryoshkaWearReport", true, MatryoshkaOption);
        MatryoshkaWearTime = Create(200804, false, CustomOptionType.Impostor, "MatryoshkaWearTime", 7.5f, 0.5f, 60f, 0.5f, MatryoshkaOption);
        MatryoshkaAddKillCoolTime = Create(200805, false, CustomOptionType.Impostor, "MatryoshkaAddKillCoolTime", 2.5f, 0f, 30f, 0.5f, MatryoshkaOption);
        MatryoshkaCoolTime = Create(200806, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 0f, 180f, 2.5f, MatryoshkaOption);

        VampireOption = SetupCustomRoleOption(200900, false, RoleId.Vampire);
        VampirePlayerCount = Create(200901, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VampireOption);
        VampireKillDelay = Create(200902, false, CustomOptionType.Impostor, "VampireKillDelay", 10f, 1f, 60f, 0.5f, VampireOption, format: "unitSeconds");
        VampireViewBloodStainsTurn = Create(200903, false, CustomOptionType.Impostor, "VampireViewBloodStainsTurn", 1f, 1f, 15f, 1f, VampireOption, format: "unitSeconds");
        VampireCanCreateDependents = Create(200904, false, CustomOptionType.Impostor, "VampireCanCreateDependents", true, VampireOption);
        VampireCreateDependentsCoolTime = Create(200905, false, CustomOptionType.Impostor, "VampireCreateDependentsCoolTime", 30f, 2.5f, 120f, 2.5f, VampireCanCreateDependents);
        VampireDependentsKillCoolTime = Create(200906, false, CustomOptionType.Impostor, "VampireDependentsKillCoolTime", 30f, 2.5f, 120f, 2.5f, VampireCanCreateDependents);
        VampireDependentsCanVent = Create(200907, false, CustomOptionType.Impostor, "VampireDependentsCanVent", true, VampireCanCreateDependents);

        Spider.CustomOptionData.SetupCustomOptions();

        Bat.CustomOptionData.SetupCustomOptions();

        KunoichiOption = SetupCustomRoleOption(201000, false, RoleId.Kunoichi);
        KunoichiPlayerCount = Create(201001, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], KunoichiOption);
        KunoichiCoolTime = Create(201002, false, CustomOptionType.Impostor, "KunoichiCoolTime", 2.5f, 0f, 15f, 0.5f, KunoichiOption);
        KunoichiKillKunai = Create(201003, false, CustomOptionType.Impostor, "KunoichiKillKunai", 10f, 1f, 20f, 1f, KunoichiOption);
        KunoichiIsHide = Create(201004, false, CustomOptionType.Impostor, "KunoichiIsHide", true, KunoichiOption);
        KunoichiIsWaitAndPressTheButtonToHide = Create(201005, false, CustomOptionType.Impostor, "KunoichiIsWaitAndPressTheButtonToHide", true, KunoichiIsHide);
        KunoichiHideTime = Create(201006, false, CustomOptionType.Impostor, "KunoichiHideTime", 3f, 0.5f, 10f, 0.5f, KunoichiIsHide);
        KunoichiHideKunai = Create(201007, false, CustomOptionType.Impostor, "KunoichiHideKunai", false, KunoichiIsHide);

        SerialKillerOption = SetupCustomRoleOption(201100, true, RoleId.SerialKiller);
        SerialKillerPlayerCount = Create(201101, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SerialKillerOption);
        SerialKillerSuicideTime = Create(201102, true, CustomOptionType.Impostor, "SerialKillerSuicideTimeSetting", 60f, 0f, 180f, 2.5f, SerialKillerOption);
        SerialKillerKillTime = Create(201103, true, CustomOptionType.Impostor, "SerialKillerKillTimeSetting", 15f, 0f, 60f, 2.5f, SerialKillerOption);
        SerialKillerIsMeetingReset = Create(201104, true, CustomOptionType.Impostor, "SerialKillerIsMeetingResetSetting", true, SerialKillerOption);

        FastMakerOption = SetupCustomRoleOption(201200, true, RoleId.FastMaker);
        FastMakerPlayerCount = Create(201201, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FastMakerOption);

        SuicideWisherOption = SetupCustomRoleOption(201300, true, RoleId.SuicideWisher);
        SuicideWisherPlayerCount = Create(201301, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SuicideWisherOption);

        SelfBomberOption = SetupCustomRoleOption(201400, true, RoleId.SelfBomber);
        SelfBomberPlayerCount = Create(201401, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SelfBomberOption);
        SelfBomberScope = Create(201402, true, CustomOptionType.Impostor, "SelfBomberScopeSetting", 1f, 0.5f, 3f, 0.5f, SelfBomberOption);
        SelfBomberBombCoolTime = Create(201403, true, CustomOptionType.Impostor, "SelfBomberBombCoolTimeSettting", 10f, 2.5f, 90f, 2.5f, SelfBomberOption);

        EvilNekomataOption = SetupCustomRoleOption(201500, true, RoleId.EvilNekomata);
        EvilNekomataPlayerCount = Create(201501, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilNekomataOption);
        EvilNekomataNotImpostorExiled = Create(201502, true, CustomOptionType.Impostor, "NotImpostorExiled", false, EvilNekomataOption);

        FinderOption = SetupCustomRoleOption(201700, true, RoleId.Finder);
        FinderPlayerCount = Create(201701, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FinderOption);
        FinderCheckMadmateSetting = Create(201702, true, CustomOptionType.Impostor, "FinderCheckMadmateSetting", 3f, 1f, 15f, 1f, FinderOption);

        NunOption = SetupCustomRoleOption(201601, false, RoleId.Nun);
        NunPlayerCount = Create(201602, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], NunOption);
        NunCoolTime = Create(201603, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, NunOption);

        EvilButtonerOption = SetupCustomRoleOption(202000, true, RoleId.EvilButtoner);
        EvilButtonerPlayerCount = Create(202001, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilButtonerOption);
        EvilButtonerCoolTime = Create(202002, false, CustomOptionType.Impostor, "ButtonerCooldownSetting", 20f, 2.5f, 60f, 2.5f, EvilButtonerOption, format: "unitSeconds");//クールタイムはSHR未対応の為false
        EvilButtonerCount = Create(202003, true, CustomOptionType.Impostor, "ButtonerCountSetting", 1f, 1f, 10f, 1f, EvilButtonerOption);

        TaskerOption = SetupCustomRoleOption(202100, false, RoleId.Tasker);
        TaskerPlayerCount = Create(202101, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoppelgangerOption);
        TaskerIsSettingNumberOfUniqueTasks = Create(202102, false, CustomOptionType.Impostor, "IsSettingNumberOfUniqueTasks", true, TaskerOption);
        var taskeroption = SelectTask.TaskSetting(202103, 202104, 202105, TaskerIsSettingNumberOfUniqueTasks, CustomOptionType.Impostor, false);
        TaskerCommonTask = taskeroption.Item1;
        TaskerShortTask = taskeroption.Item2;
        TaskerLongTask = taskeroption.Item3;
        TaskerIsKillCoolTaskNow = Create(202106, false, CustomOptionType.Impostor, "TaskerIsKillCoolTaskNow", true, TaskerOption);
        TaskerCanKill = Create(202107, false, CustomOptionType.Impostor, "TaskerCanKill", true, TaskerOption);

        EvilMechanic.SetupCustomOptions();

        CamouflagerOption = SetupCustomRoleOption(202300, false, RoleId.Camouflager);
        CamouflagerPlayerCount = Create(202301, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CamouflagerOption);
        CamouflagerCoolTime = Create(202302, false, CustomOptionType.Impostor, "CamouflagerCoolTimeSetting", 30f, 0f, 60f, 2.5f, CamouflagerOption);
        CamouflagerDurationTime = Create(202303, false, CustomOptionType.Impostor, "CamouflagerDurationTimeSetting", 10f, 0f, 60f, 2.5f, CamouflagerOption);
        CamouflagerCamouflageArsonist = Create(202304, false, CustomOptionType.Impostor, "CamouflagerCamouflageArsonistSetting", true, CamouflagerOption);
        CamouflagerCamouflageDemon = Create(202305, false, CustomOptionType.Impostor, "CamouflagerCamouflageDemonSetting", true, CamouflagerOption);
        CamouflagerCamouflageLovers = Create(202306, false, CustomOptionType.Impostor, "CamouflagerCamouflageLoversSetting", false, CamouflagerOption);
        CamouflagerCamouflageQuarreled = Create(202307, false, CustomOptionType.Impostor, "CamouflagerCamouflageQuarreledSetting", false, CamouflagerOption);
        CamouflagerCamouflageChangeColor = Create(202308, false, CustomOptionType.Impostor, "CamouflagerCamouflageChangeColorSetting", false, CamouflagerOption);
        CamouflagerCamouflageColor = Create(202309, false, CustomOptionType.Impostor, "CamouflagerCamouflageColorSetting", Camouflager.ColorOption, CamouflagerCamouflageChangeColor);

        SamuraiOption = SetupCustomRoleOption(202400, true, RoleId.Samurai);
        SamuraiPlayerCount = Create(202401, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SamuraiOption);
        SamuraiKillCoolTime = Create(202402, true, CustomOptionType.Impostor, "SamuraiKillCoolSetting", 30f, 2.5f, 60f, 2.5f, SamuraiOption);
        SamuraiSwordCoolTime = Create(202405, true, CustomOptionType.Impostor, "SamuraiSwordCoolSetting", 50f, 30f, 70f, 2.5f, SamuraiOption);
        SamuraiVent = Create(202406, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, SamuraiOption);
        SamuraiSabo = Create(202407, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, SamuraiOption);
        SamuraiScope = Create(202408, true, CustomOptionType.Impostor, "SamuraiScopeSetting", 1f, 0.5f, 3f, 0.5f, SamuraiOption);

        PursuerOption = SetupCustomRoleOption(202500, false, RoleId.Pursuer);
        PursuerPlayerCount = Create(202501, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PursuerOption);

        NekoKabocha.SetupCustomOptions();

        CrackerOption = SetupCustomRoleOption(202600, false, RoleId.Cracker);
        CrackerPlayerCount = Create(202601, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CrackerOption);
        CrackerCoolTime = Create(202602, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, CrackerOption);
        CrackerIsAdminView = Create(202603, false, CustomOptionType.Impostor, "CrackerIsAdminView", false, CrackerOption);
        CrackerIsVitalsView = Create(202604, false, CustomOptionType.Impostor, "CrackerIsVitalsView", false, CrackerOption);
        CrackerOneTurnSelectCount = Create(202605, false, CustomOptionType.Impostor, "CrackerOneTurnSelectCount", 1f, 1f, 15f, 1f, CrackerOption);
        CrackerAllTurnSelectCount = Create(202606, false, CustomOptionType.Impostor, "CrackerAllTurnSelectCount", 3f, 1f, 100f, 1f, CrackerOption);
        CrackerIsSelfNone = Create(202607, false, CustomOptionType.Impostor, "CrackerIsSelfNone", true, CrackerOption);

        ConnectKillerOption = SetupCustomRoleOption(202700, false, RoleId.ConnectKiller);
        ConnectKillerPlayerCount = Create(202701, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], ConnectKillerOption);

        EvilSpeedBoosterOption = SetupCustomRoleOption(202800, false, RoleId.EvilSpeedBooster);
        EvilSpeedBoosterPlayerCount = Create(202801, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EvilSpeedBoosterOption);
        EvilSpeedBoosterCoolTime = Create(202802, false, CustomOptionType.Impostor, "EvilSpeedBoosterCooldownSetting", 30f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
        EvilSpeedBoosterDurationTime = Create(202803, false, CustomOptionType.Impostor, "EvilSpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, EvilSpeedBoosterOption, format: "unitSeconds");
        EvilSpeedBoosterSpeed = Create(202804, false, CustomOptionType.Impostor, "EvilSpeedBoosterPlusSpeedSetting", 1.25f, 0.0f, 5f, 0.25f, EvilSpeedBoosterOption, format: "unitSeconds");
        EvilSpeedBoosterIsNotSpeedBooster = Create(202805, false, CustomOptionType.Impostor, "EvilSpeedBoosterIsNotSpeedBooster", false, EvilSpeedBoosterOption, isHidden: true);
        EvilSpeedBoosterIsNotSpeedBooster.selection = 0;

        EvilDoorrOption = SetupCustomRoleOption(202900, false, RoleId.EvilDoorr);
        EvilDoorrPlayerCount = Create(202901, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilDoorrOption);
        EvilDoorrCoolTime = Create(202902, false, CustomOptionType.Impostor, "EvilDoorrCoolTimeSetting", 2.5f, 2.5f, 60f, 2.5f, EvilDoorrOption);

        TeleporterOption = SetupCustomRoleOption(203000, false, RoleId.Teleporter);
        TeleporterPlayerCount = Create(203001, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], TeleporterOption);
        TeleporterCoolTime = Create(203002, false, CustomOptionType.Impostor, "TeleporterCooldownSetting", 30f, 2.5f, 60f, 2.5f, TeleporterOption, format: "unitSeconds");
        TeleporterDurationTime = Create(203003, false, CustomOptionType.Impostor, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleporterOption, format: "unitSeconds");

        FreezerOption = SetupCustomRoleOption(203100, false, RoleId.Freezer);
        FreezerPlayerCount = Create(203101, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], FreezerOption);
        FreezerCoolTime = Create(203102, false, CustomOptionType.Impostor, "FreezerCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, FreezerOption, format: "unitSeconds");
        FreezerDurationTime = Create(203103, false, CustomOptionType.Impostor, "FreezerDurationSetting", 1f, 1f, 7f, 1f, FreezerOption, format: "unitSeconds");

        SpeederOption = SetupCustomRoleOption(203200, false, RoleId.Speeder);
        SpeederPlayerCount = Create(203201, false, CustomOptionType.Impostor, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeederOption);
        SpeederCoolTime = Create(203202, false, CustomOptionType.Impostor, "SpeederCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, SpeederOption, format: "unitSeconds");
        SpeederDurationTime = Create(203203, false, CustomOptionType.Impostor, "SpeederDurationTimeSetting", 10f, 2.5f, 20f, 2.5f, SpeederOption, format: "unitSeconds");

        EvilGamblerOption = SetupCustomRoleOption(203300, true, RoleId.EvilGambler);
        EvilGamblerPlayerCount = Create(203301, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilGamblerOption);
        EvilGamblerSucTime = Create(203302, true, CustomOptionType.Impostor, "EvilGamblerSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
        EvilGamblerNotSucTime = Create(203303, true, CustomOptionType.Impostor, "EvilGamblerNotSucTimeSetting", 15f, 0f, 60f, 2.5f, EvilGamblerOption);
        EvilGamblerSucpar = Create(203304, true, CustomOptionType.Impostor, "EvilGamblerSucParSetting", rates, EvilGamblerOption);

        CountChangerOption = SetupCustomRoleOption(203400, false, RoleId.CountChanger);
        CountChangerPlayerCount = Create(203401, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CountChangerOption);
        CountChangerMaxCount = Create(203402, false, CustomOptionType.Impostor, "CountChangerMaxCountSetting", 1f, 1f, 15f, 1f, CountChangerOption);
        CountChangerNextTurn = Create(203403, false, CustomOptionType.Impostor, "CountChangerNextTurnSetting", false, CountChangerOption);

        MinimalistOption = SetupCustomRoleOption(203500, true, RoleId.Minimalist);
        MinimalistPlayerCount = Create(203501, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MinimalistOption);
        MinimalistKillCoolTime = Create(203502, true, CustomOptionType.Impostor, "MinimalistKillCoolSetting", 20f, 2.5f, 60f, 2.5f, MinimalistOption);
        MinimalistVent = Create(203503, true, CustomOptionType.Impostor, "MinimalistVentSetting", false, MinimalistOption);
        MinimalistSabo = Create(203504, true, CustomOptionType.Impostor, "MinimalistSaboSetting", false, MinimalistOption);
        MinimalistReport = Create(203505, true, CustomOptionType.Impostor, "MinimalistReportSetting", true, MinimalistOption);

        HawkOption = SetupCustomRoleOption(203600, false, RoleId.Hawk);
        HawkPlayerCount = Create(203602, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], HawkOption);
        HawkCoolTime = Create(203603, false, CustomOptionType.Impostor, "HawkCoolTimeSetting", 15f, 0f, 120f, 2.5f, HawkOption, format: "unitCouples");
        HawkDurationTime = Create(203604, false, CustomOptionType.Impostor, "HawkDurationTimeSetting", 5f, 0f, 60f, 0.5f, HawkOption, format: "unitCouples");

        EvilEraserOption = SetupCustomRoleOption(203700, false, RoleId.EvilEraser);
        EvilEraserPlayerCount = Create(203701, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilEraserOption);
        EvilEraserMaxCount = Create(203702, false, CustomOptionType.Impostor, "EvilEraserMaxCountSetting", 1f, 1f, 15f, 1f, EvilEraserOption);

        MagazinerOption = SetupCustomRoleOption(203800, false, RoleId.Magaziner);
        MagazinerPlayerCount = Create(203801, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MagazinerOption);
        MagazinerSetKillTime = Create(203802, false, CustomOptionType.Impostor, "MagazinerSetTimeSetting", 0f, 0f, 60f, 2.5f, MagazinerOption);

        OverKillerOption = SetupCustomRoleOption(203900, true, RoleId.OverKiller);
        OverKillerPlayerCount = Create(203901, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], OverKillerOption);
        OverKillerKillCoolTime = Create(203902, true, CustomOptionType.Impostor, "OverKillerKillCoolTimeSetting", 45f, 0f, 60f, 2.5f, OverKillerOption);
        OverKillerKillCount = Create(203903, true, CustomOptionType.Impostor, "OverKillerKillCountSetting", 30f, 1f, 60f, 1f, OverKillerOption);

        LevelingerOption = SetupCustomRoleOption(204000, false, RoleId.Levelinger);
        LevelingerPlayerCount = Create(204001, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], LevelingerOption);
        LevelingerOneKillXP = Create(204002, false, CustomOptionType.Impostor, "LevelingerOneKillXPSetting", 1f, 0f, 10f, 1f, LevelingerOption);
        LevelingerUpLevelXP = Create(204003, false, CustomOptionType.Impostor, "LevelingerUpLevelXPSetting", 2f, 1f, 50f, 1f, LevelingerOption);
        LevelingerLevelOneGetPower = Create(204004, false, CustomOptionType.Impostor, "1" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelTwoGetPower = Create(204005, false, CustomOptionType.Impostor, "2" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelThreeGetPower = Create(204006, false, CustomOptionType.Impostor, "3" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelFourGetPower = Create(204007, false, CustomOptionType.Impostor, "4" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerLevelFiveGetPower = Create(204008, false, CustomOptionType.Impostor, "5" + ModTranslation.GetString("LevelingerGetPowerSetting"), LevelingerTexts, LevelingerOption);
        LevelingerReviveXP = Create(204009, false, CustomOptionType.Impostor, "LevelingerReviveXPSetting", false, LevelingerOption);
        LevelingerUseXPRevive = Create(204010, false, CustomOptionType.Impostor, "LevelingerUseXPReviveSetting", 5f, 0f, 20f, 1f, LevelingerReviveXP);

        EvilMovingOption = SetupCustomRoleOption(204100, false, RoleId.EvilMoving);
        EvilMovingPlayerCount = Create(204101, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], EvilMovingOption);
        EvilMovingCoolTime = Create(204102, false, CustomOptionType.Impostor, "MovingCooldownSetting", 30f, 0f, 60f, 2.5f, EvilMovingOption);

        SideKillerOption = SetupCustomRoleOption(204200, false, RoleId.SideKiller);
        SideKillerPlayerCount = Create(204201, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SideKillerOption);
        SideKillerKillCoolTime = Create(204202, false, CustomOptionType.Impostor, "SideKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);
        SideKillerMadKillerKillCoolTime = Create(204203, false, CustomOptionType.Impostor, "SideKillerMadKillerKillCoolTimeSetting", 45f, 0f, 75f, 2.5f, SideKillerOption);

        SurvivorOption = SetupCustomRoleOption(204300, true, RoleId.Survivor);
        SurvivorPlayerCount = Create(204301, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SurvivorOption);
        SurvivorKillCoolTime = Create(204302, true, CustomOptionType.Impostor, "SurvivorKillCoolTimeSetting", 15f, 0f, 75f, 2.5f, SurvivorOption);

        DarkKillerOption = SetupCustomRoleOption(204400, true, RoleId.DarkKiller);
        DarkKillerPlayerCount = Create(204401, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DarkKillerOption);
        DarkKillerKillCoolTime = Create(204402, true, CustomOptionType.Impostor, "DarkKillerKillCoolSetting", 20f, 2.5f, 60f, 2.5f, DarkKillerOption);

        CleanerOption = SetupCustomRoleOption(204500, false, RoleId.Cleaner);
        CleanerPlayerCount = Create(204501, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], CleanerOption);
        CleanerKillCoolTime = Create(204502, false, CustomOptionType.Impostor, "CleanerKillCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, CleanerOption, format: "unitSeconds");
        CleanerCooldown = Create(204503, false, CustomOptionType.Impostor, "CleanerCooldownSetting", 60f, 40f, 70f, 2.5f, CleanerOption, format: "unitSeconds");

        VentMakerOption = SetupCustomRoleOption(204600, false, RoleId.VentMaker);
        VentMakerPlayerCount = Create(204601, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], VentMakerOption);

        PositionSwapperOption = SetupCustomRoleOption(204700, false, RoleId.PositionSwapper);
        PositionSwapperPlayerCount = Create(204701, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], PositionSwapperOption);
        PositionSwapperSwapCount = Create(204702, false, CustomOptionType.Impostor, "SettingPositionSwapperSwapCountName", 1f, 0f, 99f, 1f, PositionSwapperOption);
        PositionSwapperCoolTime = Create(204703, false, CustomOptionType.Impostor, "SettingPositionSwapperSwapCoolTimeName", 2.5f, 2.5f, 90f, 2.5f, PositionSwapperOption);

        MafiaOption = SetupCustomRoleOption(204800, true, RoleId.Mafia);
        MafiaPlayerCount = Create(204801, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MafiaOption);

        SecretlyKillerOption = SetupCustomRoleOption(204900, false, RoleId.SecretlyKiller);
        SecretlyKillerPlayerCount = Create(204901, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SecretlyKillerOption);
        SecretlyKillerKillCoolTime = Create(204902, false, CustomOptionType.Impostor, "SheriffCooldownSetting", 2.5f, 2.5f, 60f, 2.5f, SecretlyKillerOption);
        SecretlyKillerIsKillCoolTimeChange = Create(204903, false, CustomOptionType.Impostor, "SettingCoolCharge", true, SecretlyKillerOption);
        SecretlyKillerIsBlackOutKillCharge = Create(204904, false, CustomOptionType.Impostor, "SettingBlackoutCharge", false, SecretlyKillerOption);
        SecretlyKillerSecretKillLimit = Create(204905, false, CustomOptionType.Impostor, "SettingLimitName", 1f, 0f, 99f, 1f, SecretlyKillerOption);
        SecretlyKillerSecretKillCoolTime = Create(204906, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 45f, 2.5f, 60f, 2.5f, SecretlyKillerOption);

        DoubleKillerOption = SetupCustomRoleOption(205000, false, RoleId.DoubleKiller);
        DoubleKillerPlayerCount = Create(205001, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], DoubleKillerOption);
        MainKillCoolTime = Create(205002, false, CustomOptionType.Impostor, "MainCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoubleKillerOption, format: "unitSeconds");
        SubKillCoolTime = Create(205003, false, CustomOptionType.Impostor, "SubCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, DoubleKillerOption, format: "unitSeconds");
        DoubleKillerSabo = Create(205004, false, CustomOptionType.Impostor, "DoubleKillerSaboSetting", false, DoubleKillerOption);
        DoubleKillerVent = Create(205005, false, CustomOptionType.Impostor, "MinimalistVentSetting", false, DoubleKillerOption);

        SmasherOption = SetupCustomRoleOption(205100, false, RoleId.Smasher);
        SmasherPlayerCount = Create(205101, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], SmasherOption);
        SmasherKillCoolTime = Create(205102, false, CustomOptionType.Impostor, "KillCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, SmasherOption, format: "unitSeconds");

        WerewolfOption = new(205200, false, CustomOptionType.Impostor, "WerewolfName", RoleClass.Werewolf.color, 1);
        WerewolfPlayerCount = Create(205201, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], WerewolfOption);

        // SetupImpostorCustomOptions

        /* |: ========================= Neutral Settings ========================== :| */

        JackalOption = SetupCustomRoleOption(300000, true, RoleId.Jackal);
        JackalPlayerCount = Create(300001, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalOption);
        JackalKillCooldown = Create(300002, true, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, JackalOption, format: "unitSeconds");
        JackalUseVent = Create(300003, true, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalOption);
        JackalUseSabo = Create(300004, true, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalOption);
        JackalIsImpostorLight = Create(300005, true, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, JackalOption);
        JackalCreateFriend = Create(300006, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, JackalOption);
        JackalCreateSidekick = Create(300007, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalOption);
        JackalSKCooldown = Create(300008, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, JackalCreateSidekick, format: "unitSeconds");
        JackalNewJackalCreateSidekick = Create(300009, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalCreateSidekick);

        WaveCannonJackal.SetupCustomOptions();

        JackalSeerOption = SetupCustomRoleOption(300200, true, RoleId.JackalSeer);
        JackalSeerPlayerCount = Create(300201, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalSeerOption);
        JackalSeerMode = Create(300202, false, CustomOptionType.Neutral, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, JackalSeerOption);
        JackalSeerLimitSoulDuration = Create(300203, false, CustomOptionType.Neutral, "SeerLimitSoulDuration", false, JackalSeerOption);
        JackalSeerSoulDuration = Create(300204, false, CustomOptionType.Neutral, "SeerSoulDuration", 15f, 0f, 120f, 5f, JackalSeerLimitSoulDuration, format: "unitCouples");
        JackalSeerKillCooldown = Create(300205, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, JackalSeerOption, format: "unitSeconds");
        JackalSeerUseVent = Create(300206, true, CustomOptionType.Neutral, "JackalUseVentSetting", true, JackalSeerOption);
        JackalSeerUseSabo = Create(300207, true, CustomOptionType.Neutral, "JackalUseSaboSetting", false, JackalSeerOption);
        JackalSeerIsImpostorLight = Create(300208, true, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, JackalSeerOption);
        JackalSeerCreateSidekick = Create(300209, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalSeerOption);
        JackalSeerCreateFriend = Create(300210, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, JackalSeerOption);
        JackalSeerSKCooldown = Create(300211, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, JackalSeerCreateSidekick, format: "unitSeconds");
        JackalSeerNewJackalCreateSidekick = Create(300212, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalSeerCreateSidekick);

        TeleportingJackalOption = SetupCustomRoleOption(300300, false, RoleId.TeleportingJackal);
        TeleportingJackalPlayerCount = Create(300301, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TeleportingJackalOption);
        TeleportingJackalKillCooldown = Create(300302, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, TeleportingJackalOption, format: "unitSeconds");
        TeleportingJackalUseVent = Create(300303, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, TeleportingJackalOption);
        TeleportingJackalIsImpostorLight = Create(300304, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, TeleportingJackalOption);
        TeleportingJackalUseSabo = Create(300305, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, TeleportingJackalOption);
        TeleportingJackalCoolTime = Create(300306, false, CustomOptionType.Neutral, "TeleporterCooldownSetting", 30f, 2.5f, 60f, 2.5f, TeleportingJackalOption, format: "unitSeconds");
        TeleportingJackalDurationTime = Create(300307, false, CustomOptionType.Neutral, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, TeleportingJackalOption, format: "unitSeconds");

        PavlovsownerOption = new(300400, false, CustomOptionType.Neutral, "PavlovsdogsName", RoleClass.Pavlovsdogs.color, 1, role: RoleId.Pavlovsowner);
        PavlovsownerPlayerCount = Create(300401, false, CustomOptionType.Neutral, "SettingPlayerCountName", AlonePlayers[0], AlonePlayers[1], AlonePlayers[2], AlonePlayers[3], PavlovsownerOption);
        PavlovsownerCreateCoolTime = Create(300402, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, PavlovsownerOption);
        PavlovsownerCreateDogLimit = Create(300403, false, CustomOptionType.Neutral, "PavlovsownerCreateDogLimit", 1f, 1f, 15f, 1f, PavlovsownerOption);
        PavlovsownerIsTargetImpostorDeath = Create(300404, false, CustomOptionType.Neutral, "PavlovsownerIsTargetImpostorDeath", true, PavlovsownerOption);
        PavlovsdogIsImpostorView = Create(300405, false, CustomOptionType.Neutral, "PavlovsdogIsImpostorView", true, PavlovsownerOption);
        PavlovsdogKillCoolTime = Create(300406, false, CustomOptionType.Neutral, "SheriffCooldownSetting", 30f, 2.5f, 120f, 2.5f, PavlovsownerOption);
        PavlovsdogCanVent = Create(300407, false, CustomOptionType.Neutral, "MadmateUseVentSetting", true, PavlovsownerOption);
        PavlovsdogRunAwayKillCoolTime = Create(300408, false, CustomOptionType.Neutral, "PavlovsdogRunAwayKillCoolTime", 20f, 2.5f, 60f, 2.5f, PavlovsownerOption);
        PavlovsdogRunAwayDeathTime = Create(300409, false, CustomOptionType.Neutral, "PavlovsdogRunAwayDeathTime", 60f, 2.5f, 180f, 2.5f, PavlovsownerOption);
        PavlovsdogRunAwayDeathTimeIsMeetingReset = Create(300410, false, CustomOptionType.Neutral, "PavlovsdogRunAwayDeathTimeIsMeetingReset", true, PavlovsownerOption);

        HitmanOption = SetupCustomRoleOption(303200, false, RoleId.Hitman);
        HitmanPlayerCount = Create(303201, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HitmanOption);
        HitmanKillCoolTime = Create(303202, false, CustomOptionType.Neutral, "SheriffCooldownSetting", 20f, 0f, 120f, 2.5f, HitmanOption);
        HitmanChangeTargetTime = Create(303203, false, CustomOptionType.Neutral, "HitmanChangeTargetTime", 60f, 0f, 240f, 2.5f, HitmanOption);
        HitmanIsOutMission = Create(303204, false, CustomOptionType.Neutral, "HitmanIsOutMission", true, HitmanOption);
        HitmanOutMissionLimit = Create(303205, false, CustomOptionType.Neutral, "HitmanOutMissionLimit", 5f, 1f, 30f, 1f, HitmanIsOutMission);
        HitmanWinKillCount = Create(303206, false, CustomOptionType.Neutral, "HitmanWinKillCount", 5f, 1f, 15f, 1f, HitmanOption);
        HitmanIsArrowView = Create(303207, false, CustomOptionType.Neutral, "HitmanIsTargetArrow", true, HitmanOption);
        HitmanArrowUpdateTime = Create(303208, false, CustomOptionType.Neutral, "HitmanUpdateTargetArrowTime", 0f, 0f, 120f, 2.5f, HitmanIsArrowView);

        ArsonistOption = SetupCustomRoleOption(302400, true, RoleId.Arsonist);
        ArsonistPlayerCount = Create(302401, true, CustomOptionType.Neutral, "SettingPlayerCountName", AlonePlayers[0], AlonePlayers[1], AlonePlayers[2], AlonePlayers[3], ArsonistOption);
        ArsonistCoolTime = Create(302402, true, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, ArsonistOption, format: "unitSeconds");
        ArsonistDurationTime = Create(302403, true, CustomOptionType.Neutral, "ArsonistDurationTimeSetting", 3f, 0.5f, 10f, 0.5f, ArsonistOption, format: "unitSeconds");
        ArsonistIsUseVent = Create(302404, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, ArsonistOption);

        VultureOption = SetupCustomRoleOption(302000, false, RoleId.Vulture);
        VulturePlayerCount = Create(302001, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], VultureOption);
        VultureCooldown = Create(302002, false, CustomOptionType.Neutral, "VultureCooldownSetting", 30f, 2.5f, 60f, 2.5f, VultureOption, format: "unitSeconds");
        VultureDeadBodyMaxCount = Create(302003, false, CustomOptionType.Neutral, "VultureDeadBodyCountSetting", 3f, 1f, 6f, 1f, VultureOption);
        VultureIsUseVent = Create(302004, false, CustomOptionType.Neutral, "MadmateUseVentSetting", true, VultureOption);
        VultureShowArrows = Create(302005, false, CustomOptionType.Neutral, "VultureShowArrowsSetting", true, VultureOption);

        OpportunistOption = SetupCustomRoleOption(302100, true, RoleId.Opportunist);
        OpportunistPlayerCount = Create(302101, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], OpportunistOption);

        JesterOption = SetupCustomRoleOption(300500, true, RoleId.Jester);
        JesterPlayerCount = Create(300501, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JesterOption);
        JesterIsVent = Create(300502, true, CustomOptionType.Neutral, "JesterIsVentSetting", false, JesterOption);
        JesterIsSabotage = Create(300503, false, CustomOptionType.Neutral, "JesterIsSabotageSetting", false, JesterOption);
        JesterIsWinCleartask = Create(300504, true, CustomOptionType.Neutral, "JesterIsWinClearTaskSetting", false, JesterOption);
        JesterIsSettingNumberOfUniqueTasks = Create(300505, true, CustomOptionType.Neutral, "IsSettingNumberOfUniqueTasks", true, JesterIsWinCleartask);
        var jesteroption = SelectTask.TaskSetting(300506, 300507, 300508, JesterIsSettingNumberOfUniqueTasks, CustomOptionType.Neutral, true);
        JesterCommonTask = jesteroption.Item1;
        JesterShortTask = jesteroption.Item2;
        JesterLongTask = jesteroption.Item3;

        Crook.CustomOptionData.SetupCustomOptions();

        Sauner.CustomOptionData.SetupCustomOptions();

        Pokerface.CustomOptionData.SetupCustomOptions();

        trueloverOption = SetupCustomRoleOption(300600, true, RoleId.truelover);
        trueloverPlayerCount = Create(300601, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], trueloverOption);

        GodOption = SetupCustomRoleOption(300700, true, RoleId.God);
        GodPlayerCount = Create(300701, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GodOption);
        GodViewVote = Create(300702, true, CustomOptionType.Neutral, "GodViewVoteSetting", false, GodOption);
        GodIsEndTaskWin = Create(300703, true, CustomOptionType.Neutral, "GodIsEndTaskWinSetting", true, GodOption);
        GodIsSettingNumberOfUniqueTasks = Create(300704, true, CustomOptionType.Neutral, "IsSettingNumberOfUniqueTasks", true, GodIsEndTaskWin);
        var godoption = SelectTask.TaskSetting(300705, 300706, 300707, GodIsSettingNumberOfUniqueTasks, CustomOptionType.Neutral, true);
        GodCommonTask = godoption.Item1;
        GodShortTask = godoption.Item2;
        GodLongTask = godoption.Item3;

        WorkpersonOption = SetupCustomRoleOption(300800, true, RoleId.Workperson);
        WorkpersonPlayerCount = Create(300801, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WorkpersonOption);
        WorkpersonIsAliveWin = Create(300802, true, CustomOptionType.Neutral, "WorkpersonIsAliveWinSetting", false, WorkpersonOption);
        WorkpersonIsSettingNumberOfUniqueTasks = Create(300803, false, CustomOptionType.Neutral, "IsSettingNumberOfUniqueTasks", true, WorkpersonOption);
        WorkpersonCommonTask = Create(300804, true, CustomOptionType.Neutral, "GameCommonTasks", 2, 0, 12, 1, WorkpersonOption);
        WorkpersonLongTask = Create(300805, true, CustomOptionType.Neutral, "GameLongTasks", 10, 0, 69, 1, WorkpersonOption);
        WorkpersonShortTask = Create(300806, true, CustomOptionType.Neutral, "GameShortTasks", 5, 0, 45, 1, WorkpersonOption);

        TunaOption = SetupCustomRoleOption(300900, true, RoleId.Tuna);
        TunaPlayerCount = Create(300901, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TunaOption);
        TunaStoppingTime = Create(300902, true, CustomOptionType.Neutral, "TunaStoppingTimeSetting", 1f, 1f, 3f, 1f, TunaOption);
        TunaIsUseVent = Create(300903, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, TunaOption);
        TunaIsAddWin = Create(300904, true, CustomOptionType.Neutral, "TunaAddWinSetting", false, TunaOption);

        RevolutionistAndDictatorOption = new(301000, false, CustomOptionType.Neutral, "RevolutionistAndDictatorName", Color.white, 1, role: RoleId.Revolutionist);
        RevolutionistPlayerCount = Create(301001, false, CustomOptionType.Neutral, "SettingRevolutionistPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RevolutionistAndDictatorOption);
        DictatorPlayerCount = Create(301002, false, CustomOptionType.Neutral, "SettingDictatorPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RevolutionistAndDictatorOption);
        DictatorVoteCount = Create(301003, false, CustomOptionType.Neutral, "DictatorVoteCount", 2f, 1f, 100f, 1f, RevolutionistAndDictatorOption);
        DictatorSubstituteExile = Create(301004, false, CustomOptionType.Neutral, "DictatorSubExile", false, RevolutionistAndDictatorOption);
        DictatorSubstituteExileLimit = Create(301005, false, CustomOptionType.Neutral, "DictatorSubExileLimit", 1f, 1f, 15f, 1f, DictatorSubstituteExile);
        RevolutionistCoolTime = Create(301006, false, CustomOptionType.Neutral, "RevolutionCoolTime", 10f, 2.5f, 60f, 2.5f, RevolutionistAndDictatorOption);
        RevolutionistTouchTime = Create(301007, false, CustomOptionType.Neutral, "RevolutionTouchTime", 1f, 0f, 10f, 0.5f, RevolutionistAndDictatorOption);
        RevolutionistAddWin = Create(301008, false, CustomOptionType.Neutral, "RevolutionistAddWin", false, RevolutionistAndDictatorOption);
        RevolutionistAddWinIsAlive = Create(301009, false, CustomOptionType.Neutral, "RevolutionistAddWinIsAlive", true, RevolutionistAddWin);

        LoversBreakerOption = SetupCustomRoleOption(301200, false, RoleId.LoversBreaker);
        LoversBreakerPlayerCount = Create(301201, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LoversBreakerOption);
        LoversBreakerBreakCount = Create(301202, false, CustomOptionType.Neutral, "LoversBreakerBreakCount", 1f, 1f, 7f, 1f, LoversBreakerOption);
        LoversBreakerCoolTime = Create(301203, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, LoversBreakerOption, format: "unitSeconds");
        LoversBreakerIsDeathWin = Create(301204, false, CustomOptionType.Neutral, "LoversBreakerIsDeathWin", true, LoversBreakerOption);

        OrientalShaman.SetupCustomOptions();

        FoxOption = SetupCustomRoleOption(301400, true, RoleId.Fox);
        FoxPlayerCount = Create(301401, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FoxOption);
        FoxIsUseVent = Create(301402, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, FoxOption);
        FoxIsImpostorLight = Create(301403, true, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, FoxOption);
        FoxReport = Create(301404, true, CustomOptionType.Neutral, "MinimalistReportSetting", true, FoxOption);
        FoxCanHouwaWin = Create(301405, true, CustomOptionType.Neutral, "CanHouwaWin", false, FoxOption);

        FireFox.SetupCustomOptions();

        AmnesiacOption = SetupCustomRoleOption(301600, false, RoleId.Amnesiac);
        AmnesiacPlayerCount = Create(301601, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], AmnesiacOption);
        AmnesiacShowArrows = Create(301602, false, CustomOptionType.Neutral, "VultureShowArrowsSetting", true, AmnesiacOption);

        TheThreeLittlePigs.SetupCustomOptions();

        Safecracker.SetupCustomOptions();

        FalseChargesOption = SetupCustomRoleOption(301900, true, RoleId.FalseCharges);
        FalseChargesPlayerCount = Create(301901, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], FalseChargesOption);
        FalseChargesExileTurn = Create(301902, true, CustomOptionType.Neutral, "FalseChargesExileTurn", 2f, 1f, 10f, 1f, FalseChargesOption);
        FalseChargesCoolTime = Create(301903, true, CustomOptionType.Neutral, "FalseChargesCoolTime", 15f, 0f, 75f, 2.5f, FalseChargesOption);

        EgoistOption = SetupCustomRoleOption(302200, true, RoleId.Egoist);
        EgoistPlayerCount = Create(302201, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], EgoistOption);
        EgoistUseVent = Create(302202, true, CustomOptionType.Neutral, "EgoistUseVentSetting", false, EgoistOption);
        EgoistUseSabo = Create(302203, true, CustomOptionType.Neutral, "EgoistUseSaboSetting", false, EgoistOption);
        EgoistImpostorLight = Create(302204, true, CustomOptionType.Neutral, "EgoistImpostorLightSetting", false, EgoistOption);
        EgoistUseKill = Create(302205, true, CustomOptionType.Neutral, "EgoistUseKillSetting", false, EgoistOption);

        DemonOption = SetupCustomRoleOption(302301, true, RoleId.Demon);
        DemonPlayerCount = Create(302302, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DemonOption);
        DemonCoolTime = Create(302303, true, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, DemonOption, format: "unitSeconds");
        DemonIsUseVent = Create(302304, true, CustomOptionType.Neutral, "MadmateUseVentSetting", false, DemonOption);
        DemonIsCheckImpostor = Create(302305, true, CustomOptionType.Neutral, "MadmateIsCheckImpostorSetting", false, DemonOption);
        DemonIsAliveWin = Create(302306, true, CustomOptionType.Neutral, "DemonIsAliveWinSetting", false, DemonOption);

        NeetOption = SetupCustomRoleOption(302500, false, RoleId.Neet);
        NeetPlayerCount = Create(302501, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NeetOption);
        NeetIsAddWin = Create(302502, false, CustomOptionType.Neutral, "TunaAddWinSetting", false, NeetOption);

        SpelunkerOption = SetupCustomRoleOption(302600, false, RoleId.Spelunker);
        SpelunkerPlayerCount = Create(302601, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpelunkerOption);
        SpelunkerVentDeathChance = Create(302602, false, CustomOptionType.Neutral, "SpelunkerVentDeathChance", rates, SpelunkerOption);
        SpelunkerLadderDeadChance = Create(302603, false, CustomOptionType.Neutral, "LadderDeadChance", rates, SpelunkerOption);
        SpelunkerIsDeathCommsOrPowerdown = Create(302604, false, CustomOptionType.Neutral, "SpelunkerIsDeathCommsOrPowerdown", true, SpelunkerOption);
        SpelunkerDeathCommsOrPowerdownTime = Create(302605, false, CustomOptionType.Neutral, "SpelunkerDeathCommsOrPowerdownTime", 20f, 0f, 120f, 2.5f, SpelunkerIsDeathCommsOrPowerdown);
        SpelunkerLiftDeathChance = Create(302606, false, CustomOptionType.Neutral, "SpelunkerLiftDeathChance", rates, SpelunkerOption);
        SpelunkerDoorOpenChance = Create(302607, false, CustomOptionType.Neutral, "SpelunkerDoorOpenChance", rates, SpelunkerOption);

        SuicidalIdeationOption = SetupCustomRoleOption(302700, false, RoleId.SuicidalIdeation);
        SuicidalIdeationPlayerCount = Create(302701, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SuicidalIdeationOption);
        SuicidalIdeationWinText = Create(302702, false, CustomOptionType.Neutral, "SuicidalIdeationWinTextSetting", false, SuicidalIdeationOption);
        SuicidalIdeationTimeLeft = Create(302703, false, CustomOptionType.Neutral, "SuicidalIdeationTimeLeftSetting", 90f, 30f, 600f, 5f, SuicidalIdeationOption, format: "unitSeconds");
        SuicidalIdeationAddTimeLeft = Create(302704, false, CustomOptionType.Neutral, "SuicidalIdeationAddTimeLeftSetting", 20f, 0f, 300f, 5f, SuicidalIdeationOption, format: "unitSeconds");
        SuicidalIdeationFallProbability = Create(302705, false, CustomOptionType.Neutral, "SuicidalIdeationFallProbabilitySetting", rates, SuicidalIdeationOption);
        SuicidalIdeationIsSettingNumberOfUniqueTasks = Create(302706, false, CustomOptionType.Neutral, "IsSettingNumberOfUniqueTasks", true, SuicidalIdeationOption);
        var SuicidalIdeationoption = SelectTask.TaskSetting(302707, 302708, 302709, SuicidalIdeationIsSettingNumberOfUniqueTasks, CustomOptionType.Neutral, false);
        SuicidalIdeationCommonTask = SuicidalIdeationoption.Item1;
        SuicidalIdeationShortTask = SuicidalIdeationoption.Item2;
        SuicidalIdeationLongTask = SuicidalIdeationoption.Item3;

        PartTimerOption = SetupCustomRoleOption(302800, false, RoleId.PartTimer);
        PartTimerPlayerCount = Create(302801, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PartTimerOption);
        PartTimerDeathTurn = Create(302802, false, CustomOptionType.Neutral, "PartTimerDeathTurn", 3f, 0f, 15f, 1f, PartTimerOption);
        PartTimerCoolTime = Create(302803, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PartTimerOption);
        PartTimerIsCheckTargetRole = Create(302804, false, CustomOptionType.Neutral, "PartTimerIsCheckTargetRole", true, PartTimerOption);

        PhotographerOption = SetupCustomRoleOption(302900, false, RoleId.Photographer);
        PhotographerPlayerCount = Create(302901, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PhotographerOption);
        PhotographerCoolTime = Create(302902, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PhotographerOption);
        PhotographerIsBonus = Create(302903, false, CustomOptionType.Neutral, "PhotographerIsBonus", true, PhotographerOption);
        PhotographerBonusCount = Create(302904, false, CustomOptionType.Neutral, "PhotographerBonusCount", 5f, 1f, 15f, 1f, PhotographerIsBonus);
        PhotographerBonusCoolTime = Create(302905, false, CustomOptionType.Neutral, "PhotographerBonusCoolTime", 20f, 2.5f, 60f, 2.5f, PhotographerIsBonus);
        PhotographerIsImpostorVision = Create(302906, false, CustomOptionType.Neutral, "PhotographerIsImpostorVision", false, PhotographerOption);
        PhotographerIsNotification = Create(302907, false, CustomOptionType.Neutral, "PhotographerIsNotification", true, PhotographerOption);

        StefinderOption = SetupCustomRoleOption(303000, false, RoleId.Stefinder);
        StefinderPlayerCount = Create(303001, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StefinderOption);
        StefinderKillCooldown = Create(303002, false, CustomOptionType.Neutral, "StefinderKillCooldownSetting", 30f, 0f, 120f, 2.5f, StefinderOption, format: "unitSeconds");
        StefinderVent = Create(303003, false, CustomOptionType.Neutral, "StefinderVentSetting", false, StefinderOption);
        StefinderSabo = Create(303004, false, CustomOptionType.Neutral, "StefinderSaboSetting", false, StefinderOption);
        StefinderSoloWin = Create(303005, false, CustomOptionType.Neutral, "StefinderSoloWinSetting", false, StefinderOption);

        BlackHatHacker.SetupCustomOptions();

        Moira.SetupCustomOptions();

        Frankenstein.SetupCustomOptions();

        // SetupNeutralCustomOptions

        /* |: ========================= Crewmate Settings ========================== :| */

        SheriffOption = SetupCustomRoleOption(400000, true, RoleId.Sheriff);
        SheriffPlayerCount = Create(400001, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SheriffOption);
        SheriffCoolTime = Create(400002, true, CustomOptionType.Crewmate, "SheriffCooldownSetting", 30f, 2.5f, 60f, 2.5f, SheriffOption, format: "unitSeconds");
        SheriffKillMaxCount = Create(400003, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, SheriffOption, format: "unitSeconds");
        SheriffExecutionMode = Create(400011, true, CustomOptionType.Crewmate, "SheriffExecutionMode", new string[] { "SheriffDefaultExecutionMode", "SheriffAlwaysSuicideMode", "SheriffAlwaysKillMode" }, SheriffOption);
        SheriffCanKillImpostor = Create(400005, true, CustomOptionType.Crewmate, "SheriffIsKillImpostorSetting", true, SheriffOption);
        SheriffMadRoleKill = Create(400006, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, SheriffOption);
        SheriffNeutralKill = Create(400007, true, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, SheriffOption);
        SheriffFriendsRoleKill = Create(400008, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, SheriffOption);
        SheriffLoversKill = Create(400009, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, SheriffOption);
        SheriffQuarreledKill = Create(400010, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, SheriffOption);

        RemoteSheriffOption = SetupCustomRoleOption(400101, true, RoleId.RemoteSheriff);
        RemoteSheriffPlayerCount = Create(400102, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], RemoteSheriffOption);
        RemoteSheriffCoolTime = Create(400103, false, CustomOptionType.Crewmate, ModTranslation.GetString("SheriffCooldownSetting"), 30f, 2.5f, 60f, 2.5f, RemoteSheriffOption, format: "unitSeconds");
        RemoteSheriffKillMaxCount = Create(400104, true, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, RemoteSheriffOption, format: "unitSeconds");
        RemoteSheriffIsKillTeleportSetting = Create(400105, true, CustomOptionType.Crewmate, "RemoteSheriffIsKillTeleportSetting", false, RemoteSheriffOption);
        RemoteSheriffExecutionMode = Create(400112, true, CustomOptionType.Crewmate, "SheriffExecutionMode", new string[] { "SheriffDefaultExecutionMode", "SheriffAlwaysSuicideMode", "SheriffAlwaysKillMode" }, RemoteSheriffOption);
        RemoteSheriffMadRoleKill = Create(400107, true, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, RemoteSheriffOption);
        RemoteSheriffNeutralKill = Create(400108, true, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, RemoteSheriffOption);
        RemoteSheriffFriendRolesKill = Create(400109, true, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, RemoteSheriffOption);
        RemoteSheriffLoversKill = Create(400110, true, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, RemoteSheriffOption);
        RemoteSheriffQuarreledKill = Create(400111, true, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, RemoteSheriffOption);

        MeetingSheriffOption = SetupCustomRoleOption(400201, false, RoleId.MeetingSheriff);
        MeetingSheriffPlayerCount = Create(400202, false, CustomOptionType.Crewmate, Cs(Color.white, "SettingPlayerCountName"), CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MeetingSheriffOption);
        MeetingSheriffKillMaxCount = Create(400203, false, CustomOptionType.Crewmate, "MeetingSheriffMaxKillCountSetting", 1f, 1f, 20f, 1f, MeetingSheriffOption, format: "unitSeconds");
        MeetingSheriffOneMeetingMultiKill = Create(400204, false, CustomOptionType.Crewmate, "MeetingSheriffMeetingmultipleKillSetting", false, MeetingSheriffOption);
        MeetingSheriffExecutionMode = Create(400211, true, CustomOptionType.Crewmate, "SheriffExecutionMode", new string[] { "SheriffDefaultExecutionMode", "SheriffAlwaysSuicideMode", "SheriffAlwaysKillMode" }, MeetingSheriffOption);
        MeetingSheriffMadRoleKill = Create(400206, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillMadRoleSetting", false, MeetingSheriffOption);
        MeetingSheriffNeutralKill = Create(400207, false, CustomOptionType.Crewmate, "MeetingSheriffIsKillNeutralSetting", false, MeetingSheriffOption);
        MeetingSheriffFriendsRoleKill = Create(400208, false, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, MeetingSheriffOption);
        MeetingSheriffLoversKill = Create(400209, false, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, MeetingSheriffOption);
        MeetingSheriffQuarreledKill = Create(400210, false, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, MeetingSheriffOption);

        ChiefOption = SetupCustomRoleOption(400301, false, RoleId.Chief);
        ChiefPlayerCount = Create(400302, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ChiefOption);
        ChiefSheriffCoolTime = Create(400303, false, CustomOptionType.Crewmate, "SheriffCooldownSetting", 30f, 2.5f, 60f, 2.5f, ChiefOption, format: "unitSeconds");
        ChiefSheriffKillLimit = Create(400304, false, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, ChiefOption, format: "unitSeconds");
        ChiefSheriffExecutionMode = Create(400312, true, CustomOptionType.Crewmate, "SheriffExecutionMode", new string[] { "SheriffDefaultExecutionMode", "SheriffAlwaysSuicideMode", "SheriffAlwaysKillMode" }, ChiefOption);
        ChiefSheriffCanKillImpostor = Create(400306, false, CustomOptionType.Crewmate, "SheriffIsKillImpostorSetting", true, ChiefOption);
        ChiefSheriffCanKillMadRole = Create(400307, false, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, ChiefOption);
        ChiefSheriffCanKillNeutral = Create(400308, false, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, ChiefOption);
        ChiefSheriffFriendsRoleKill = Create(400309, false, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, ChiefOption);
        ChiefSheriffCanKillLovers = Create(400310, false, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, ChiefOption);
        ChiefSheriffQuarreledKill = Create(400311, false, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, ChiefOption);

        WiseMan.SetupCustomOptions();

        Pteranodon.SetupCustomOptions();

        JumpDancer.SetupCustomOptions();

        WellBehaver.SetupCustomOptions();

        Balancer.SetupCustomOptions();

        MayorOption = SetupCustomRoleOption(400700, true, RoleId.Mayor);
        MayorPlayerCount = Create(400701, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorOption);
        MayorVoteCount = Create(400702, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorOption);

        GhostMechanicOption = SetupCustomRoleOption(400800, true, RoleId.GhostMechanic);
        GhostMechanicPlayerCount = Create(400801, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], GhostMechanicOption);
        GhostMechanicRepairLimit = Create(400802, true, CustomOptionType.Crewmate, "GhostMechanicRepairLimitSetting", 1f, 1f, 30f, 1f, GhostMechanicOption);
        GhostMechanicCooldown = Create(400803, true, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 60f, 0f, 120f, 5f, GhostMechanicOption);

        MadmateOption = SetupCustomRoleOption(400900, true, RoleId.Madmate);
        MadmatePlayerCount = Create(400901, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadmateOption);
        MadmateIsCheckImpostor = Create(400902, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadmateOption);
        MadmateIsSettingNumberOfUniqueTasks = Create(400903, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadmateIsCheckImpostor);
        var madmateoption = SelectTask.TaskSetting(400904, 400905, 400906, MadmateIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadmateCommonTask = madmateoption.Item1;
        MadmateShortTask = madmateoption.Item2;
        MadmateLongTask = madmateoption.Item3;
        MadmateIsParcentageForTaskTrigger = Create(400907, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadmateIsCheckImpostor);
        MadmateParcentageForTaskTriggerSetting = Create(400908, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadmateIsParcentageForTaskTrigger);
        MadmateIsUseVent = Create(400909, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadmateOption);
        MadmateIsImpostorLight = Create(400910, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadmateOption);

        BlackCatOption = SetupCustomRoleOption(401000, true, RoleId.BlackCat);
        BlackCatPlayerCount = Create(401001, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BlackCatOption);
        BlackCatNotImpostorExiled = Create(401002, true, CustomOptionType.Crewmate, "NotImpostorExiled", false, BlackCatOption);
        BlackCatIsCheckImpostor = Create(401003, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, BlackCatOption);
        BlackCatIsSettingNumberOfUniqueTasks = Create(401004, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, BlackCatIsCheckImpostor);
        var blackcatoption = SelectTask.TaskSetting(401005, 401006, 401007, BlackCatIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        BlackCatCommonTask = blackcatoption.Item1;
        BlackCatShortTask = blackcatoption.Item2;
        BlackCatLongTask = blackcatoption.Item3;
        BlackCatIsParcentageForTaskTrigger = Create(401008, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, BlackCatIsCheckImpostor);
        BlackCatParcentageForTaskTriggerSetting = Create(401009, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, BlackCatIsParcentageForTaskTrigger);
        BlackCatIsUseVent = Create(401010, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, BlackCatOption);
        BlackCatIsImpostorLight = Create(401011, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, BlackCatOption);

        Worshiper.CustomOptionData.SetupCustomOptions();

        MadRaccoon.CustomOptionData.SetupCustomOptions();

        MadJesterOption = SetupCustomRoleOption(401200, true, RoleId.MadJester);
        MadJesterPlayerCount = Create(401201, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadJesterOption);
        MadJesterIsUseVent = Create(401202, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadJesterOption);
        MadJesterIsImpostorLight = Create(401203, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadJesterOption);
        MadJesterIsSettingNumberOfUniqueTasks = Create(401204, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadJesterOption);
        var MadJesteroption = SelectTask.TaskSetting(401205, 401206, 401207, MadJesterIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadJesterCommonTask = MadJesteroption.Item1;
        MadJesterShortTask = MadJesteroption.Item2;
        MadJesterLongTask = MadJesteroption.Item3;
        IsMadJesterTaskClearWin = Create(401208, true, CustomOptionType.Crewmate, "JesterIsWinClearTaskSetting", false, MadJesterOption);
        MadJesterIsCheckImpostor = Create(401209, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadJesterOption);
        MadJesterIsParcentageForTaskTrigger = Create(401210, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadJesterIsCheckImpostor);
        MadJesterParcentageForTaskTriggerSetting = Create(401211, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadJesterIsParcentageForTaskTrigger);

        MadSeerOption = SetupCustomRoleOption(401300, true, RoleId.MadSeer);
        MadSeerPlayerCount = Create(401301, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadSeerOption);
        MadSeerMode = Create(401302, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, MadSeerOption);
        MadSeerLimitSoulDuration = Create(401303, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, MadSeerOption);
        MadSeerSoulDuration = Create(401304, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, MadSeerLimitSoulDuration, format: "unitCouples");
        MadSeerIsUseVent = Create(401305, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadSeerOption);
        MadSeerIsImpostorLight = Create(401306, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadSeerOption);
        MadSeerIsCheckImpostor = Create(401307, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadSeerOption);
        MadSeerIsSettingNumberOfUniqueTasks = Create(401308, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadSeerIsCheckImpostor);
        var madseeroption = SelectTask.TaskSetting(401309, 401310, 401311, MadSeerIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadSeerCommonTask = madseeroption.Item1;
        MadSeerShortTask = madseeroption.Item2;
        MadSeerLongTask = madseeroption.Item3;
        MadSeerIsParcentageForTaskTrigger = Create(401312, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadSeerIsCheckImpostor);
        MadSeerParcentageForTaskTriggerSetting = Create(401313, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadSeerIsParcentageForTaskTrigger);

        MadMayorOption = SetupCustomRoleOption(401400, true, RoleId.MadMayor);
        MadMayorPlayerCount = Create(401401, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMayorOption);
        MadMayorVoteCount = Create(401402, true, CustomOptionType.Crewmate, "MadMayorVoteCountSetting", 2f, 1f, 100f, 1f, MadMayorOption);
        MadMayorIsCheckImpostor = Create(401403, true, CustomOptionType.Crewmate, "MadMayorIsCheckImpostorSetting", false, MadMayorOption);
        MadMayorIsSettingNumberOfUniqueTasks = Create(401404, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadMayorIsCheckImpostor);
        var madmayoroption = SelectTask.TaskSetting(401405, 401406, 401407, MadMayorIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadMayorCommonTask = madmayoroption.Item1;
        MadMayorShortTask = madmayoroption.Item2;
        MadMayorLongTask = madmayoroption.Item3;
        MadMayorIsParcentageForTaskTrigger = Create(401408, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadMayorIsCheckImpostor);
        MadMayorParcentageForTaskTriggerSetting = Create(401409, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadMayorIsParcentageForTaskTrigger);
        MadMayorIsUseVent = Create(401410, true, CustomOptionType.Crewmate, "MadMayorUseVentSetting", false, MadMayorOption);
        MadMayorIsImpostorLight = Create(401411, true, CustomOptionType.Crewmate, "MadMayorImpostorLightSetting", false, MadMayorOption);

        MadMakerOption = SetupCustomRoleOption(401500, true, RoleId.MadMaker);
        MadMakerPlayerCount = Create(401501, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadMakerOption);
        MadMakerIsUseVent = Create(401502, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadMakerOption);
        MadMakerIsImpostorLight = Create(401503, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadMakerOption);

        MadHawkOption = SetupCustomRoleOption(401600, false, RoleId.MadHawk);
        MadHawkPlayerCount = Create(401601, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadHawkOption);
        MadHawkCoolTime = Create(401602, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 0f, 120f, 2.5f, MadHawkOption, format: "unitCouples");
        MadHawkDurationTime = Create(401603, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 0.5f, MadHawkOption, format: "unitCouples");
        MadHawkIsCheckImpostor = Create(401604, false, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadHawkOption);
        MadHawkIsSettingNumberOfUniqueTasks = Create(401605, false, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadHawkIsCheckImpostor);
        var madhawkoption = SelectTask.TaskSetting(401606, 401607, 401608, MadHawkIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadHawkCommonTask = madhawkoption.Item1;
        MadHawkShortTask = madhawkoption.Item2;
        MadHawkLongTask = madhawkoption.Item3;
        MadHawkIsParcentageForTaskTrigger = Create(401609, false, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadHawkIsCheckImpostor);
        MadHawkParcentageForTaskTriggerSetting = Create(401610, false, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadHawkIsParcentageForTaskTrigger);
        MadHawkIsUseVent = Create(401611, false, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadHawkOption);
        MadHawkIsImpostorLight = Create(401612, false, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadHawkOption);

        MadCleanerOption = SetupCustomRoleOption(401700, false, RoleId.MadCleaner);
        MadCleanerPlayerCount = Create(401701, false, CustomOptionType.Crewmate, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], MadCleanerOption);
        MadCleanerCooldown = Create(401702, false, CustomOptionType.Crewmate, "CleanerCooldownSetting", 30f, 2.5f, 60f, 2.5f, MadCleanerOption, format: "unitSeconds");
        MadCleanerIsCheckImpostor = Create(401703, true, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadCleanerOption);
        MadCleanerIsSettingNumberOfUniqueTasks = Create(401704, false, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadCleanerIsCheckImpostor);
        var MadCleaneroption = SelectTask.TaskSetting(401705, 401706, 401707, MadCleanerIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadCleanerCommonTask = MadCleaneroption.Item1;
        MadCleanerShortTask = MadCleaneroption.Item2;
        MadCleanerLongTask = MadCleaneroption.Item3;
        MadCleanerIsParcentageForTaskTrigger = Create(401708, false, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadCleanerIsCheckImpostor);
        MadCleanerParcentageForTaskTriggerSetting = Create(401709, false, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadCleanerIsParcentageForTaskTrigger);
        MadCleanerIsUseVent = Create(401710, false, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadCleanerOption);
        MadCleanerIsImpostorLight = Create(401711, false, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadCleanerOption);

        MadStuntManOption = SetupCustomRoleOption(401800, false, RoleId.MadStuntMan);
        MadStuntManPlayerCount = Create(401801, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MadStuntManOption);
        MadStuntManIsCheckImpostor = Create(401802, false, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, MadStuntManOption);
        MadStuntManIsSettingNumberOfUniqueTasks = Create(401803, false, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MadStuntManIsCheckImpostor);
        var MadStuntManoption = SelectTask.TaskSetting(401804, 401805, 401806, MadStuntManIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MadStuntManCommonTask = MadStuntManoption.Item1;
        MadStuntManShortTask = MadStuntManoption.Item2;
        MadStuntManLongTask = MadStuntManoption.Item3;
        MadStuntManIsParcentageForTaskTrigger = Create(401807, false, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MadStuntManIsCheckImpostor);
        MadStuntManParcentageForTaskTriggerSetting = Create(401808, false, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MadStuntManIsParcentageForTaskTrigger);
        MadStuntManIsUseVent = Create(401809, false, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MadStuntManOption);
        MadStuntManIsImpostorLight = Create(401810, false, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MadStuntManOption);

        JackalFriendsOption = SetupCustomRoleOption(402000, true, RoleId.JackalFriends);
        JackalFriendsPlayerCount = Create(402001, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JackalFriendsOption);
        JackalFriendsIsUseVent = Create(402002, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, JackalFriendsOption);
        JackalFriendsIsImpostorLight = Create(402003, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, JackalFriendsOption);
        JackalFriendsIsCheckJackal = Create(402004, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, JackalFriendsOption);
        JackalFriendsIsSettingNumberOfUniqueTasks = Create(402005, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, JackalFriendsIsCheckJackal);
        var JackalFriendsoption = SelectTask.TaskSetting(402006, 402007, 402008, JackalFriendsIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        JackalFriendsCommonTask = JackalFriendsoption.Item1;
        JackalFriendsShortTask = JackalFriendsoption.Item2;
        JackalFriendsLongTask = JackalFriendsoption.Item3;
        JackalFriendsIsParcentageForTaskTrigger = Create(402009, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, JackalFriendsIsCheckJackal);
        JackalFriendsParcentageForTaskTriggerSetting = Create(402010, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, JackalFriendsIsParcentageForTaskTrigger);

        SeerFriendsOption = SetupCustomRoleOption(402100, true, RoleId.SeerFriends);
        SeerFriendsPlayerCount = Create(402101, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerFriendsOption);
        SeerFriendsMode = Create(402102, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerFriendsOption);
        SeerFriendsLimitSoulDuration = Create(402103, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerFriendsOption);
        SeerFriendsSoulDuration = Create(402104, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerFriendsLimitSoulDuration, format: "unitCouples");
        SeerFriendsIsUseVent = Create(402105, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, SeerFriendsOption);
        SeerFriendsIsImpostorLight = Create(402106, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, SeerFriendsOption);
        SeerFriendsIsCheckJackal = Create(402107, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, SeerFriendsOption);
        SeerFriendsIsSettingNumberOfUniqueTasks = Create(402108, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, SeerFriendsIsCheckJackal);
        var SeerFriendsoption = SelectTask.TaskSetting(402109, 402110, 402111, SeerFriendsIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        SeerFriendsCommonTask = SeerFriendsoption.Item1;
        SeerFriendsShortTask = SeerFriendsoption.Item2;
        SeerFriendsLongTask = SeerFriendsoption.Item3;
        SeerFriendsIsParcentageForTaskTrigger = Create(402112, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, SeerFriendsIsCheckJackal);
        SeerFriendsParcentageForTaskTriggerSetting = Create(402113, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, SeerFriendsIsParcentageForTaskTrigger);

        MayorFriendsOption = SetupCustomRoleOption(402200, true, RoleId.MayorFriends);
        MayorFriendsPlayerCount = Create(402201, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MayorFriendsOption);
        MayorFriendsIsUseVent = Create(402202, true, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, MayorFriendsOption);
        MayorFriendsIsImpostorLight = Create(402203, true, CustomOptionType.Crewmate, "MadmateImpostorLightSetting", false, MayorFriendsOption);
        MayorFriendsIsCheckJackal = Create(402204, true, CustomOptionType.Crewmate, "JackalFriendsIsCheckJackalSetting", false, MayorFriendsOption);
        MayorFriendsIsSettingNumberOfUniqueTasks = Create(402205, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, MayorFriendsIsCheckJackal);
        var MayorFriendsoption = SelectTask.TaskSetting(402206, 402207, 402208, MayorFriendsIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        MayorFriendsCommonTask = MayorFriendsoption.Item1;
        MayorFriendsShortTask = MayorFriendsoption.Item2;
        MayorFriendsLongTask = MayorFriendsoption.Item3;
        MayorFriendsIsParcentageForTaskTrigger = Create(402209, true, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, MayorFriendsIsCheckJackal);
        MayorFriendsParcentageForTaskTriggerSetting = Create(402210, true, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", rates4, MayorFriendsIsParcentageForTaskTrigger);
        MayorFriendsVoteCount = Create(402211, true, CustomOptionType.Crewmate, "MayorVoteCountSetting", 2f, 1f, 100f, 1f, MayorFriendsOption);

        LighterOption = SetupCustomRoleOption(402301, false, RoleId.Lighter);
        LighterPlayerCount = Create(402302, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], LighterOption);
        LighterCoolTime = Create(402303, false, CustomOptionType.Crewmate, "LigtherCooldownSetting", 30f, 2.5f, 60f, 2.5f, LighterOption, format: "unitSeconds");
        LighterDurationTime = Create(402304, false, CustomOptionType.Crewmate, "LigtherDurationSetting", 10f, 0f, 180f, 5f, LighterOption, format: "unitSeconds");
        LighterUpVision = Create(402305, false, CustomOptionType.Crewmate, "LighterUpVisionSetting", 0.25f, 0f, 5f, 0.25f, LighterOption);

        CelebrityOption = SetupCustomRoleOption(402400, true, RoleId.Celebrity);
        CelebrityPlayerCount = Create(402401, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], CelebrityOption);
        CelebrityChangeRoleView = Create(402402, true, CustomOptionType.Crewmate, "CelebrityChangeRoleViewSetting", false, CelebrityOption);
        CelebrityIsTaskPhaseFlash = Create(402403, false, CustomOptionType.Crewmate, "CelebrityIsTaskPhaseFlashSetting", false, CelebrityOption);
        CelebrityIsFlashWhileAlivingOnly = Create(402404, false, CustomOptionType.Crewmate, "CelebrityIsFlashWhileAlivingOnly", false, CelebrityIsTaskPhaseFlash);


        ToiletFanOption = SetupCustomRoleOption(405900, true, RoleId.ToiletFan);
        ToiletFanPlayerCount = Create(405901, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ToiletFanOption);
        ToiletFanCoolTime = Create(405902, true, CustomOptionType.Crewmate, "ToiletCooldownSetting", 30f, 0f, 60f, 2.5f, ToiletFanOption);

        PoliceSurgeon.CustomOptionData.SetupCustomOptions();

        DoctorOption = SetupCustomRoleOption(404700, false, RoleId.Doctor);
        DoctorPlayerCount = Create(404701, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoctorOption);
        DoctorChargeTime = Create(404702, false, CustomOptionType.Crewmate, "DoctorChargeTime", 10f, 0f, 60f, 2.5f, DoctorOption);
        DoctorUseTime = Create(404703, false, CustomOptionType.Crewmate, "DoctorUseTime", 5f, 0f, 60f, 2.5f, DoctorOption);

        DyingMessenger.SetupCustomOptions();

        SoothSayerOption = SetupCustomRoleOption(402500, false, RoleId.SoothSayer);
        SoothSayerPlayerCount = Create(402501, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
        SoothSayerDisplayMode = Create(402502, false, CustomOptionType.Crewmate, "SoothSayerDisplaySetting", false, SoothSayerOption);
        SoothSayerMaxCount = Create(402503, false, CustomOptionType.Crewmate, "SoothSayerMaxCountSetting", CrewPlayers[0] - 1, CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SoothSayerOption);
        SoothSayerFirstWhiteOption = Create(402504, false, CustomOptionType.Crewmate, "SoothSayerFirstWhiteOption", false, SoothSayerOption);

        SpiritMediumOption = SetupCustomRoleOption(402600, false, RoleId.SpiritMedium);
        SpiritMediumPlayerCount = Create(402601, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpiritMediumOption);
        SpiritMediumIsAutoMode = Create(402602, false, CustomOptionType.Crewmate, "SpiritMediumIsAutoMode", false, SpiritMediumOption);
        SpiritMediumDisplayMode = Create(402603, false, CustomOptionType.Crewmate, "SpiritMediumDisplaySetting", false, SpiritMediumOption);
        SpiritMediumMaxCount = Create(402604, false, CustomOptionType.Crewmate, "SpiritMediumMaxCountSetting", 2f, 1f, 15f, 1f, SpiritMediumOption);

        Squid.SetupCustomOptions();

        HamburgerShopOption = SetupCustomRoleOption(402900, true, RoleId.HamburgerShop);
        HamburgerShopPlayerCount = Create(402901, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HamburgerShopOption);
        HamburgerShopChangeTaskPrefab = Create(402902, false, CustomOptionType.Crewmate, "HamburgerShopChangeTaskPrefab", true, HamburgerShopOption);
        HamburgerShopIsSettingNumberOfUniqueTasks = Create(402903, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, HamburgerShopOption);
        var HamburgerShopoption = SelectTask.TaskSetting(402904, 402905, 402906, HamburgerShopIsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, true);
        HamburgerShopCommonTask = HamburgerShopoption.Item1;
        HamburgerShopShortTask = HamburgerShopoption.Item2;
        HamburgerShopLongTask = HamburgerShopoption.Item3;

        BaitOption = SetupCustomRoleOption(403100, true, RoleId.Bait);
        BaitPlayerCount = Create(403101, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BaitOption);
        BaitReportTime = Create(403102, true, CustomOptionType.Crewmate, "BaitReportTimeSetting", 2f, 1f, 4f, 0.5f, BaitOption);

        NiceMechanic.SetupCustomOptions();

        NiceNekomataOption = SetupCustomRoleOption(403500, true, RoleId.NiceNekomata);
        NiceNekomataPlayerCount = Create(403501, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceNekomataOption);
        NiceNekomataIsChain = Create(403502, true, CustomOptionType.Crewmate, "NiceNekomataIsChainSetting", true, NiceNekomataOption);

        SeerOption = SetupCustomRoleOption(403600, true, RoleId.Seer);
        SeerPlayerCount = Create(403601, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeerOption);
        SeerMode = Create(403602, false, CustomOptionType.Crewmate, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerOption);
        SeerLimitSoulDuration = Create(403603, false, CustomOptionType.Crewmate, "SeerLimitSoulDuration", false, SeerOption);
        SeerSoulDuration = Create(403604, false, CustomOptionType.Crewmate, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerLimitSoulDuration, format: "unitCouples");

        NiceButtonerOption = SetupCustomRoleOption(403700, true, RoleId.NiceButtoner);
        NiceButtonerPlayerCount = Create(403701, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceButtonerOption);
        NiceButtonerCoolTime = Create(403702, false, CustomOptionType.Crewmate, "ButtonerCooldownSetting", 20f, 2.5f, 60f, 2.5f, NiceButtonerOption, format: "unitSeconds");//クールタイムはSHR未対応の為false
        NiceButtonerCount = Create(403703, true, CustomOptionType.Crewmate, "ButtonerCountSetting", 1f, 1f, 10f, 1f, NiceButtonerOption);

        TaskManagerOption = SetupCustomRoleOption(403800, true, RoleId.TaskManager);
        TaskManagerPlayerCount = Create(403801, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TaskManagerOption);
        var taskmanageroption = SelectTask.TaskSetting(403802, 403803, 403804, TaskManagerOption, CustomOptionType.Crewmate, true);
        TaskManagerCommonTask = taskmanageroption.Item1;
        TaskManagerShortTask = taskmanageroption.Item2;
        TaskManagerLongTask = taskmanageroption.Item3;

        DoorrOption = SetupCustomRoleOption(403900, false, RoleId.Doorr);
        DoorrPlayerCount = Create(403901, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], DoorrOption);
        DoorrCoolTime = Create(403902, false, CustomOptionType.Crewmate, "DoorrCoolTimeSetting", 2.5f, 2.5f, 60f, 2.5f, DoorrOption);

        SpeedBoosterOption = SetupCustomRoleOption(404000, false, RoleId.SpeedBooster);
        SpeedBoosterPlayerCount = Create(404001, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpeedBoosterOption);
        SpeedBoosterCoolTime = Create(404002, false, CustomOptionType.Crewmate, "SpeedBoosterCooldownSetting", 30f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
        SpeedBoosterDurationTime = Create(404003, false, CustomOptionType.Crewmate, "SpeedBoosterDurationSetting", 15f, 2.5f, 60f, 2.5f, SpeedBoosterOption, format: "unitSeconds");
        SpeedBoosterSpeed = Create(404004, false, CustomOptionType.Crewmate, "SpeedBoosterPlusSpeedSetting", 1.25f, 0.0f, 5f, 0.25f, SpeedBoosterOption, format: "unitSeconds");

        ShielderOption = SetupCustomRoleOption(404100, false, RoleId.Shielder);
        ShielderPlayerCount = Create(404101, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ShielderOption);
        ShielderCoolTime = Create(404102, false, CustomOptionType.Crewmate, "ShielderCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, ShielderOption, format: "unitCouples");
        ShielderDurationTime = Create(404103, false, CustomOptionType.Crewmate, "ShielderDurationSetting", 10f, 2.5f, 30f, 2.5f, ShielderOption, format: "unitCouples");

        ClergymanOption = SetupCustomRoleOption(404300, false, RoleId.Clergyman);
        ClergymanPlayerCount = Create(404301, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ClergymanOption);
        ClergymanCoolTime = Create(404302, false, CustomOptionType.Crewmate, "ClergymanCooldownSetting", 30f, 2.5f, 60f, 2.5f, ClergymanOption, format: "unitSeconds");
        ClergymanDurationTime = Create(404303, false, CustomOptionType.Crewmate, "ClergymanDurationTimeSetting", 10f, 1f, 20f, 0.5f, ClergymanOption, format: "unitSeconds");
        ClergymanDownVision = Create(404304, false, CustomOptionType.Crewmate, "ClergymanDownVisionSetting", 0.25f, 0f, 5f, 0.25f, ClergymanOption);

        HomeSecurityGuardOption = SetupCustomRoleOption(404400, true, RoleId.HomeSecurityGuard);
        HomeSecurityGuardPlayerCount = Create(404401, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], HomeSecurityGuardOption);

        StuntManOption = SetupCustomRoleOption(404500, true, RoleId.StuntMan);
        StuntManPlayerCount = Create(404501, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], StuntManOption);
        StuntManMaxGuardCount = Create(404502, true, CustomOptionType.Crewmate, "StuntManGuardMaxCountSetting", 1f, 1f, 15f, 1f, StuntManOption);

        MovingOption = SetupCustomRoleOption(404600, false, RoleId.Moving);
        MovingPlayerCount = Create(404601, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], MovingOption);
        MovingCoolTime = Create(404602, false, CustomOptionType.Crewmate, "MovingCooldownSetting", 30f, 0f, 60f, 2.5f, MovingOption);

        TechnicianOption = SetupCustomRoleOption(404800, true, RoleId.Technician);
        TechnicianPlayerCount = Create(404801, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], TechnicianOption);

        NiceHawkOption = SetupCustomRoleOption(404900, false, RoleId.NiceHawk);
        NiceHawkPlayerCount = Create(404901, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceHawkOption);
        NiceHawkCoolTime = Create(404902, false, CustomOptionType.Crewmate, "HawkCoolTimeSetting", 15f, 0f, 120f, 2.5f, NiceHawkOption, format: "unitCouples");
        NiceHawkDurationTime = Create(404903, false, CustomOptionType.Crewmate, "HawkDurationTimeSetting", 5f, 0f, 60f, 0.5f, NiceHawkOption, format: "unitCouples");

        BakeryOption = SetupCustomRoleOption(405000, true, RoleId.Bakery);
        BakeryPlayerCount = Create(405001, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], BakeryOption);

        NiceTeleporterOption = SetupCustomRoleOption(405100, false, RoleId.NiceTeleporter);
        NiceTeleporterPlayerCount = Create(405101, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NiceTeleporterOption);
        NiceTeleporterCoolTime = Create(405102, false, CustomOptionType.Crewmate, "NiceTeleporterCooldownSetting", 30f, 2.5f, 60f, 2.5f, NiceTeleporterOption, format: "unitSeconds");
        NiceTeleporterDurationTime = Create(405103, false, CustomOptionType.Crewmate, "TeleporterTeleportTimeSetting", 10f, 1f, 20f, 0.5f, NiceTeleporterOption, format: "unitSeconds");

        NocturnalityOption = SetupCustomRoleOption(405200, true, RoleId.Nocturnality);
        NocturnalityPlayerCount = Create(405201, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], NocturnalityOption);

        PainterOption = SetupCustomRoleOption(405300, false, RoleId.Painter);
        PainterPlayerCount = Create(405301, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PainterOption);
        PainterCoolTime = Create(405302, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PainterOption);
        PainterIsTaskCompleteFootprint = Create(405303, false, CustomOptionType.Crewmate, "PainterIsTaskCompleteFootprint", true, PainterOption);
        PainterIsSabotageRepairFootprint = Create(405304, false, CustomOptionType.Crewmate, "PainterIsSabotageRepairFootprint", true, PainterOption);
        PainterIsInVentFootprint = Create(405305, false, CustomOptionType.Crewmate, "PainterIsInVentFootprint", true, PainterOption);
        PainterIsExitVentFootprint = Create(405306, false, CustomOptionType.Crewmate, "PainterIsExitVentFootprint", true, PainterOption);
        PainterIsCheckVitalFootprint = Create(405307, false, CustomOptionType.Crewmate, "PainterIsCheckVitalFootprint", false, PainterOption);
        PainterIsCheckAdminFootprint = Create(405308, false, CustomOptionType.Crewmate, "PainterIsCheckAdminFootprint", false, PainterOption);
        PainterIsDeathFootprint = Create(405309, false, CustomOptionType.Crewmate, "PainterIsDeathFootprint", true, PainterOption);
        PainterIsDeathFootprintBig = Create(405310, false, CustomOptionType.Crewmate, "PainterIsDeathFootprintBig", true, PainterIsDeathFootprint);
        PainterIsFootprintMeetingDestroy = Create(405311, false, CustomOptionType.Crewmate, "PainterIsFootprintMeetingDestroy", true, PainterOption);

        SeeThroughPersonOption = SetupCustomRoleOption(405400, false, RoleId.SeeThroughPerson, isHidden: true);
        SeeThroughPersonOption.selection = 0;
        SeeThroughPersonPlayerCount = Create(405401, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SeeThroughPersonOption, isHidden: true);

        PsychometristOption = SetupCustomRoleOption(405500, false, RoleId.Psychometrist);
        PsychometristPlayerCount = Create(405501, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], PsychometristOption);
        PsychometristCoolTime = Create(405502, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 20f, 2.5f, 60f, 2.5f, PsychometristOption);
        PsychometristReadTime = Create(405503, false, CustomOptionType.Crewmate, "PsychometristReadTime", 5f, 0f, 15f, 0.5f, PsychometristOption);
        PsychometristIsCheckDeathTime = Create(405504, false, CustomOptionType.Crewmate, "PsychometristIsCheckDeathTime", true, PsychometristOption);
        PsychometristDeathTimeDeviation = Create(405505, false, CustomOptionType.Crewmate, "PsychometristDeathTimeDeviation", 3f, 0f, 30f, 1f, PsychometristIsCheckDeathTime);
        PsychometristIsCheckDeathReason = Create(405506, false, CustomOptionType.Crewmate, "PsychometristIsCheckDeathReason", true, PsychometristOption);
        PsychometristIsCheckFootprints = Create(405507, false, CustomOptionType.Crewmate, "PsychometristIsCheckFootprints", true, PsychometristOption);
        PsychometristCanCheckFootprintsTime = Create(405508, false, CustomOptionType.Crewmate, "PsychometristCanCheckFootprintsTime", 7.5f, 0.5f, 60f, 0.5f, PsychometristIsCheckFootprints);
        PsychometristIsReportCheckedDeadBody = Create(405509, false, CustomOptionType.Crewmate, "PsychometristIsReportCheckedDeadBody", false, PsychometristOption);

        SpyOption = SetupCustomRoleOption(405700, true, RoleId.Spy);
        SpyPlayerCount = Create(405701, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], SpyOption);
        SpyCanUseVent = Create(405702, true, CustomOptionType.Crewmate, "JesterIsVentSetting", false, SpyOption);

        Knight.SetupCustomOptions();

        // SetupCrewmateCustomOptions

        /* |: ========================= Modifiers Settings ========================== :| */

        // SetupModifierCustomOptions

        RoleBaseHelper.SetUpOptions();

        // 表示設定

        MadRolesCanFixComms = Create(500000, true, CustomOptionType.Modifier, "MadRolesCanFixComms", false, null, isHeader: true);
        MadRolesCanFixElectrical = Create(500001, true, CustomOptionType.Modifier, "MadRolesCanFixElectrical", false, null);
        MadRolesCanVentMove = Create(500002, false, CustomOptionType.Modifier, "MadRolesCanVentMove", false, null);

        HauntedWolf.CustomOptionData.SetUpCustomRoleOptions();

        LoversOption = Create(500200, true, CustomOptionType.Modifier, Cs(RoleClass.Lovers.color, "LoversName"), false, null, isHeader: true);
        LoversTeamCount = Create(500201, true, CustomOptionType.Modifier, "LoversTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], LoversOption);
        LoversPar = Create(500202, true, CustomOptionType.Modifier, "LoversParSetting", rates, LoversOption);
        LoversOnlyCrewmate = Create(500203, true, CustomOptionType.Modifier, "LoversOnlyCrewmateSetting", false, LoversOption);
        LoversSingleTeam = Create(500204, true, CustomOptionType.Modifier, "LoversSingleTeamSetting", true, LoversOption);
        LoversSameDie = Create(500205, true, CustomOptionType.Modifier, "LoversSameDieSetting", true, LoversOption);
        LoversAliveTaskCount = Create(500206, true, CustomOptionType.Modifier, "LoversAliveTaskCountSetting", false, LoversOption);
        LoversDuplicationQuarreled = Create(500207, true, CustomOptionType.Modifier, "LoversDuplicationQuarreledSetting", true, LoversOption);
        var loversoption = SelectTask.TaskSetting(500208, 500209, 500210, LoversOption, CustomOptionType.Modifier, true);

        LoversCommonTask = loversoption.Item1;
        LoversShortTask = loversoption.Item2;
        LoversLongTask = loversoption.Item3;

        QuarreledOption = Create(500100, true, CustomOptionType.Modifier, Cs(RoleClass.Quarreled.color, "QuarreledName"), false, null, isHeader: true);
        QuarreledTeamCount = Create(500101, true, CustomOptionType.Modifier, "QuarreledTeamCountSetting", QuarreledPlayers[0], QuarreledPlayers[1], QuarreledPlayers[2], QuarreledPlayers[3], QuarreledOption);
        QuarreledOnlyCrewmate = Create(500102, true, CustomOptionType.Modifier, "QuarreledOnlyCrewmateSetting", false, QuarreledOption);

        JumboOption = SetupCustomRoleOption(500300, false, RoleId.Jumbo, type: CustomOptionType.Modifier);
        JumboPlayerCount = Create(500301, false, CustomOptionType.Modifier, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], JumboOption);
        JumboCrewmateChance = Create(500302, false, CustomOptionType.Modifier, "JumboCrewmateChance", rates, JumboOption);
        JumboMaxSize = Create(500303, false, CustomOptionType.Modifier, "JumboMaxSize", 24f, 1f, 48f, 1f, JumboOption);
        JumboSpeedUpSize = Create(500304, false, CustomOptionType.Modifier, "JumboSpeedUpSize", 300f, 10f, 600f, 10f, JumboOption);
        JumboWalkSoundSize = Create(500305, false, CustomOptionType.Modifier, "JumboWalkSoundSize", rates, JumboOption);

        // SetupModifierCustomOptions

        RoleBaseHelper.SetUpOptions();

        // 表示設定

        /* |: ========================= Roles Settings ========================== :| */

        MatchTagOption.LoadOption(); // マッチタグの設定

        Logger.Info("---------- CustomOption Id Info start ----------", "CustomOptionId Info");

        Logger.Info("設定数:" + options.Count);

        Logger.Info("---------- SettingRoleId Info----------", "SettingRoleId Info");
        Logger.Info($"SettingRoleIdのMax: 1 - {GetRoleSettingid(GenericIdMax)}", "Generic ");
        Logger.Info($"SettingRoleIdのMax: 2 - {GetRoleSettingid(ImpostorIdMax)}", "Impostor");
        Logger.Info($"SettingRoleIdのMax: 3 - {GetRoleSettingid(NeutralIdMax)}", "Neutral ");
        Logger.Info($"SettingRoleIdのMax: 4 - {GetRoleSettingid(CrewmateIdMax)}", "Crewmate");
        Logger.Info($"SettingRoleIdのMax: 5 - {GetRoleSettingid(ModifierIdMax)}", "Modifier");
        Logger.Info($"SettingRoleIdのMax: 6 - {GetRoleSettingid(MatchingTagIdMax)}", "MatchingTag");

        Logger.Info("---------- CustomOption Id Info End ----------", "CustomOptionId Info");

        CheckOption();

        /*
        string OPTIONDATA = "{";
        foreach (CustomOption opt in CustomOption.options)
        {
            OPTIONDATA += "\"" + opt.id.ToString() + "\":" + "{\"name\":\"" + opt.name + "\",\"selections\":[";
            foreach (object selection in opt.selections)
            {
                OPTIONDATA += "\"" + selection.ToString() + "\",";
            }
            OPTIONDATA = OPTIONDATA.Substring(0, OPTIONDATA.Length - 1);
            OPTIONDATA += "]},";
        }
        OPTIONDATA = OPTIONDATA.Substring(0, OPTIONDATA.Length - 1);
        OPTIONDATA += "}";
        GUIUtility.systemCopyBuffer = OPTIONDATA;
        */
    }

    /// <summary>
    /// 各分類毎の最終設定Idを取得する
    /// </summary>
    /// <param name="maxId">処理したい6桁の設定Id</param>
    /// <returns></returns>
    private static string GetRoleSettingid(int maxId) => $"{maxId / 100}"[1..];

    /// <summary>
    /// CustomOptionの状態をlogに印字する
    /// </summary>
    private static void CheckOption()
    {
        Logger.Info("----------- CustomOption Info start -----------", "CustomOption");

        if (GameOptionsMenuUpdatePatch.HasSealingOption)
        {
            foreach (CustomOption option in options)
            {
                if (option.RoleId is RoleId.DefaultRole or RoleId.None) continue; // 役職以外はスキップ
                if (Roles.Role.RoleInfoManager.GetRoleInfo(option.RoleId) == null) continue; // GetOptionInfoでlogを出さない様 RoleBase未移行役は先にスキップする。

                OptionInfo optionInfo = OptionInfo.GetOptionInfo(option.RoleId);
                if (optionInfo == null) continue;
                if (!optionInfo.HasSealingCondition || optionInfo.IsHidden) continue;

                Logger.Info($"解放済の封印処理が残っています。 CustomOption Id => {option.id}", "Sealing");
            }
        }

        Logger.Info("----------- CustomOption Info End -----------", "CustomOption");
    }
}