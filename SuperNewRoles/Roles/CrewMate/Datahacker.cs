using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.CrewMate;

/// <summary>
/// データハッカー役職のメインクラス
/// </summary>
class Datahacker : RoleBase<Datahacker>
{
    public override RoleId Role { get; } = RoleId.Datahacker;
    public override Color32 RoleColor { get; } = new(157, 236, 255, byte.MaxValue); // 水色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new DatahackerAbility(new(
        UseIndividualTaskSetting: UseIndividualTaskSetting,
        IndividualTasks: IndividualTasks,
        TaskRequirePercent: TaskRequirePercent,
        ExposeTasksLeft: ExposeTasksLeft,
        ShowArrowWhenExposed: ShowArrowWhenExposed,
        CanSeeDuringMeeting: CanSeeDuringMeeting,
        CanSeeImpostor: CanSeeImpostor,
        CanSeeNeutral: CanSeeNeutral,
        CanSeeKillingNeutral: CanSeeKillingNeutral,
        CanSeeCrew: CanSeeCrew,
        CanSeeMadmates: CanSeeMadmates,
        CanSeeRoleNames: CanSeeRoleNames
    ))];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    // タスクを個別で設定するか
    [CustomOptionBool("Datahacker.TaskOption", true)]
    public static bool UseIndividualTaskSetting = true;

    [CustomOptionTask("Datahacker.TaskRequirePercent", 4, 4, 4, parentFieldName: nameof(UseIndividualTaskSetting))]
    public static TaskOptionData IndividualTasks;

    // 能力が発動するタスク進捗
    [CustomOptionFloat("Datahacker.TaskRequirePercent", 0f, 100f, 5f, 100f)]
    public static float TaskRequirePercent = 100f;

    // 人外にバレる残りタスク数
    [CustomOptionInt("Datahacker.ExposeTasksLeft", 0, 20, 1, 2)]
    public static int ExposeTasksLeft = 2;

    // バレた際、矢印が表示されるか
    [CustomOptionBool("Datahacker.ShowArrowWhenExposed", true)]
    public static bool ShowArrowWhenExposed = true;

    // 会議時に役職を見れるか
    [CustomOptionBool("Datahacker.CanSeeDuringMeeting", false)]
    public static bool CanSeeDuringMeeting = false;

    // インポスターがわかる
    [CustomOptionBool("Datahacker.CanSeeImpostor", true)]
    public static bool CanSeeImpostor = true;

    // 第三陣営がわかる
    [CustomOptionBool("Datahacker.CanSeeNeutral", false)]
    public static bool CanSeeNeutral = false;

    // キル第三だけがわかる
    [CustomOptionBool("Datahacker.CanSeeKillingNeutral", true, parentFieldName: nameof(CanSeeNeutral))]
    public static bool CanSeeKillingNeutral = true;

    // クルーメイトがわかる
    [CustomOptionBool("Datahacker.CanSeeCrew", false)]
    public static bool CanSeeCrew = false;

    // マッド系役職がわかる
    [CustomOptionBool("Datahacker.CanSeeMadmates", false, parentFieldName: nameof(CanSeeCrew))]
    public static bool CanSeeMadmates = false;

    // 役職名もわかる
    [CustomOptionBool("Datahacker.CanSeeRoleNames", false)]
    public static bool CanSeeRoleNames = false;
}

public record DatahackerData(
    bool UseIndividualTaskSetting,
    TaskOptionData IndividualTasks,
    float TaskRequirePercent,
    int ExposeTasksLeft,
    bool ShowArrowWhenExposed,
    bool CanSeeDuringMeeting,
    bool CanSeeImpostor,
    bool CanSeeNeutral,
    bool CanSeeKillingNeutral,
    bool CanSeeCrew,
    bool CanSeeMadmates,
    bool CanSeeRoleNames);

/// <summary>
/// データハッカーの能力クラス
/// </summary>
public class DatahackerAbility : AbilityBase
{
    // 矢印関連
    private Arrow arrow;
    private EventListener fixedUpdateListener;
    private EventListener<TaskCompleteEventData> taskCompleteListener;
    private EventListener<NameTextUpdateEventData> nameTextUpdateListener;
    private EventListener<NameTextUpdateVisiableEventData> nameTextUpdateVisiableListener;
    private CustomTaskAbility customTaskAbility;

    private bool exposedToImpostors = false;
    private bool hackingCompleted = false;

    private DatahackerData hackingData;

    public DatahackerAbility(DatahackerData hackingData) : base()
    {
        this.hackingData = hackingData;
    }

    public override void AttachToLocalPlayer()
    {
        nameTextUpdateVisiableListener = NameTextUpdateVisiableEvent.Instance.AddListener((data) => UpdateNameVisible(data));
    }

