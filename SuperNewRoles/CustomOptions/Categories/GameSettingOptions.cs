using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.CustomOptions.Categories;

public static class GameSettingOptions
{
    [CustomOptionBool("CannotTaskTrigger", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool CannotTaskTrigger;
    [CustomOptionBool("CursedTaskOption", false, parentFieldName: nameof(Categories.GameSettings), displayMode: DisplayModeId.Default)]
    public static bool CursedTaskOption;
    [CustomOptionBool("ExcludeSpecificTasksFromSelection", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool ExcludeSpecificTasksFromSelection;
    [CustomOptionBool("ExcludeUnlockSafeTaskFromSelection", false, parentFieldName: nameof(ExcludeSpecificTasksFromSelection))]
    public static bool ExcludeUnlockSafeTaskFromSelection;
    [CustomOptionBool("ExcludeResetBreakersTaskFromSelection", false, parentFieldName: nameof(ExcludeSpecificTasksFromSelection))]
    public static bool ExcludeResetBreakersTaskFromSelection;
    [CustomOptionBool("ExcludeCatchFishTaskFromSelection", false, parentFieldName: nameof(ExcludeSpecificTasksFromSelection))]
    public static bool ExcludeCatchFishTaskFromSelection;
    [CustomOptionBool("ExcludeUploadDataTaskFromSelection", false, parentFieldName: nameof(ExcludeSpecificTasksFromSelection))]
    public static bool ExcludeUploadDataTaskFromSelection;
    [CustomOptionBool("ExcludeVentCleaningTaskFromSelection", false, parentFieldName: nameof(ExcludeSpecificTasksFromSelection))]
    public static bool ExcludeVentCleaningTaskFromSelection;
    [CustomOptionBool("ExcludeSubmitScanTaskFromSelection", false, parentFieldName: nameof(ExcludeSpecificTasksFromSelection))]
    public static bool ExcludeSubmitScanTaskFromSelection;
    [CustomOptionBool("RandomizeFixLightsSwitches", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool RandomizeFixLightsSwitches;
    // |:========== ベントアニメーション有効化の設定 ==========:|
    [CustomOptionBool("VentAnimationPlaySetting", true, parentFieldName: nameof(Categories.GameSettings))]
    public static bool VentAnimationPlaySetting;
    [CustomOptionBool("SyncSpawn", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool SyncSpawn;
    [CustomOptionBool("DisableTaskWin", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool DisableTaskWin;
    [CustomOptionBool("DisableHijackTaskWin", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool DisableHijackTaskWin;
    [CustomOptionBool("HideGhostRoles", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool HideGhostRoles;
    [CustomOptionBool("ShowGhostRolesToImpostor", false, parentFieldName: nameof(HideGhostRoles))]
    public static bool ShowGhostRolesToImpostor;
    [CustomOptionSelect("GhostVoteDisplay", typeof(GhostVoteDisplayType), "GhostVoteDisplayType.", parentFieldName: nameof(Categories.GameSettings), defaultValue: GhostVoteDisplayType.Vanilla)]
    public static GhostVoteDisplayType GhostVoteDisplay;

    [CustomOptionBool("ChangeReportDistance", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool ChangeReportDistance;
    [CustomOptionFloat("ReportDistanceOption", 0.4f, 10f, 0.2f, 5f, parentFieldName: nameof(ChangeReportDistance))]
    public static float ReportDistanceOption;

    [CustomOptionBool("IsLimitEmergencyMeeting", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool IsLimitEmergencyMeeting;
    [CustomOptionInt("EmergencyMeetingLimitCount", 0, 20, 1, 10, parentFieldName: nameof(IsLimitEmergencyMeeting))]
    public static int EmergencyMeetingLimitCount;

    [CustomOptionBool("CustomAprilFools", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool CustomAprilFools;
    [CustomOptionSelect("AprilFoolsOutfitType", typeof(AprilFoolsOutfitType), "AprilFoolsOutfitType.", parentFieldName: nameof(CustomAprilFools))]
    public static AprilFoolsOutfitType AprilFoolsOutfitType;
    [CustomOptionBool("AprilFoolsEnableDleks", false, parentFieldName: nameof(CustomAprilFools))]
    public static bool AprilFoolsEnableDleks;

    // 死亡後の手動ズーム全体を有効化する親設定。
    [CustomOptionBool("EnabledZoomOnDead", true, parentFieldName: nameof(Categories.GameSettings))]
    public static bool EnabledZoomOnDead;
    // タスク進捗での使用制限。親設定がOFFなら UI 上もロジック上も無効。
    [CustomOptionBool("EnabledZoomOnDeadRequireCompletedTasks", false, parentFieldName: nameof(EnabledZoomOnDead))]
    public static bool EnabledZoomOnDeadRequireCompletedTasks;
    // 幽霊役職と GuardianAngel に対する使用制限。こちらも親設定配下の子設定。
    [CustomOptionBool("EnabledZoomOnDeadDisableForGhostOrGuardianAngel", false, parentFieldName: nameof(EnabledZoomOnDead))]
    public static bool EnabledZoomOnDeadDisableForGhostOrGuardianAngel;

    [CustomOptionSelect("InitialCooldown", typeof(InitialCooldownType), "InitialCooldownType.", parentFieldName: nameof(Categories.GameSettings), defaultValue: InitialCooldownType.TenSeconds)]
    public static InitialCooldownType InitialCooldown;

    [CustomOptionBool("DisableHauntNonCompleted", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool DisableHauntNonCompleted;
}
public enum InitialCooldownType
{
    TenSeconds,
    Immediate,
    OneThird,
}
public enum AprilFoolsOutfitType
{
    None,

}

public enum GhostVoteDisplayType
{
    Vanilla,
    Hide,
    Show,
}
