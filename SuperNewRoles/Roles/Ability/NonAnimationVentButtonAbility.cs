using System;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class NonAnimationVentButtonAbility : CustomVentAbility
{
    public NonAnimationVentButtonAbility(Func<bool> canUseVent, Func<float?> ventCooldown = null, Func<float?> ventDuration = null) : base(canUseVent, ventCooldown, ventDuration)
    {
    }
    public override void OnClick()
    {
        if (PlayerControl.LocalPlayer.inVent)
        {
            if (Vent.currentVent == null && PlayerControl.LocalPlayer.Visible)
                return;
            float num = Vector2.Distance(PlayerControl.LocalPlayer.Collider.bounds.center, Vent.currentVent.transform.position);
            // ベントに入っている途中に出れないように
            if (num < 10000)
            {
                ExitVent();
            }
            return;
        }
        CurrentVent = SetVentTarget();
        if (CurrentVent != null)
        {
            // ベントの使用処理を実装
            CurrentVent.SetButtons(true);
            RpcNonAnimationEnterVent(CurrentVent.Id);
        }
    }
    protected override void ExitVent()
    {
        if (Vent.currentVent == null) return;
        Vent.currentVent.SetButtons(false);
        RpcNonAnimationExitVent(Vent.currentVent.Id);
    }
    [CustomRPC]
    public void RpcNonAnimationEnterVent(int id)
    {
        Vent inVent = ModHelpers.VentById(id);
        if (inVent == null) return;

        if (Player.AmOwner)
            Vent.currentVent = inVent;
        Player.Player.moveable = false;
        Player.Player.Visible = false;
        Player.Player.inVent = true;
        Player.NetTransform.SnapTo(inVent.transform.position);
    }
    [CustomRPC]
    public void RpcNonAnimationExitVent(int id)
    {
        Vent inVent = ModHelpers.VentById(id);
        if (inVent == null) return;

        if (Player.AmOwner)
            Vent.currentVent = null;
        Player.Player.moveable = true;
        Player.Player.Visible = true;
        Player.Player.inVent = false;
    }
}