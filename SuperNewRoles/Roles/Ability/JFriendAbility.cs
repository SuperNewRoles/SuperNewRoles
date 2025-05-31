using AmongUs.GameOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.Ability;

public record JFriendData(bool CanUseVent, bool IsImpostorVision, bool CouldKnowJackals, int TaskNeeded, TaskOptionData SpecialTasks);
public class JFriendAbility : AbilityBase
{

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    public CustomTaskAbility CustomTaskAbility { get; private set; }
    private EventListener<TaskCompleteEventData> _taskCompleteEvent;
    private readonly JFriendData Data;
    private bool _canKnowJackal;
    public JFriendAbility(JFriendData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        VentAbility = new CustomVentAbility(
            () => Data.CanUseVent
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => CanKnowJackal() && player.IsJackalTeam(),
            () => true
        );
        ImpostorVisionAbility = new ImpostorVisionAbility(
            () => Data.IsImpostorVision
        );
        CustomTaskAbility = new CustomTaskAbility(
            () => (true, false, Data.TaskNeeded),
            Data.SpecialTasks
        );

        _taskCompleteEvent = TaskCompleteEvent.Instance.AddListener(x => RecalucateTaskComplete(x.player));
        RecalucateTaskComplete(Player);

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(VentAbility, parentAbility);
        Player.AttachAbility(KnowJackalAbility, parentAbility);
        Player.AttachAbility(ImpostorVisionAbility, parentAbility);
        Player.AttachAbility(CustomTaskAbility, parentAbility);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _taskCompleteEvent?.RemoveListener();
    }
    private bool CanKnowJackal()
    {
        return _canKnowJackal;
    }
    public override void AttachToLocalPlayer()
    {
    }
    private void RecalucateTaskComplete(PlayerControl player)
    {
        if (!Player.AmOwner) return;
        if (player != Player) return;
        if (!Data.CouldKnowJackals) _canKnowJackal = false;
        else
        {
            bool last = _canKnowJackal;
            var (complete, all) = ModHelpers.TaskCompletedData(player.Data);
            if (complete == -1 || all == -1) _canKnowJackal = false;
            else _canKnowJackal = complete >= Data.TaskNeeded;
            if (last != _canKnowJackal)
                NameText.UpdateAllNameInfo();
        }
    }
}