    public override void DetachToLocalPlayer()
    {
        nameTextUpdateVisiableListener?.RemoveListener();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        customTaskAbility = new CustomTaskAbility(() => (true, true, null), hackingData.UseIndividualTaskSetting ? hackingData.IndividualTasks : null);
        Player.AttachAbility(customTaskAbility, new AbilityParentAbility(this));

        taskCompleteListener = TaskCompleteEvent.Instance.AddListener((data) => TaskCompleted(data));
        fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(() => UpdateArrow());
        nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener((data) => UpdateNameText(data));
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        fixedUpdateListener?.RemoveListener();
        taskCompleteListener?.RemoveListener();
        nameTextUpdateListener?.RemoveListener();
        nameTextUpdateVisiableListener?.RemoveListener();
        if (arrow != null && arrow.arrow != null)
            UnityEngine.Object.Destroy(arrow.arrow);
        arrow = null;
    }

    private void UpdateNameText(NameTextUpdateEventData data)
    {
        if (data.Player == Player && !Player.AmOwner && ExPlayerControl.LocalPlayer.IsNonCrewKiller() && exposedToImpostors)
            NameText.SetNameTextColor(data.Player, Datahacker.Instance.RoleColor);
        else if (Player.AmOwner && hackingCompleted)
        {
            if (MeetingHud.Instance != null && !hackingData.CanSeeDuringMeeting) return;
            if (CanSeeRole(data.Player, out Color32 color))
                NameText.SetNameTextColor(data.Player, color);
        }
    }

    private void UpdateNameVisible(NameTextUpdateVisiableEventData data)
    {
        if (Player.AmOwner && hackingCompleted && hackingData.CanSeeRoleNames)
        {
            if (CanSeeRole(data.Player, out _))
                NameText.UpdateVisiable(data.Player, data.Player.IsAlive());
        }
    }

    /// <summary>
    /// タスク進捗に応じた発動条件チェック
    /// </summary>
    public (bool exposedToImpostors, bool hackingCompleted) CanSeeOtherPlayer()
    {
        if (Player.IsDead()) return (false, false);
        // データハッカーはタスク進捗で能力発動の計算
        var (taskCompletedCount, totalTasks) = ModHelpers.TaskCompletedData(Player.Data);

        // タスク進捗が0の場合は発動しない
        if (totalTasks == 0) return (false, false);

        // タスク進捗率を計算
        float progress = (float)taskCompletedCount / totalTasks * 100f;

        // 能力発動残りタスク数（人外にバレるタイミング）を計算
        int exposeTaskThreshold = Math.Max(0, (int)Math.Ceiling(totalTasks * hackingData.TaskRequirePercent / 100f) - hackingData.ExposeTasksLeft);

        return (taskCompletedCount >= exposeTaskThreshold, progress >= hackingData.TaskRequirePercent);
    }

    /// <summary>
    /// プレイヤー情報の更新処理
    /// </summary>
    private void TaskCompleted(TaskCompleteEventData data)
    {
        if (data.player != Player) return;
        bool oldhackingCompleted = hackingCompleted;
        (exposedToImpostors, hackingCompleted) = CanSeeOtherPlayer();
        if (exposedToImpostors && arrow == null)
        {
            arrow = new Arrow(Datahacker.Instance.RoleColor);
            arrow.arrow.SetActive(false);
            NameText.UpdateNameInfo(Player);
        }
        if (hackingCompleted && !oldhackingCompleted)
            NameText.UpdateAllNameInfo();
    }

    /// <summary>
    /// 矢印の更新処理
    /// </summary>
    private void UpdateArrow()
    {
        if (!hackingData.ShowArrowWhenExposed || !ExPlayerControl.LocalPlayer.IsNonCrewKiller() || !exposedToImpostors || Player.IsDead())
        {
            if (arrow != null && arrow.arrow != null && arrow.arrow.activeSelf)
                arrow.arrow.SetActive(false);
        }
        else
        {
            if (arrow == null || arrow.arrow == null || !arrow.arrow.activeSelf)
                arrow.arrow.SetActive(true);
            arrow.Update(Player.transform.position);
        }
    }
    private bool CanSeeRole(ExPlayerControl target, out Color32 color)
    {
        color = Color.white;
        Logger.Info($"CanSeeRole: {target.Data.PlayerName}");
        if (target.IsNeutral())
        {
            if (!hackingData.CanSeeNeutral)
                return false;
            Logger.Info("Neutral");
            color = new(127, 127, 127, byte.MaxValue);
            return hackingData.CanSeeKillingNeutral ? target.IsKiller() : true;
        }
        if (target.IsImpostor())
        {
            if (!hackingData.CanSeeImpostor)
                return false;
            Logger.Info("Impostor");
            color = Palette.ImpostorRed;
            return true;
        }
        else
        {
            if (!hackingData.CanSeeCrew)
                return false;
            Logger.Info("Crewmate");
            if ((target.IsMadRoles() || target.IsFriendRoles()) && hackingData.CanSeeMadmates)
            {
                Logger.Info("Madmates");
                color = Palette.ImpostorRed;
                return true;
            }
            else
            {
                Logger.Info("Crewmate (Mad as Crew)");
                color = target.Data.Role.TeamColor;
                return true;
            }
        }
    }
}