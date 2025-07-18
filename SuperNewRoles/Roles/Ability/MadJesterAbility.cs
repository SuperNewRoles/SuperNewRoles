using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Roles.Ability;
public class MadJesterAbility : AbilityBase
{
    private readonly MadJesterData _data;
    private EventListener<ExileEventData> _playerExiledListener;
    private EventListener<TaskCompleteEventData> _taskCompleteListener;
    private MadmateAbility _madmateAbility;

    public MadJesterAbility(MadJesterData data)
    {
        _data = data;
    }

    public override void AttachToAlls()
    {
        var madmateData = new MadmateData(
            _data.HasImpostorVision,
            _data.CanUseVent,
            _data.CanKnowImpostors,
            _data.RequiredTaskCount,
            _data.IsSpecialTasks ? _data.CustomTasks : null
        );
        _madmateAbility = new MadmateAbility(madmateData);

        Player.AttachAbility(_madmateAbility, new AbilityParentAbility(this));

        _playerExiledListener = ExileEvent.Instance.AddListener(OnPlayerExiled);
        if (_data.WinOnTaskComplete)
        {
            _taskCompleteListener = TaskCompleteEvent.Instance.AddListener(OnTaskComplete);
        }
    }

    private void OnPlayerExiled(ExileEventData data)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (data.exiled == null || data.exiled.PlayerId != Player.PlayerId) return;

        EndGamer.RpcEndGameImpostorWin();
    }

    private void OnTaskComplete(TaskCompleteEventData data)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (data.player.PlayerId != Player.PlayerId) return;

        var (tasksCompleted, tasksTotal) = ModHelpers.TaskCompletedData(Player.Data);
        if (tasksTotal > 0 && tasksCompleted >= tasksTotal)
        {
            EndGamer.RpcEndGameImpostorWin();
            _taskCompleteListener?.RemoveListener();
        }
    }

    public override void DetachToAlls()
    {
        _playerExiledListener?.RemoveListener();
        _taskCompleteListener?.RemoveListener();
    }
}

public class MadJesterData
{
    public bool CanUseVent { get; }
    public bool HasImpostorVision { get; }
    public bool CanKnowImpostors { get; }
    public int RequiredTaskCount { get; }
    public bool IsSpecialTasks { get; }
    public TaskOptionData CustomTasks { get; }
    public bool WinOnTaskComplete { get; }

    public MadJesterData(bool canUseVent, bool hasImpostorVision, bool canKnowImpostors, int requiredTaskCount, bool isSpecialTasks, TaskOptionData customTasks, bool winOnTaskComplete)
    {
        CanUseVent = canUseVent;
        HasImpostorVision = hasImpostorVision;
        CanKnowImpostors = canKnowImpostors;
        RequiredTaskCount = requiredTaskCount;
        IsSpecialTasks = isSpecialTasks;
        CustomTasks = customTasks;
        WinOnTaskComplete = winOnTaskComplete;
    }
}