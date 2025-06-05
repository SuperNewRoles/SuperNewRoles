using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class ExitVentOnFixSabotageAbility : AbilityBase
{
    private EventListener<SaboEndEventData> saboEndEventListener;
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        saboEndEventListener = SaboEndEvent.Instance.AddListener(OnSaboEnd);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        saboEndEventListener?.RemoveListener();
    }

    private void OnSaboEnd(SaboEndEventData data)
    {
        if (!PlayerControl.LocalPlayer.inVent || Vent.currentVent == null) return;
        Vent.currentVent.SetButtons(false);
        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
    }
}