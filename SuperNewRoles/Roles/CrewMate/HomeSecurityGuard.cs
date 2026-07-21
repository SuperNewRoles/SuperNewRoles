using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.CrewMate;

class HomeSecurityGuard : RoleBase<HomeSecurityGuard>
{
    public override RoleId Role { get; } = RoleId.HomeSecurityGuard;
    public override Color32 RoleColor { get; } = new(0, 255, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } =
    [
        () => new CustomTaskAbility(
            () => (false, false, 0),
            new TaskOptionData(0, 0, 0)
        ),
        () => new HomeSecurityGuardAbility(),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

public class HomeSecurityGuardAbility : AbilityBase
{
    // タスク一覧に表示するランダムテキストの最大番号
    public const int TaskTextMax = 12;
    private ImportantTextTask _task;
    private EventListener<MeetingCloseEventData> _meetingCloseListener;
    private int _currentIndex = -1;

    public override void AttachToLocalPlayer()
    {
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        EnsureTask();
        UpdateIndexAndTaskText();
    }

    public override void DetachToLocalPlayer()
    {
        _meetingCloseListener?.RemoveListener();
        DestroyTask();
    }

    private void OnMeetingClose(MeetingCloseEventData data)
    {
        if (!Player.AmOwner) return;
        if (Player.Data.IsDead) return;
        UpdateIndexAndTaskText();
    }

    private void UpdateIndexAndTaskText()
    {
        if (!Player.AmOwner) return;
        // 現在以外の番号からランダムに選ぶ
        int next;
        do
        {
            next = ModHelpers.GetRandomInt(TaskTextMax - 1);
        } while (next == _currentIndex && TaskTextMax > 1);
        _currentIndex = next;
        EnsureTask();
        if (_task == null) return;

        string text = ModTranslation.GetString($"HomeSecurityGuardTask{_currentIndex + 1}");
        _task.Text = ModHelpers.Cs(HomeSecurityGuard.Instance.RoleColor, text);
    }

    private void EnsureTask()
    {
        if (!Player.AmOwner) return;
        if (_task != null) return;

        var pc = Player?.Player;
        if (pc == null) return;

        var taskObj = new GameObject("HomeSecurityGuardTask").AddComponent<ImportantTextTask>();
        taskObj.transform.SetParent(pc.transform, false);
        pc.myTasks.Insert(0, taskObj);
        taskObj.HasLocation = false;
        _task = taskObj;
    }

    private void DestroyTask()
    {
        if (_task == null) return;
        var pc = Player?.Player;
        if (pc != null) pc.myTasks.Remove(_task);
        UnityEngine.Object.Destroy(_task.gameObject);
        _task = null;
    }
}
