using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class ExitVentOnFixSabotageAbility : AbilityBase
{
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(SaboEndEvent.Instance, OnSaboEnd);
    }
    private void OnSaboEnd(SaboEndEventData data)
    {
        Logger.Info("Sabo end");
        if (!PlayerControl.LocalPlayer.inVent || Vent.currentVent == null) return;
        Vent.currentVent.SetButtons(false);
        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
    }
}
