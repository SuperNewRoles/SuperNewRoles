using AmongUs.GameOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public class SyncKillCoolTimeAbility : AbilityBase
{
    public CustomButtonBase Button { get; }
    public SyncKillCoolTimeAbility(CustomButtonBase button)
    {
        Button = button;
    }
    private EventListener<MurderEventData> murderEvent;
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        Button.OnClickEventAction += () =>
        {
            Player.ResetKillCooldown();
        };
        murderEvent = MurderEvent.Instance.AddListener(OnMurder);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        Button.OnClickEventAction = null;
        murderEvent?.RemoveListener();
    }
    private void OnMurder(MurderEventData data)
    {
        if (data.killer == Player && Player.AmOwner)
        {
            Button?.ResetTimer();
        }
    }
    public static SyncKillCoolTimeAbility CreateAndAttach(CustomButtonBase button)
    {
        var ability = new SyncKillCoolTimeAbility(button);
        button.Player.AttachAbility(ability, new AbilityParentAbility(button));
        return ability;
    }
}
