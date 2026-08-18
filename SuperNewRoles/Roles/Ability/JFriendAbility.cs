using AmongUs.GameOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.Ability;

public record JFriendData(bool CanUseVent, bool IsImpostorVision, bool CouldKnowJackals, int TaskNeeded, TaskOptionData SpecialTasks);
public class JFriendAbility : AbilityBase
{
    private readonly int _priority;

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    public CustomTaskAbility CustomTaskAbility { get; private set; }
    private readonly JFriendData Data;
    private bool _canKnowJackal;
    public JFriendAbility(JFriendData data, int priority = AbilityPriority.Default)
    {
        Data = data;
        _priority = priority;
    }

    public override void AttachToAlls()
    {
        VentAbility = new CustomVentAbility(
            () => Data.CanUseVent,
            priority: _priority
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => CanKnowJackal() && player.IsJackalTeam(),
            () => true
        );
        ImpostorVisionAbility = new ImpostorVisionAbility(
            () => Data.IsImpostorVision,
            priority: _priority
        );
        CustomTaskAbility = new CustomTaskAbility(
            isTaskTrigger: () => true,
            countsForCrewWin: () => false,
            requiredTaskCount: () => Data.TaskNeeded,
            taskOptions: () => Data.SpecialTasks,
            priority: _priority
        );

        SubscribeWithAbility(TaskCompleteEvent.Instance, x => RecalucateTaskComplete(x.player));
        RecalucateTaskComplete(Player);

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(VentAbility, parentAbility);
        Player.AttachAbility(KnowJackalAbility, parentAbility);
        Player.AttachAbility(ImpostorVisionAbility, parentAbility);
        Player.AttachAbility(CustomTaskAbility, parentAbility);
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
